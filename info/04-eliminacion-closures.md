# Incidente 4: Eliminación de closures/lambdas en loops UI

**Fecha:** 26 de octubre de 2026  
**Prioridad:** ⚠️ Media  
**Dificultad:** Media  
**Estado:** ✅ Completado

---

## Resumen

Las lambdas anónimas (`() => ...`) dentro de loops `for` capturan variables y crean **clase anónima + delegate instance** por iteración. En UI dinámica esto se multiplica por cada botón creado durante `Build()`:

- **AddClickSound**: closure por cada botón (≈40+ botones en toda la UI)
- **BuildKeyboard** (GameView): closure por cada tecla (28 teclas QWERTY + Ñ)
- **BuildLanguageButtons** (MainMenuView): closure por cada idioma (4 botones)

**Todas eliminadas** con métodos nombrados estáticos reutilizados y componentes handler lightweight.

---

## Cambio 1: AddClickSound — delegate estático readonly (cero allocations)

### Problema

```csharp
private static void AddClickSound(Button button)
{
    button.onClick.AddListener(() =>   // ← closure/clase anónima POR CADA BOTÓN
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayClick();
    });
}
```

`AddClickSound` se llama en **cada** `CreateButton` y `CreateCenteredButton`. Con 40+ botones, genera 40+ closures (clase anónima + delegate instance).

### Solución

```csharp
// Delegate único reutilizado por todos los botones → cero allocations.
private static readonly UnityAction _playClickAction = PlayClick;

private static void PlayClick()
{
    if (AudioManager.Instance != null)
        AudioManager.Instance.PlayClick();
}

private static void AddClickSound(Button button)
{
    button.onClick.AddListener(_playClickAction);
}
```

El `UnityAction` se crea **una sola vez** (static readonly) y se reutiliza. Cero allocations por botón.

---

## Cambio 2: GameView.BuildKeyboard — KeyButtonHandler

### Problema

```csharp
for (int i = 0; i < keys.Length; i++)
{
    char letter = keys[i];
    // ...
    Button button = UIUtils.CreateCenteredButton(..., () =>
    {
        PressKey(letter);   // ← closure que capta 'letter'
    });
}
```

28 closures creadas al construir el teclado.

### Solución

**`KeyButtonHandler.cs`** (nuevo componente):
```csharp
internal sealed class KeyButtonHandler : MonoBehaviour
{
    private char _key;
    public void SetKey(char key) => _key = char.ToUpperInvariant(key);
    public void OnClick() => HangmanGameManager.Instance?.Guess(_key);
}
```

**`BuildKeyboard`:**
```csharp
Button button = UIUtils.CreateCenteredButton(..., null);
var keyHandler = button.gameObject.AddComponent<KeyButtonHandler>();
keyHandler.SetKey(letter);
button.onClick.AddListener(keyHandler.OnClick);
```

Se eliminan 28 closures. El callback `keyHandler.OnClick` es un método nombrado (no captura variables del loop).

---

## Cambio 3: MainMenuView.BuildLanguageButtons — LanguageButtonHandler

### Problema

```csharp
for (int i = 0; i < codes.Length; i++)
{
    int index = i;
    Button button = UIUtils.CreateCenteredButton(..., () => SelectLanguage(languages[index]));
    // ← closure que capta 'index'
}
```

4 closures. `SelectLanguage` era código muerto (solo referenciado por la closure).

### Solución

**`LanguageButtonHandler.cs`** (nuevo componente):
```csharp
internal sealed class LanguageButtonHandler : MonoBehaviour
{
    private string _languageCode;
    public void SetLanguage(string code) => _languageCode = code;
    public void OnClick()
    {
        if (LocalizationManager.Instance.SetLanguage(_languageCode) && UIManager.Instance != null)
            UIManager.Instance.RebuildAllViews();
    }
}
```

`BuildLanguageButtons`:
```csharp
Button button = UIUtils.CreateCenteredButton(..., null);
var langHandler = button.gameObject.AddComponent<LanguageButtonHandler>();
langHandler.SetLanguage(languages[i]);
button.onClick.AddListener(langHandler.OnClick);
```

Se eliminan 4 closures. Método `SelectLanguage` eliminado de MainMenuView (código muerto).

---

## Detalles de implementación

### UIUtils — `onClick` opcional

Para permitir pasar `null` (el handler añade su propio callback):

```csharp
public static Button CreateCenteredButton(string name, ..., UnityAction onClick = null)
{
    var button = rect.gameObject.AddComponent<Button>();
    if (onClick != null)
    {
        button.onClick.AddListener(onClick);
    }
    AddClickSound(button);  // siempre añade click sound (delegate reutilizado)
    ...
}
```

### Nota sobre delegates nombrados vs closures

- **Closure** (lambda capturando variable): crea clase anónima + delegate instance por uso → 2 allocations.
- **Método nombrado** (`AddListener(handler.OnClick)`): crea delegate instance pero **no** clase anónima → 1 allocation (delegate).
- **Delegate readonly estático** (`AddListener(_playClickAction)`): **cero** allocations → usado para todos los botones en `AddClickSound`.

Los handlers (`KeyButtonHandler`, `LanguageButtonHandler`) usan métodos nombrados → 1 delegate allocation por botón **pero** durante `Build()` (una vez), no en runtime. Y críticamente, no capturan variables del loop (no hay closure class).

---

## Verificación

- **Static analysis**: `AudioManager.PlayClick()`, `HangmanGameManager.Instance.Guess(char)`, `LocalizationManager.SetLanguage()`, `UIManager.RebuildAllViews()` — todos verificados como existentes.
- **No references huérras**: `grep PressKey` → solo en comentarios. `grep SelectLanguage` → solo en comentarios + definición eliminada.
- **Firmas compatibles**: `KeyButtonHandler.OnClick` y `LanguageButtonHandler.OnClick` son `void()` → compatibles con `UnityAction` que `AddListener` acepta.
- **UIUtils optional param**: todos los llamadores existentes que pasan callbacks nombrados (`ShowHint`, `() => _pauseManager.TogglePause()`, etc.) siguen funcionando; solo los nuevos usan `null`.

## Archivos modificados

| Archivo | Cambio |
|---------|--------|
| `UI/UIUtils.cs` | `AddClickSound`: lambda → método estático + delegate readonly. `CreateButton`/`CreateCenteredButton`: `onClick = null` opcional |
| `UI/GameView.cs` | `BuildKeyboard`: closure → `KeyButtonHandler`. Eliminado `PressKey` |
| `UI/MainMenuView.cs` | `BuildLanguageButtons`: closure → `LanguageButtonHandler`. Eliminado `SelectLanguage` (código muerto) |
| `UI/KeyButtonHandler.cs` | **Nuevo**: handler para teclado |
| `UI/LanguageButtonHandler.cs` | **Nuevo**: handler para botones de idioma |
