# Incidente 2: Optimización de PlayerPrefs

**Fecha:** 26 de octubre de 2026  
**Prioridad:** ⚠️ Alta  
**Dificultad:** Media  
**Estado:** ✅ Completado y verificado

---

## Problemas identificados

### Problema 1: `PlayerPrefs.Save()` dentro de un getter (LivesManager)

**Antes:**
```csharp
public static int GetLives()
{
    // ... lógica de regeneración ...
    PlayerPrefs.SetInt(LivesKey, newLives);
    PlayerPrefs.Save();           // ← FLUSH A DISCO EN CADA REGENERACIÓN
    // ...
    return newLives;
}
```

`GetLives()` es un getter que **escribe a disco** (`PlayerPrefs.Save()`) cuando regenera una vida. Este getter se llama:
- Cada **1 segundo** en `UIManager.Update()` (polling de vidas)
- En `MainMenuUIManager.OnEnable()`
- En `PauseManager.UpdateLives()`
- En `GameView.Build()`
- En múltiples callbacks de `LivesChanged`

`PlayerPrefs.Save()` es **bloqueante** (flush de XML a disco en cada llamada). En Android, esto puede bloquear el hilo principal varios milisegundos.

### Problema 2: Write redundante en `MainMenuUIManager`

**Antes:**
```csharp
public void UpdateLivesDisplay(int currentLives)
{
    int lives = Mathf.Min(LivesManager.MaxLives, Mathf.Max(0, currentLives));
    PlayerPrefs.SetInt(LivesKey, lives);   // ← REDUNDANTE: LivesManager ya persiste
    if (livesText != null)
    {
        livesText.text = "x" + lives;
    }
}
```

### Problema 3: `PauseManager` usaba `AddLives(0)` como truco para leer

**Antes:**
```csharp
public void UpdateLives(int lives)
{
    int current = LivesManager.AddLives(0);  // ← dispara PlayerPrefs.Save() por cada pause!
    // ...
}
```

`AddLives(0)` recalculaba, hacía `PlayerPrefs.SetInt`, y llamaba `PlayerPrefs.Save()` — todo para **leer** las vidas. Cada vez que se pausaba el juego, esto hacía un flush innecesario de disco.

---

## Solución implementada

### Archivo: `Assets/Scripts/Core/LivesManager.cs`

**Cambios:**

1. **Cache de vidas en memoria** (`_cacheValid`, `_cachedLives`, `_lastCalcTime`):
   - La primera llamada a `GetLives()` lee de PlayerPrefs y cachea.
   - Las llamadas subsecuentes usan la caché si no ha pasado `RegenIntervalSeconds` (30 min).
   - Evita `PlayerPrefs.GetInt` repetidos en cada polling.

2. **Eliminado `PlayerPrefs.Save()` del getter:**
   - El path de regeneración ahora hace `PlayerPrefs.SetInt`/`SetString` (escritura a memoria, no bloqueante).
   - El flush a disco se delega a `AddLives()` (acción explícita) o a `ForceSync()`.
   - Unity persiste automáticamente los cambios de PlayerPrefs en memoria al cerrar la aplicación normalmente.

3. **Cache del timestamp de regeneración**: `GetRegenTimestamp()` ahora cachea el valor leído.

**Patrón de escritura corregido:**

| Operación | Antes | Después |
|-----------|-------|---------|
| `GetLives()` (getter) | `SetInt` + **`Save()` (flush)** | `SetInt` (mem) + cache |
| `GetLives()` polling (caché válida) | `GetInt` cada vez | **Cero PlayerPrefs** |
| `AddLives(amount)` | `SetInt` + `Save()` | `SetInt` + `Save()` (acción de usuario, OK) |
| `ForceSync()` (nuevo) | No existía | `PlayerPrefs.Save()` (flush explícito) |

### Archivo: `Assets/Scripts/UI/MainMenuUIManager.cs`

