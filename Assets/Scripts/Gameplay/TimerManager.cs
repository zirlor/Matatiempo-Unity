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

        // Cacheado: evita allocations de WaitForSeconds por segundo.
        // WaitForSecondsRealtime no se ve afectado por timeScale; el pause se gestiona
        // deteniendo la corrutina vía HandleStateChanged.
        private static readonly WaitForSecondsRealtime _waitOneSecond = new WaitForSecondsRealtime(1f);

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
            // Detener el timer al pausar o al volver al menú
            if (current == GameState.Paused || current == GameState.MainMenu)
            {
                StopCountdown();
            }
            // Reanudar el timer al reanudar desde pausa (Playing <- Paused)
            else if (previous == GameState.Paused && current == GameState.Playing)
            {
                if (RemainingSeconds > 0)
                {
                    IsRunning = true;
                    GameEvents.NotifyTimeChanged(RemainingSeconds, TotalSeconds);
                    _tickRoutine = StartCoroutine(Tick());
                }
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
                yield return _waitOneSecond;
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