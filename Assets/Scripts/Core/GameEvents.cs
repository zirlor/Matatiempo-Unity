using System;

namespace Matatiempo.Core
{
    public static class GameEvents
    {
        public static event Action<GameState, GameState> StateChanged;
        public static event Action<int> LevelStarted;
        public static event Action<int> LevelCompleted;
        public static event Action<string> WordStarted;
        public static event Action<string> WordCompleted;
        public static event Action<string> WordFailed;
        public static event Action<char, bool> LetterGuessed;
        public static event Action<int> AttemptsChanged;
        public static event Action<int, int> TimeChanged;
        public static event Action TimeUp;
        public static event Action GameWon;
        public static event Action GameLost;
        public static event Action<int> LivesChanged;

        public static void NotifyStateChanged(GameState previous, GameState current)
        {
            StateChanged?.Invoke(previous, current);
        }

        public static void NotifyLevelStarted(int level)
        {
            LevelStarted?.Invoke(level);
        }

        public static void NotifyLevelCompleted(int level)
        {
            LevelCompleted?.Invoke(level);
        }

        public static void NotifyWordStarted(string word)
        {
            WordStarted?.Invoke(word);
        }

        public static void NotifyWordCompleted(string word)
        {
            WordCompleted?.Invoke(word);
        }

        public static void NotifyWordFailed(string word)
        {
            WordFailed?.Invoke(word);
        }

        public static void NotifyLetterGuessed(char letter, bool correct)
        {
            LetterGuessed?.Invoke(letter, correct);
        }

        public static void NotifyAttemptsChanged(int badGuesses)
        {
            AttemptsChanged?.Invoke(badGuesses);
        }

        public static void NotifyTimeChanged(int remaining, int total)
        {
            TimeChanged?.Invoke(remaining, total);
        }

        public static void NotifyTimeUp()
        {
            TimeUp?.Invoke();
        }

        public static void NotifyGameWon()
        {
            GameWon?.Invoke();
        }

        public static void NotifyGameLost()
        {
            GameLost?.Invoke();
        }

        public static void NotifyLivesChanged(int lives)
        {
            LivesChanged?.Invoke(lives);
        }
    }
}