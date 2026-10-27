# Informe de Optimización #5 + #11: PlayerPrefs.Save excesivo y writes redundantos

**Fecha:** 2026-10-26  
**Gravedad original:** ⚠️ Alta  
**Área:** `LivesManager.cs`, `MainMenuUIManager.cs`  

## Problema detectado

1. **`LivesManager.GetLives()` era un getter con side effects**: Llamaba `PlayerPrefs.Save()` (flush bloqueante a disco) cada vez que regeneraba vidas por inactividad. Como `GetLives()` se llamaba cada 1 segundo desde el polling de `UIManager.Update()`, el juego hacía flush a disco con frecuencia innecesaria.
2. **`SetRegenTimestamp()` llamaba `PlayerPrefs.Save()`** por cada escritura de timestamp, multiplicando los flushes.
3. **`MainMenuUIManager.UpdateLivesDisplay()` escribía `PlayerPrefs.SetInt(LivesKey, lives)`** de forma redundante — `LivesManager.AddLives()` ya había escrito el valor.
4. **`GetRegenTimestamp()` / `HasKey()` cada llamada**: Lectura de PlayerPrefs en cada poll incluso cuando no cambiaba nada.

## Solución aplicada

### `LivesManager.cs`
- **Cache en memoria**: Añadidos `_cacheValid`, `_cachedLives`, `_lastCalcTime`. La caché se invalida solo cuando se modifican las vidas (`AddLives()`).
- **`GetLives()` usa cache**: Si la caché es válida, no vuelve a leer PlayerPrefs hasta que haya transcurrido `RegenIntervalSeconds` (1800s) o las vidas estén al máximo de regen.
- **Eliminé `PlayerPrefs.Save()` de `SetRegenTimestamp`**: Renombrado a `SetRegenTimestamp_NoSave()`. El flush a disco se delega a:
  - Regeneración real de vidas (cuando el valor cambia).
  - `AddLives()` (operación explícita de modificación).
- **`ClearRegenTimestamp` → `ClearRegenTimestamp_NoSave`**: No hace Save() inmediato.
- **Método `ForceSync()`**: flush explícito disponible para llamar al pausar la app o al cambiar de escena críticas.
- **`GetSecondsUntilNextLife()`**: Reutiliza `GetLives()` cacheado en lugar de recalcular dos veces.

### `MainMenuUIManager.cs`
- **Eliminado `PlayerPrefs.SetInt(LivesKey, lives)`** de `UpdateLivesDisplay()`. Esta responsabilidad ya la tiene `LivesManager`.
- La constante pública `LivesKey` se mantiene por compatibilidad.

## Impacto en PlayerPrefs.Save() (antes → después)

| Escenario | Antes | Después |
|---|---|---|
| Polling cada 1s (sin regeneración) | 0 saves (solo GetInt) | 0 saves (cache devuelta) |
| Regeneración de vida | 2 saves (SetRegenTimestamp + SetInt) | 1 save (solo SetInt) |
| `AddLives(+1)` | 1 save | 1 save (inalterado) |
| SetConsent (monetización) | 1 save | 1 save (sin cambios en esta optimización) |

## Beneficios
- Reducción ~50% en llamadas a `PlayerPrefs.Save()` en el path de regeneración.
- Cache evita PlayerPrefs.GetInt + cálculo en cada poll de 1s cuando no hay cambios.
- El flush a disco solo ocurre cuando realmente cambian los datos (regeneración o AddLives explícito).
- `MainMenuUIManager` ya no duplica writes de PlayerPrefs.

## Verificación
- Sintaxis verificada: el archivo es coherente, tipos y usings intactos.
- No se rompe la API pública: `GetLives()`, `HasLives()`, `AddLives(int)`, `GetSecondsUntilNextLife()` mantienen las mismas firmas.

## Pendiente
- Considerar migrar todo el sistema de guardado de PlayerPrefs a archivo JSON en `persistentDataPath` (ver Optimización #13 — planeada).
- Integrar `ForceSync()` en pausa de app (`OnApplicationPause`/`OnApplicationFocus`) o en escenas críticas.
