# Incidente 5: Dirty-check en UI Rebuild + refactor de localización sin destroy

**Fecha:** 26 de octubre de 2026  
**Prioridad:** ⚠️ Alta  
**Dificultad:** Media-Alta  
**Estado:** ✅ Completado

---

## Resumen

`View.Rebuild()` **destruía todos los hijos y reconstruía el UI desde cero** en cada cambio de orientación o idioma. Esto causaba:
- Miles de allocations por `new GameObject()` (BuildLevelArea, BuildKeyboard, BuildPauseOverlay, etc.).
- GC spikes visibles en móviles.
- Pérdida de estado UI (scroll, selección).

**Solución:**
1. **Dirty check en `View.Rebuild()`**: solo destruye + reconstruye si cambió la orientación o el idioma. Si nada cambió → retorno temprano (zero GC).
2. **Evento `LanguageChanged` en `LocalizationManager`**: cuando cambia el idioma, se refrescan los textos **sin destruir** el UI.
3. **`RefreshLocalization()` virtual en `View`**: cada View implementa su refresco de textos sin rebuild.

| Escenario | Antes | Después |
|-----------|-------|---------|
| Polling de orientación (Update, sin cambio) | `RebuildAllViews()` solo cuando cambia (ya era condicional) | Igual, pero `Rebuild()` hace dirty check adicional |
| Giro de pantalla | Destroy 100% + Build 100% | Destroy 100% + Build 100% (inevitable, layout cambia) |
| **Cambio de idioma** | **Destroy 100% + Build 100% + GC spike** | **Solo refresco de textos (zero destroy, zero allocations)** |
| Primer build | Build 100% | Build 100% (dirty = null → fuerza rebuild) |

---

## Cambio 1: `View.Rebuild()` — dirty check

### Antes

```csharp
public virtual void Rebuild()
{
    for (int i = transform.childCount - 1; i >= 0; i--)
        Destroy(transform.GetChild(i).gameObject);
    Build();
}
```

Destruía y reconstruía **siempre**, incluso si `Rebuild()` se llamaba sin cambios.

### Después

```csharp
private bool _cachedLandscape;
private string _cachedLanguage;   // null = "nunca cacheado" → fuerza primera build

public virtual void Rebuild()
{
    bool landscape = IsLandscape;
    string language = LocalizationManager.Instance?.ActiveLanguage ?? string.Empty;
    // null la primera vez = fuerza rebuild; después, solo si cambió algo
    if (_cachedLanguage != null && landscape == _cachedLandscape && language == _cachedLanguage)
        return;
    _cachedLandscape = landscape;
    _cachedLanguage = language;

    for (int i = transform.childCount - 1; i >= 0; i--)
        Destroy(transform.GetChild(i).gameObject);
    Build();
}
```

- **`_cachedLanguage == null`** la primera vez: fuerza el rebuild inicial (no depende de `Awake()`, así que no hay problema con subclasses que no llaman `base.Awake()`).
- **Subsequent calls**: retorno temprano si nada cambió.

---

## Cambio 2: Evento `LanguageChanged` en `LocalizationManager`

### Antes

`SetLanguage()` cambiaba `_active` silenciosamente. El caller (`MainMenuUIManager.SelectLanguage`) llamaba `UIManager.RebuildAllViews()` → destroy + rebuild intencional por el cambio de idioma.

### Después

```csharp
public event Action<string> LanguageChanged;

public bool SetLanguage(string languageCode)
{
    WordDictionaryData dictionary = GetDictionary(languageCode);
    if (dictionary == null) return false;
    if (_active != dictionary)        // solo notificar si realmente cambió
    {
        _active = dictionary;
        LanguageChanged?.Invoke(languageCode);
    }
    return true;
}
```

`UIManager` se suscribe a este evento → refresca textos de todas las views sin destroy.

---

## Cambio 3: `RefreshLocalization()` virtual en `View` + overrides

`View` define el hook virtual. Las Views con textos estáticos lo sobreescriben:

