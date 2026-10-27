using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;
using Matatiempo.Core;

namespace Matatiempo.Gameplay
{
    public class HangmanGameManager : MonoBehaviour
    {
        public static HangmanGameManager Instance { get; private set; }

        public const int MaxBadGuesses = 6;

        public string TargetWord { get; private set; }
        public string DisplayWord { get; private set; }
        public int BadGuesses { get; private set; }
        public int RemainingGuesses => MaxBadGuesses - BadGuesses;
        public bool IsSolved { get; private set; }

        private readonly HashSet<int> _revealedIndexes = new HashSet<int>();
        private readonly HashSet<char> _guessedLetters = new HashSet<char>();
        private char[] _normalizedWord;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            GameEvents.WordStarted += HandleWordStarted;
        }

        private void OnDisable()
        {
            GameEvents.WordStarted -= HandleWordStarted;
        }

        private void HandleWordStarted(string word)
        {
            Setup(word);
        }

        public void Setup(string word)
        {
            TargetWord = word ?? string.Empty;
            BadGuesses = 0;
            IsSolved = false;
            _revealedIndexes.Clear();
            _guessedLetters.Clear();
            _normalizedWord = NormalizeWord(TargetWord);
            RefreshDisplay();
        }

        public void Guess(char letter)
        {
            if (IsSolved || string.IsNullOrEmpty(TargetWord))
            {
                return;
            }
            char normalizedLetter = NormalizeLetter(letter);
            if (normalizedLetter == '\0')
            {
                return;
            }
            if (_guessedLetters.Contains(normalizedLetter))
            {
                return;
            }
            _guessedLetters.Add(normalizedLetter);

            bool found = false;
            for (int i = 0; i < _normalizedWord.Length; i++)
            {
                if (_revealedIndexes.Contains(i))
                {
                    continue;
                }
                if (_normalizedWord[i] == normalizedLetter)
                {
                    _revealedIndexes.Add(i);
                    found = true;
                }
            }

            if (found)
            {
                RefreshDisplay();
                GameEvents.NotifyLetterGuessed(letter, true);
                if (IsFullyRevealed())
                {
                    IsSolved = true;
                    GameEvents.NotifyWordCompleted(TargetWord);
                }
            }
            else
            {
                BadGuesses++;
                GameEvents.NotifyLetterGuessed(letter, false);
                GameEvents.NotifyAttemptsChanged(BadGuesses);
                if (BadGuesses >= MaxBadGuesses)
                {
                    GameEvents.NotifyWordFailed(TargetWord);
                }
            }
        }

        public bool IsLetterGuessed(char letter)
        {
            char normalized = NormalizeLetter(letter);
            return normalized != '\0' && _guessedLetters.Contains(normalized);
        }

        private bool IsFullyRevealed()
        {
            for (int i = 0; i < TargetWord.Length; i++)
            {
                if (char.IsWhiteSpace(TargetWord[i]))
                {
                    continue;
                }
                if (!_revealedIndexes.Contains(i))
                {
                    return false;
                }
            }
            return true;
        }

        private void RefreshDisplay()
        {
            var builder = new StringBuilder(TargetWord.Length * 2 - 1);
            for (int i = 0; i < TargetWord.Length; i++)
            {
                if (i > 0)
                {
                    builder.Append(' ');
                }
                builder.Append(_revealedIndexes.Contains(i) ? TargetWord[i] : '_');
            }
            DisplayWord = builder.ToString();
        }

        /// <summary>
        /// Normaliza un string completo (mantiene compatibilidad estática).
        /// Usado externamente; internamente se prefiere NormalizeWord/NormalizeLetter para evitar allocations.
        /// </summary>
        public static string Normalize(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }
            var builder = new StringBuilder(value.Length);
            foreach (char character in value.Normalize(NormalizationForm.FormD))
            {
                UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(character);
                if (category != UnicodeCategory.NonSpacingMark)
                {
                    builder.Append(char.ToLowerInvariant(character));
                }
            }
            return builder.ToString();
        }

        /// <summary>
        /// Normaliza un carácter individual a su forma base minúscula sin allocations.
        /// Devuelve '\0' si el carácter no es una letra válida.
        /// Usa un mapeo explícito para el alfabeto español (incluye ñ, vocales acentuadas, ü).
        /// </summary>
        public static char NormalizeLetter(char letter)
        {
            switch (letter)
            {
                case 'á': case 'Á': return 'a';
                case 'é': case 'É': return 'e';
                case 'í': case 'Í': return 'i';
                case 'ó': case 'Ó': return 'o';
                case 'ú': case 'Ú': return 'u';
                case 'à': case 'À': return 'a';
                case 'è': case 'È': return 'e';
                case 'ì': case 'Ì': return 'i';
                case 'ò': case 'Ò': return 'o';
                case 'ù': case 'Ù': return 'u';
                case 'ñ': case 'Ñ': return 'n';
                case 'ü': case 'Ü': return 'u';
                case 'ç': case 'Ç': return 'ç';
                default:
                    if (letter >= 'A' && letter <= 'Z') return (char)(letter + 32);
                    if (letter >= 'a' && letter <= 'z') return letter;
                    if (letter >= '0' && letter <= '9') return letter;
                    if (char.IsWhiteSpace(letter)) return letter;
                    return '\0';
            }
        }

        /// <summary>
        /// Pre-normaliza una palabra completa a un char[].
        /// Se llama una sola vez en Setup(), evitando allocations durante el gameplay.
        /// </summary>
        private static char[] NormalizeWord(string word)
        {
            if (string.IsNullOrEmpty(word))
            {
                return new char[0];
            }
            char[] result = new char[word.Length];
            for (int i = 0; i < word.Length; i++)
            {
                result[i] = NormalizeLetter(word[i]);
            }
            return result;
        }
    }
}
