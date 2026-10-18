using System.Collections;
using UnityEngine;
using Matatiempo.Core;

namespace Matatiempo.Gameplay
{
    public class TimerManager : MonoBehaviour
    {
        public static TimerManager Instance { get; private set; }

        public int RemainingSeconds { get; private set; }
        public int TotalSeconds { get; private set; }
        public bool IsRunning { get; private set; }

        private Coroutine _tickRoutine;

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
            GameEvents.WordCompleted += HandleWordCompleted;
            GameEvents.WordFailed += HandleWordFailed;
            GameEvents.TimeUp += HandleTimeUp;
            GameEvents.StateChanged += HandleStateChanged;
        }

        private void OnDisable()
        {
            GameEvents.WordStarted -= HandleWordStarted;
            GameEvents.WordCompleted -= HandleWordCompleted;
            GameEvents.WordFailed -= HandleWordFailed;
            GameEvents.TimeUp -= HandleTimeUp;
            GameEvents.StateChanged -= HandleStateChanged;
        }

        private void HandleWordStarted(string word)
        {
            int level = GameManager.Instance.CurrentLevel;
            StartCountdown(LevelProgression.TimeSecondsForWord(level, word));
        }

        private void HandleWordCompleted(string word)
        {
            StopCountdown();
        }

        private void HandleWordFailed(string word)
        {
            StopCountdown();
        }

        private void HandleTimeUp()
        {
            StopCountdown();
        }

        private void HandleStateChanged(GameState previous, GameState current)
        {
            if (current == GameState.MainMenu)
            {
                StopCountdown();
            }
            else if (previous == GameState.MainMenu && current == GameState.Paused && !IsRunning && RemainingSeconds > 0)
            {
                IsRunning = true;
                GameEvents.NotifyTimeChanged(RemainingSeconds, TotalSeconds);
                _tickRoutine = StartCoroutine(Tick());
            }
        }

        public void StartCountdown(int seconds)
        {
            StopCountdown();
            TotalSeconds = Mathf.Max(0, seconds);
            RemainingSeconds = TotalSeconds;
            IsRunning = true;
            GameEvents.NotifyTimeChanged(RemainingSeconds, TotalSeconds);
            _tickRoutine = StartCoroutine(Tick());
        }

        public void StopCountdown()
        {
            IsRunning = false;
            if (_tickRoutine != null)
            {
                StopCoroutine(_tickRoutine);
                _tickRoutine = null;
            }
        }

        private IEnumerator Tick()
        {
            while (RemainingSeconds > 0)
            {
                yield return new WaitForSeconds(1f);
                RemainingSeconds--;
                GameEvents.NotifyTimeChanged(RemainingSeconds, TotalSeconds);
            }

            IsRunning = false;
            _tickRoutine = null;
            if (RemainingSeconds <= 0)
            {
                GameEvents.NotifyTimeUp();
            }
        }
    }
}