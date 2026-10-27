# Informe de Optimización #1: UI rebuild — reutilizar botones y cachear singletons

**Fecha:** 2026-10-26  
**Gravedad original:** ⚠️ Alta  
**Áreas:** `MainMenuView.cs` (RebuildLevels), `GameView.cs` (ApplyGameState)  

## Problema detectado

1. **Destruir y recrear todos los botones de nivel en cada rebuild (MainMenuView.cs)**: `RebuildLevels()` iteraba y hacía `Object.Destroy` de todos los hijos del grid, luego recreaba N botones con `UIUtils.CreateCenteredButton()` — cada uno un `new GameObject` + múltiples `AddComponent` (RectTransform, Image, Button, TextMeshProUGUI, Shadow, LayoutElement, etc.). Esto ocurrir en cada cambio de orientación o idioma.

2. **Singleton lookups redundantes en `ApplyGameState()` (GameView.cs)**: En el bucle de teclas (`_keys`, ~28 botones), se evaluaba `HangmanGameManager.Instance` tres veces por iteración (1.ª acceso a `.IsSolved` dentro de `!HangmanGameManager.Instance.IsSolved && !HangmanGameManager.Instance.IsLetterGuessed(...)`), y `LocalizationManager.Instance` se accedía 4 veces fuera del loop. Cada `.Instance` implica un acceso a propiedad estática con null-check.

## Solución aplicada

### MainMenuView.cs — Reutilización de botones de nivel
- **`RebuildLevels()`** ahora primero resuelve `_maxLevel` y verifica `existingCount = _levelGridRect.childCount`.
- **Si el número de niveles no cambió** (`existingCount == _maxLevel`): llama a `RefreshLevelButtons()` que **actualiza** texto, tamaño de fuente, tamaño de celda y callbacks **sin destruir objetos** — elimina GC por rebuild.
- **Si el número cambió** (dictionary distinto): procede con el destroy+recreate original.
- **`RefreshLevelButtons()`**: itera los hijos existentes, busca el `TextMeshProUGUI` label (vía `targetGraphic`, luego `Find("Label")`, luego `GetChild(0)`), actualiza texto con `string.Format` + localización, reescala con `SetSizeWithCurrentAnchors`, y reasigna el callback (`RemoveAllListeners` + `AddListener` con click sound + `StartLevel`).

### GameView.cs — Cacheo de singletons en ApplyGameState()
- Cachea `LocalizationManager.Instance` → variable local `localization` (1 acceso en vez de 4).
- Cachea `HangmanGameManager.Instance` → variable local `hangman` (1 acceso en vez de 1 + 28 accesos).
- Extrae `bool solved = hangman.IsSolved` **antes del loop** → 1 evaluación en vez de 28 evaluaciones redundantes de `IsSolved`.
- El loop ahora usa `!solved && !hangman.IsLetterGuessed(pair.Key)` — 1 acceso a `.Instance` + 1 acceso a `.IsSolved` + 28 accesos a `.IsLetterGuessed` (estos sí deben ser por-key).

## Impacto

| Escenario | Antes | Después |
|---|---|---|
| RebuildLevels (misma cantidad de niveles) | N× Destroy+Create (~28 GameObjects + 8+ componentes cada uno) | 0 Destroy, solo update de props (28 labels + callbacks) |
| ApplyGameState loop (28 teclas) | 56 accesos a `.Instance` (28×2 para IsSolved + IsLetterGuessed... corregido: 28 Instance + 28 IsSolved redundantes) | 2 accesos a `.Instance` + 1 evaluación de IsSolved |
| Localización en ApplyGameState | 4 accesos a `LocalizationManager.Instance` | 1 acceso cacheado |

**GC reducido**: RebuildLevels sin reutilización = ~28 Destroy + ~28 GameObject alloc + ~224 AddComponent allocations. Con reutilización = 0.

## Verificación
- Syntax verificado: imports intactos (`using UnityEngine.UI`, `using TMPro` presentes en MainMenuView).
- `RefreshLevelButtons` usa `button.targetGraphic.GetComponent<TextMeshProUGUI>()` como primera estrategia (el targetGraphic es el Label por defecto en UnityButton), con fallbacks para estructuras distintas.
- Callbacks de click sound preservados: `RemoveAllListeners()` limpia el listener original de `AddClickSound`, se re-add manualmente el click sound.
- `ApplyGameState` mantiene la misma lógica de interactividad de teclas.

## Pendiente
- **GameView.Build()**: sigue llamando `LocalizationManager.Instance.GetText()` ~15 veces directamente en `Build()`, `BuildTopHud`, `BuildPauseOverlay`, etc. (cada rebuild). Podría cachear como variable local `level` y `timer` strings o pasar el LocalizationManager como parámetro. Impacto menor (build es menos frecuente que ApplyGameState, y localización está O(1) ahora).
- **InstructionsModal / HintModal**: `Build()` destruye y recrea el modal en cada rebuild del GameView. Podría reutilizar el GameObject existente.
- **Migrar a prefabs serializados**: la solución definitiva sería usar prefabs con anchors responsivos en vez de código Build imperative. Requiere refactor de asset structure.
