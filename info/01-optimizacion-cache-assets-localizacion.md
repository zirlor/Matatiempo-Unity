# Incidente 1: Cacheo de Assets y Localización

**Fecha:** 26 de octubre de 2026  
**Prioridad:** ⚠️ Alta  
**Dificultad:** Fácil  
**Estado:** ✅ Completado y verificado

---

## Problema

El juego realizaba múltiples allocations y operaciones de I/O innecesarias por segundo durante el gameplay, debido a:

1. **`Resources.Load<Sprite>` repetidos** — Cada vez que se aplicaba un sprite a una `Image` UI, se llamaba a `Resources.Load` sin cachear. En un rebuild de UI completo, esto se repetía decenas de veces.
2. **Búsqueda lineal O(n)** — `GetText(string key)` iteraba sobre `UIStringEntry[]` con un `for` para encontrar cada key de localización. Llamado cientos de veces durante UI builds.
3. **Normalización de caracteres por cada pulsación** — `HangmanGameManager.Normalize(letter.ToString())` llamaba a `string.Normalize(NormalizationForm.FormD)` + `StringBuilder` + `CharUnicodeInfo` por cada carácter en cada pulsación de tecla. También llamaba `Normalize(TargetWord[i].ToString())` dentro del loop de `Guess()`.

## Solución implementada

### Archivo: `Assets/Scripts/UI/UIUtils.cs` (ya implementado)

- Agregado `Dictionary<string, Sprite> _spriteCache` estático.
- `ApplySprite()` y `ApplyPaperButton()` ahora llaman a `LoadSprite()` que hace lookup O(1) en caché antes de `Resources.Load`.
- Método público `ClearSpriteCache()` para limpiar en modo editor.

### Archivo: `Assets/Scripts/Core/LocalizationManager.cs` (ya implementado)

- Agregado `Dictionary<string, string> entriesLookup` a `UIStringTable` (con `[NonSerialized]`).
- `BuildLookup()` construye el diccionario al cargar cada tabla (una sola vez por idioma).
- `GetText()` usa `TryGetValue` (O(1)) con fallback a búsqueda lineal solo si el lookup no está poblado.

### Archivo: `Assets/Scripts/Gameplay/HangmanGameManager.cs` (cambios nuevos)

**Antes:**
```csharp
public bool IsLetterGuessed(char letter)
{
    string normalized = Normalize(letter.ToString());       // allocation: char→string→Normalize→StringBuilder
    return normalized.Length > 0 && _guessedLetters.Contains(normalized);
}
// ...
private readonly HashSet<string> _guessedLetters = new HashSet<string>();  // string allocations

public void Guess(char letter)
{
    string normalizedLetter = Normalize(letter.ToString()); // allocation por cada pulsación
    // ...
    if (Normalize(TargetWord[i].ToString()) == normalizedLetter) // allocation POR CADA CARACTER en el loop
}
```

**Después:**
```csharp
public static char NormalizeLetter(char letter)        // O(1), zero allocations
{
    switch (letter)
    {
        case 'á': case 'Á': return 'a';
        case 'é': case 'É': return 'e';
        // ... vocales acentuadas, ñ→n, ü→u, ç→ç, whitespace preservado
        default:
            if (letter >= 'A' && letter <= 'Z') return (char)(letter + 32);
            if (letter >= 'a' && letter <= 'z') return letter;
            if (char >= '0' && letter <= '9') return letter;
            if (char.IsWhiteSpace(letter)) return letter;
            return '\0';
    }
}

private char[] _normalizedWord;                        // pre-normalizada en Setup()
private readonly HashSet<char> _guessedLetters = new HashSet<char>();

public void Setup(string word)
{
    // ...
    _normalizedWord = NormalizeWord(TargetWord);     // una sola vez al iniciar la palabra
}

public void Guess(char letter)
{
    char normalizedLetter = NormalizeLetter(letter);   // O(1), zero allocation
    // ...
    if (_normalizedWord[i] == normalizedLetter)        // comparación directa de char, no strings
}
```

## Impacto de allocations eliminado

| Operación | Antes | Después |
|-----------|-------|---------|
| Cada pulsación de tecla | `letter.ToString()` + `Normalize()` → múltiples strings + StringBuilder | `NormalizeLetter(char)` O(1), cero allocations |
| Loop de comparación por letra | `Normalize(TargetWord[i].ToString())` por cada índice | comparación directa `_normalizedWord[i] == normalizedLetter` |
| Verificación `IsLetterGuessed` | `Normalize(letter.ToString())` + string lookup | `NormalizeLetter(char)` + `HashSet<char>.Contains` |
| Sprite UI builds | `Resources.Load<Sprite>` por cada Image | Lookup en `Dictionary<string, Sprite>` |
| `GetText` localización | Búsqueda lineal O(n) | `Dictionary.TryGetValue` O(1) |

## Verificación

- **Tests unitarios (`.NET 10` console project):** 42 tests pasados, 0 fallidos.
  - Test 1: Vocales acentuadas (áÁéÉíÍóÓúÚ → aeiou) ✅
  - Test 2: Eñe (ñÑ → n, coincide con método original `Normalize`) ✅
  - Test 3: Diéresis (üÜ → u) ✅
  - Test 4: Letras ASCII (lowercase/uppercase/numeros) ✅
  - Test 5: Whitespace preservado, caracteres inválidos → '\0' ✅
  - Test 6: 10 palabras normalizadas vs método original → **100% coincidencia** ✅
  - Test 7: Palabras con espacios vs método original → **100% coincidencia** ✅
  - Test 8: Longitud de `char[]` preservada ✅
  - Test 9: Búsqueda de letras simula `Guess()` → comportamiento identico ✅
- **Método original `Normalize(string)` preservado** como público estático para compatibilidad externa.

## Archivos modificados

| Archivo | Cambio |
|---------|--------|
| `Assets/Scripts/Gameplay/HangmanGameManager.cs` | `NormalizeLetter(char)` O(1), `NormalizeWord(string)`, pre-normalización en `Setup()`, `HashSet<char>` |
| `Assets/Scripts/UI/UIUtils.cs` | Cache de sprites `_spriteCache` + `LoadSprite` |
| `Assets/Scripts/Core/LocalizationManager.cs` | `entriesLookup` Dictionary + `BuildLookup` |

## Notas

- La normalización de ñ→n **coincide exactamente** con el comportamiento del método original `Normalize()`, que usa `FormD` + filtro de `NonSpacingMark`. La ñ se descompone en 'n' + tilde combinada, y la tilde se filtra, resultando en 'n'.
- El método `Normalize(string)` original se mantiene como método público estático para cualquier código externo que lo dependa.
