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

        public bool IsLetterGuessed(char letter)
        {
            string normalized = Normalize(letter.ToString());
            return normalized.Length > 0 && _guessedLetters.Contains(normalized);
        }

        private readonly HashSet<int> _revealedIndexes = new HashSet<int>();
        private readonly HashSet<string> _guessedLetters = new HashSet<string>();

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
            RefreshDisplay();
        }

        public void Guess(char letter)
        {
            if (IsSolved || string.IsNullOrEmpty(TargetWord))
            {
                return;
            }

            string normalizedLetter = Normalize(letter.ToString());
            if (normalizedLetter.Length == 0)
            {
                return;
            }
            if (_guessedLetters.Contains(normalizedLetter))
            {
                return;
            }
            _guessedLetters.Add(normalizedLetter);

            bool found = false;
            for (int i = 0; i < TargetWord.Length; i++)
            {
                if (_revealedIndexes.Contains(i))
                {
                    continue;
                }
                if (Normalize(TargetWord[i].ToString()) == normalizedLetter)
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
    }
}