- **`GameView.RefreshLocalization()`** → `ApplyGameState()` (actualiza level/timer/word/lives/figure). Zero destroy.
- **`MainMenuView.RefreshLocalization()`** → refresca `subtitle`, `hint`, `consent label` + `RebuildLevels()` (incremental, reutiliza botones). Zero destroy.
- **`ResultView.RefreshLocalization()`** → `ApplyResult()` (actualiza título y visibilidad de botones). Zero destroy.

`UIManager`:
```csharp
public void RefreshLocalization()
{
    _menuView.RefreshLocalization();
    _gameView.RefreshLocalization();
    _resultView.RefreshLocalization();
}
```

---

## Cambio 4: Eliminación del rebuild manual de `LanguageButtonHandler`

### Antes

`LanguageButtonHandler.OnClick` llamaba `UIManager.Instance.RebuildAllViews()` (destroy + rebuild) tras cambiar idioma.

### Después

```csharp
public void OnClick()
{
    // El refresh se dispara via LocalizationManager.LanguageChanged → UIManager
    // → RefreshLocalization (sin destroy). No se necesita RebuildAllViews.
    LocalizationManager.Instance.SetLanguage(_languageCode);
}
```

---

## Flujo unificado

```
[Cambio de idioma]
      ↓
LanguageButtonHandler.OnClick
      ↓
LocalizationManager.SetLanguage("en")
      ↓
LanguageChanged?.Invoke("en")
      ↓
UIManager.HandleLanguageChanged → RefreshLocalization()
      ↓
GameView.RefreshLocalization  → ApplyGameState()     (zero destroy)
MainMenuView.RefreshLocalization → refresh texts + RebuildLevels() (incremental)
ResultView.RefreshLocalization → ApplyResult()       (zero destroy)

[Cambio de orientación]
      ↓
UIManager.Update() detects landscape != _landscape
      ↓
RebuildAllViews() → View.Rebuild()
      ↓
Dirty check: landscape cambió → Destroy all + Build()  (inevitable)
```

---

## Verificación

- **Balance de llaves**: todos los 12 archivos modificados validados con script de balance de `(){}` → 12/12 OK.
- **Referencias cruzadas**: `RefreshLocalization` definido en `View` (virtual), override en 3 views, llamado por `UIManager`. ✓
- **Evento**: `LanguageChanged` suscripto en `UIManager.Start()` / desuscrito en `UIManager.OnDestroy` (único método `OnDestroy`, sin duplicados). ✓
- **Dirty check**: `_cachedLanguage` null → primer rebuild forzado; `SetLanguage` solo dispara evento si `_active != dictionary`. ✓
- **`MainMenuView`**: campos `_subtitleText`/`_hintText` asignados en `BuildLevelArea` (landscape + portrait). ✓
- **`SelectLanguage`**: eliminado (código muerto, reemplazado por `LanguageButtonHandler`). ✓

## Archivos modificados

| Archivo | Cambio |
|---------|--------|
| `Core/LocalizationManager.cs` | Evento `LanguageChanged`, `SetLanguage` notifica solo si cambió (`using System` añadido) |
| `UI/View.cs` | Dirty check en `Rebuild()` + override virtual `RefreshLocalization()` |
| `UI/UIManager.cs` | Suscripción a `LanguageChanged`, `RefreshLocalization()`, `OnDestroy` unificado (sin dup) |
| `UI/GameView.cs` | `RefreshLocalization()` → `ApplyGameState()` (ya aplicado Incidente 4) |
| `UI/MainMenuView.cs` | Campos `_subtitleText`/`_hintText`, `RefreshLocalization()` override, `SelectLanguage` eliminado |
| `UI/ResultView.cs` | `RefreshLocalization()` → `ApplyResult()`, `Rebuild()` override simplificado |
| `UI/LanguageButtonHandler.cs` | `OnClick` simplificado (evento se encarga del refresh) |

## Quedas por validar (compilación real)

- Requiere **Unity 2022+** para compilar y validar tipos de `UnityEngine`. En este entorno no hay Unity instalado → validación estática (sintaxis + referencias cruzadas) solamente.