**Antes:**
```csharp
public void UpdateLivesDisplay(int currentLives)
{
    int lives = Mathf.Min(MaxLives, Mathf.Max(0, currentLives));
    PlayerPrefs.SetInt(LivesKey, lives);   // ← REDUNDANTE
    if (livesText != null) { livesText.text = "x" + lives; }
}
```

**Después:**
```csharp
public void UpdateLivesDisplay(int currentLives)
{
    int lives = Mathf.Min(MaxLives, Mathf.Max(0, currentLives));
    if (livesText != null) { livesText.text = "x" + lives; }
}
```

Eliminado el `PlayerPrefs.SetInt` redundante — `LivesManager.AddLives()` o `GetLives()` ya persisten el valor.

### Archivo: `Assets/Scripts/UI/PauseManager.cs`

**Antes:**
```csharp
int current = LivesManager.AddLives(0);
```

**Después:**
```csharp
int current = LivesManager.GetLives();
```

Usa el getter cacheado en lugar del truco `AddLives(0)` que disparaba `Save()`.

---

## Flujo de persistencia corregido

```
[Jugador gana/viere vida]
      ↓
AddLives(amount) → SetInt + SetString + Save()  ← flush explícito (OK)
      ↓
GameEvents.NotifyLivesChanged(nuevasVidas)
      ↓
[UI se actualiza via event, no via polling]
      ↓
[App se pausa/cierra]
      ↓
ForceSync() → PlayerPrefs.Save()  ← flush opcional al pausar
```

---

## Verificación

**Tests unitarios (`.NET 10` console project):** 16 pasados, 0 fallidos.

| Test | Verificación | Resultado |
|------|-------------|-----------|
| 1 | `GetLives()` no llama `Save()` | ✅ |
| 2 | `AddLives()` llama `Save()` (acción explícita) | ✅ |
| 3 | Regeneración +1 vida tras 30 min sin `Save()` | ✅ |
| 4 | Regeneración múltiple (60 min → +2, capped a 3) | ✅ |
| 5 | No regenera antes de 30 min | ✅ |
| 6 | Vidas al máximo no regenera | ✅ |
| 7 | Cache evita lectura repetida de PlayerPrefs | ✅ |
| 8 | `AddLives(0)` sigue llamando `Save()` (ineficiente, por eso se reemplazó) | ✅ |
| 9 | Vidas no negativas (clamp a 0) | ✅ |
| 10 | Cálculo de segundos hasta próxima vida (1800 - elapsed) | ✅ |

**Verificación estática (grep):**
- `LivesManager.cs`: solo 2 calls a `PlayerPrefs.Save()` → `ForceSync()` (línea 36) y `AddLives()` (línea 137). El getter `GetLives()` (línea 39-117) no contiene `Save()`.
- `PauseManager.cs`: `AddLives(0)` eliminado (usado `GetLives()`).
- `MainMenuUIManager.cs`: `PlayerPrefs.SetInt` eliminado de `UpdateLivesDisplay()`.

---

## Archivos modificados

| Archivo | Cambio |
|---------|--------|
| `Assets/Scripts/Core/LivesManager.cs` | Cache de vidas + timestamp, eliminado `Save()` del getter, método `_NoSave` para timestamp |
| `Assets/Scripts/UI/MainMenuUIManager.cs` | Eliminado `PlayerPrefs.SetInt()` redundante en `UpdateLivesDisplay()` |
| `Assets/Scripts/UI/PauseManager.cs` | `AddLives(0)` → `GetLives()` en `UpdateLives()` |

## Notas

- `MonetizationManager.SetConsent()` también llama a `PlayerPrefs.Save()`, pero es una acción rara del usuario (aceptar/rechazar consentimiento de anuncios), así que el `Save()` es aceptable allí.
- La regeneración de vidas persiste los cambios vía `SetInt`/`SetString` (memoria). El flush ocurre en `AddLives()` o al cerrar la app (flush automático de Unity). Si la app se cierra abruptamente, la regeneración de vidas puede perder un cálculo, pero se recalcula en la siguiente apertura desde el último timestamp guardado.
- El método `ForceSync()` está disponible para llamarlo en puntos críticos (ej: `OnApplicationPause`).
