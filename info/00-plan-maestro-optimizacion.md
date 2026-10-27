# 📋 Plan Maestro de Optimización — Matatiempo-Unity

**Proyecto:** `C:\Users\zirow\source\repos\Matatiempo-Unity` (Unity 6000.5.0f1, .NET)  
**Fecha:** 26 de octubre de 2026  
**Estado general:** 5 de 5 incidentes aplicados. ⏳ Compilación Unity pendiente (instalación de editor en background).

---

## Incidencias identificadas y prioridades

| # | Incidencia | Archivos | Gravedad | Prioridad | Estado | Informe |
|---|-----------|----------|----------|-----------|--------|---------|
| 1 | Cache de assets (Resources.Load repetido) | `UIUtils.cs`, `LocalizationManager.cs`, `MonetizationManager.cs` | ⚠️ Alta | 1 | ✅ Resuelta | `01-` |
| 2 | PlayerPrefs: `Save()` en getter polling | `LivesManager.cs`, `MainMenuUIManager.cs`, `PauseManager.cs` | ⚠️ Alta | 2 | ✅ Resuelta | `02-` |
| 3 | Polling runtime + allocations en timers | `TimerManager.cs`, `UIManager.cs`, `RewardAdButton.cs` | ⚠️ Media-Alta | 3 | ✅ Resuelta | `03-` |
| 4 | Closures/lambdas en loops UI | `UIUtils.cs`, `GameView.cs`, `MainMenuView.cs` | ⚠️ Media | 4 | ✅ Resuelta | `04-` |
| 5 | View.Rebuild destruye+reconstruye todo; localización fuerza rebuild | `View.cs`, `UIManager.cs`, `LocalizationManager.cs`, views | ⚠️ Alta | 5 | ✅ Resuelta | `05-` |

## Incidencias analizadas y descartadas (no aplican)

| # | Incidencia | Hallazgo |
|---|-----------|----------|
| 6 | `new WaitForSeconds(1f)` en TimerManager | ✅ Resuelta en Incidente 3 (cacheado con `WaitForSecondsRealtime`) |
| 7 | AssetBundle/Addressables | ❌ No usado en el proyecto. Caching de `Resources.Load` (Incidente 1) es suficiente. |
| 8 | Closures/lambdas en loops | ✅ Resuelta en Incidente 4 |
| 9 | `LevelManager.EnsureSceneLoaded()` polling | ❌ No existe en el código actual. `LevelManager` no hace scene loading. |
| 10 | GameManager `Update()` con GetComponent redundantes | ❌ `GameManager` **no tiene** `Update()`. Usa eventos (`OnEnable`/`OnDisable`). |
| 11 | `LocalizationManager._activeAsset` no cached | ✅ Ya implementado: `_dictionaries` + `_uiTables` caches + `entriesLookup`. |

---

## Arquitectura de datos del proyecto

```
Matatiempo-Unity (Unity 6000.5.0f1, .NET Framework)
├── Core/                          # Managers singletons (DontDestroyOnLoad)
│   ├── GameManager.cs             Estado, level flow (eventos, sin Update)
│   ├── LevelManager.cs            Progreso vidas (PlayerPrefs)
│   ├── WordManager.cs             Palabras (usa LocalizationManager.Active)
│   ├── LivesManager.cs            Regeneración de vidas (Incidente 2)
│   ├── AudioManager.cs            Sonidos
│   ├── LocalizationManager.cs     Cache de diccionarios UI + eventos (Incidentes 1, 5)
│   ├── LevelProgression.cs        Constantes (static)
│   └── GameState.cs               Enum
├── Gameplay/
│   ├── HangmanGameManager.cs      Lógica del ahorcado
│   ├── TimerManager.cs            Timer (Incidente 3)
│   └── LevelProgression.cs
├── Monetization/
│   ├── MonetizationManager.cs     Consentimiento, cooldown (PlayerPrefs)
│   ├── AdsManager.cs              LevelPlay provider
│   ├── IAdProvider.cs             Interface
│   ├── NoOpAdProvider.cs          Fallback
│   └── UnityAdsAdProvider.cs
├── UI/
│   ├── View.cs                    Base: dirty-check Rebuild + RefreshLocalization (Incidente 5)
│   ├── UIManager.cs               Orquesta views, polling vidas (Incidentes 3, 5)
│   ├── GameView.cs                Teclado, HUD (Incidentes 3, 4)
│   ├── MainMenuView.cs            Menú, niveles, idioma (Incidentes 4, 5)
│   ├── ResultView.cs              Pantalla de resultado
│   ├── RewardAdButton.cs          Botón rewarded + ReadinessGate (Incidente 3)
│   ├── SafeAreaPanel.cs           Safe area (polling barato)
│   ├── UIUtils.cs                 Fábricas UI + cache sprites (Incidentes 1, 4)
│   ├── KeyButtonHandler.cs        Handler teclado (nuevo, Incidente 4)
│   └── LanguageButtonHandler.cs   Handler idioma (nuevo, Incidente 4)
└── info/                          # Informes numerados
```

---

## Principios aplicados

- **Eventos sobre polling**: `GameEvents.StateChanged` en lugar de `Update()` polling.
- **Cachear, no recalcular**: sprite cache, dictionary cache, lives cache, `WaitForSeconds` cacheado.
- **Delegates reutilizados**: `static readonly UnityAction` en vez de lambda por botón.
- **Dirty check**: solo destruir/reconstruir UI cuando algo cambió.
- **Refresh incremental**: `RefreshLocalization()` refresca textos sin destroy.
- **PlayerPrefs no bloqueante**: `SetInt` (memoria) + `Save()` solo en acciones explícitas de usuario.
