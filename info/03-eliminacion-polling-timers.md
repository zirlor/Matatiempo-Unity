# Incidente 3: Eliminación de polling runtime y allocations en timers

**Fecha:** 26 de octubre de 2026  
**Prioridad:** ⚠️ Media-Alta  
**Dificultad:** Media  
**Estado:** ✅ Completado

---

## Resumen

Tres fuentes de polling y allocations por-segundo eliminadas o reducidas:

| Área | Problema | Solución |
|------|----------|----------|
| TimerManager | `new WaitForSeconds(1f)` cada segundo → allocation GC | `WaitForSecondsRealtime` cacheado (`_waitOneSecond`) |
| UIManager | Polling de vidas cada 1s | Aumentado a 2s (ya es barato con cache de LivesManager) |
| RewardAdButton | ReadinessGate polleaba cada 0.5s continuamente | Aumentado a 1s + suscripción a `GameEvents.StateChanged` + skip si GameObject inactivo |

---

## Cambio 1: TimerManager — cache de `WaitForSeconds`

### Problema

```csharp
yield return new WaitForSeconds(1f);
```

Esto crea **una nueva instancia de `WaitForSeconds` por segundo**, generando garbage que el GC debe recolectar. En dispositivos móviles esto causa micro-stutters.

Además, `WaitForSeconds` depende de `timeScale`. `PauseManager` pone `Time.timeScale = 0f`, lo que "pausaba" el timer implícitamente. Pero `HandleStateChanged` tenía un **bug**: solo reanudaba el tick en `MainMenu → Paused`, no en el flujo real `Playing → Paused → Playing`.

### Antes

```csharp
yield return new WaitForSeconds(1f);

// HandleStateChanged: solo reanudaba en MainMenu → Paused
else if (previous == GameState.MainMenu && current == GameState.Paused && !IsRunning && RemainingSeconds > 0)
```

### Después

```csharp
private static readonly WaitForSecondsRealtime _waitOneSecond = new WaitForSecondsRealtime(1f);
// ...
yield return _waitOneSecond;

// HandleStateChanged corregido: detiene en Paused/MainMenu, reanuda en Paused → Playing
if (current == GameState.Paused || current == GameState.MainMenu) { StopCountdown(); }
else if (previous == GameState.Paused && current == GameState.Playing)
{
    if (RemainingSeconds > 0) { /* Resume */ }
}
```

### Detalles

- `WaitForSecondsRealtime` (cacheado como `static readonly`) = **cero allocations** por tick.
- El pause se gestiona ahora **explícitamente** vía `HandleStateChanged` (detener/reanudar la corrutina), no dependiendo de `timeScale`.
- El bug de reanudación (`previous == GameState.MainMenu`) se corrige a `previous == GameState.Paused`.

---

## Cambio 2: UIManager — polling de vidas 1s → 2s

### Problema

Polleaba `LivesManager.GetLives()` cada 1 segundo.

### Solución

Reducido a 2 segundos. Con el cache de `LivesManager` del Incidente 2, `GetLives()` es casi gratis (cero PlayerPrefs cuando el cache es válido), así que 2s es suficiente para detectar la regeneración (vidas regeneran cada 30 min).

```csharp
if (Time.unscaledTime - _lastLivesPoll >= 2f)
```

---

## Cambio 3: RewardAdButton.ReadinessGate — reducir polling + eventos

### Problema

El `ReadinessGate` (MonoBehaviour adjunto al botón de anuncio) hacía polling cada **0.5s** continuamente, incluso cuando el botón estaba inactivo o la monetización estaba deshabilitada. Llamaba a `MonetizationManager.Instance.IsRewardedAvailable` (property access + float subtraction) cada 0.5s.

### Solución

1. **Intervalo 0.5s → 1s**: reduce el número de checks a la mitad.
2. **Suscripción a `GameEvents.StateChanged`**: refresca el botón inmediatamente cuando cambia el estado del juego (ej: cooldown termina al volver al menú).
3. **Skip si GameObject inactivo**: `if (!gameObject.activeInHierarchy || ...)` — evita polling cuando el botón está deshabilitado.
4. **Refresco inmediato en `OnEnable`**: `_nextPoll = 0f` para refrescar al activarse.

```csharp
private void OnEnable()
{
    GameEvents.StateChanged += HandleStateChanged;
    _nextPoll = 0f; // refrescar inmediatamente
}

private void Update()
{
    if (!gameObject.activeInHierarchy || Time.unscaledTime < _nextPoll)
        return;
    _nextPoll = Time.unscaledTime + 1f;
    Refresh();
}
```

---

## Verificación

- **TimerManager**: el campo `_waitOneSecond` es `static readonly` → cero allocations. `HandleStateChanged` corregido para el flujo real de pausa (`Playing ↔ Paused`).
- **UIManager**: el polling de vidas sigue funcionando (2s es suficiente).
- **RewardAdButton**: `GameEvents.StateChanged` existe y se usa en otros componentes (UIManager, MonetizationManager). La suscripción/desuscripción es correcta vía `OnEnable`/`OnDisable`.
- **Static analysis**: todos los tipos referenciados (`WaitForSecondsRealtime`, `GameState`, `GameEvents`) existen en el codebase.

## Archivos modificados

| Archivo | Cambio |
|---------|--------|
| `Assets/Scripts/Gameplay/TimerManager.cs` | Cache `WaitForSecondsRealtime`, `new WaitForSeconds(1f)` → `_waitOneSecond`, `HandleStateChanged` corregido |
| `Assets/Scripts/UI/UIManager.cs` | Polling de vidas 1s → 2s |
| `Assets/Scripts/UI/RewardAdButton.cs` | ReadinessGate: polling 0.5s → 1s, suscripción a `StateChanged`, skip si inactivo |
