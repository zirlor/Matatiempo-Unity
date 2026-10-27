# Informe de Optimización #2 + #3: Cache de Resources.Load + Diccionario de localización O(1)

**Fecha:** 2026-10-26  
**Gravedad original:** ⚠️ Alta / Media-Alta  
**Áreas:** `UIUtils.cs`, `LocalizationManager.cs`, `InstructionsModal.cs`  

## Problema detectado

1. **Resources.Load<Sprite> repetido sin cache (UIUtils.cs)**: `ApplySprite()`, `ApplyPaperButton()` y `ApplyPaperKey()` llamaban `Resources.Load<Sprite>()` en cada invocación. Durante un `Rebuild()` de UI se puede disparar decenas de veces, haciendo búsquedas de asset en disco cada vez.
2. **`InstructionsModal.cs`** llamaba `Resources.Load<Sprite>()` directamente (línea 120) sin cache.
3. **`GetText()` búsqueda lineal O(n) (LocalizationManager.cs)**: Cada traducción hacía un `for` sobre `table.entries` (array de UIStringEntry). En un build de UI con 30+ traducciones, eso es 30+ búsquedas lineales = O(n²) por rebuild.

## Solución aplicada

### UIUtils.cs — Cacheo de sprites
- Añadido `private static readonly Dictionary<string, Sprite> _spriteCache`.
- Nuevo método público `LoadSprite(string resourcePath)` que cachea cada sprite cargado (primer `Resources.Load` + cache).
- `ApplySprite()`, `ApplyPaperButton()`, `ApplyPaperKey()` ahora usan `LoadSprite()` en lugar de `Resources.Load<Sprite>` directo.
- Método público `ClearSpriteCache()` para recargar assets en modo editor.

### InstructionsModal.cs
- Reemplazado `Resources.Load<Sprite>(spritePath)` → `UIUtils.LoadSprite(spritePath)` (usa el cache compartido).

### LocalizationManager.cs — Lookup O(1)
- Añadido campo `[System.NonSerialized] public Dictionary<string, string> entriesLookup` a `UIStringTable` (no se serializa en JSON, se construye en runtime).
- Método `BuildLookup()` construye el diccionario al cargar la tabla UI (solo una vez por idioma).
- `GetUiTable()` llama `BuildLookup()` antes de cachear.
- `GetText()` ahora hace `TryGetValue` O(1) en `entriesLookup`, con fallback lineal solo si el diccionario no está poblado (compatibilidad con tablas pre-serializadas).
- Guardado el `using System.Collections.Generic` ya existente.

## Impacto esperado
- **Sprite loading**: 22 sprites en escena → 22 `Resources.Load` → 1 por sprite + 21 lookups en caché (≈95% reducción en calls de Resources.Load).
- **Localización**: cada `GetText` pasa de O(n) a O(1). Con ~35 keys × 30+ llamadas por rebuild, reducción ~30x en tiempo de localización.

## Verificación
- Sintaxis C# coherente: `using System.Collections.Generic` presente, tipos correctos.
- API pública intacta: `GetText()`, `SetLanguage()`, `GetDictionary()`, `UIStringTable`, `UIStringEntry` sin romper contratos.
- `[System.NonSerialized]` garantiza que `JsonUtility.FromJson` ignora el lookup (no se deserializa desde JSON).
- Fallback lineal preserva comportamiento si `entriesLookup` es null (e.g., tabla creada manualmente).

## Pendiente
- Considerar aplicar el mismo patrón de cache a `Resources.Load<TextAsset>` en `GetDictionary()`/`LoadUiTable()` — ya están cacheados en `_dictionaries`/`_uiTables`, pero el primer acceso sigue haciendo `Resources.Load<TextAsset>`.
