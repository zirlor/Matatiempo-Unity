using System.Collections;
using UnityEngine;

namespace Matatiempo.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState State { get; private set; }
        public int CurrentLevel { get; private set; }

        [SerializeField] private string startupLanguage = "es";

        private Coroutine _pendingNextWord;
        private bool _runInProgress;
        private int _runLevel;

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
            GameEvents.WordCompleted += HandleWordCompleted;
            GameEvents.WordFailed += HandleWordFailed;
            GameEvents.TimeUp += HandleTimeUp;
        }

        private void OnDisable()
        {
            GameEvents.WordCompleted -= HandleWordCompleted;
            GameEvents.WordFailed -= HandleWordFailed;
            GameEvents.TimeUp -= HandleTimeUp;
        }

        private void Start()
        {
            LocalizationManager.Instance.SetLanguage(startupLanguage);
            SetState(GameState.MainMenu);
        }

        public void SetState(GameState state)
        {
            if (State == state)
            {
                return;
            }
            GameState previous = State;
            State = state;
            GameEvents.NotifyStateChanged(previous, state);
        }

        public bool StartLevel(int level)
        {
            if (!LevelManager.Instance.IsLevelUnlocked(level))
            {
                Debug.LogWarning($"Level {level} is locked. Unlocked levels: 1-{LevelManager.Instance.GetMaxLevelUnlocked()}");
                return false;
            }
            if (!LivesManager.HasLives())
            {
                Debug.LogWarning("Sin vidas suficientes. Mira un anuncio para ganar +1 vida.");
                return false;
            }
            if (_runInProgress && level == _runLevel)
            {
                ResumeRun();
                return true;
            }
            if (!WordManager.Instance.PrepareLevel(level))
            {
                Debug.LogError($"Level {level} is not available in the active dictionary.");
                return false;
            }

            if (_pendingNextWord != null)
            {
                StopCoroutine(_pendingNextWord);
                _pendingNextWord = null;
            }

            CurrentLevel = level;
            _runInProgress = true;
            _runLevel = level;
            GameEvents.NotifyLevelStarted(level);
            SetState(GameState.Playing);
            StartNextWord();
            return true;
        }

        public void SuspendRun()
        {
            if (State == GameState.Playing || State == GameState.Paused)
            {
                _runInProgress = true;
                _runLevel = CurrentLevel;
            }
            if (_pendingNextWord != null)
            {
                StopCoroutine(_pendingNextWord);
                _pendingNextWord = null;
            }
            SetState(GameState.MainMenu);
        }

        private void ResumeRun()
        {
            if (_pendingNextWord != null)
            {
                StopCoroutine(_pendingNextWord);
                _pendingNextWord = null;
            }
            SetState(GameState.Paused);
        }

        private void HandleWordCompleted(string word)
        {
            WordManager.Instance.CompleteCurrentWord();
            if (WordManager.Instance.HasMoreWords)
            {
                if (_pendingNextWord != null)
                {
                    StopCoroutine(_pendingNextWord);
                }
                _pendingNextWord = StartCoroutine(StartNextWordDeferred());
            }
            else
            {
                _runInProgress = false;
                LevelManager.Instance.CompleteLevel(CurrentLevel);
                GameEvents.NotifyLevelCompleted(CurrentLevel);
                SetState(GameState.LevelCompleted);
            }
        }

        private void HandleWordFailed(string word)
        {
            LoseLifeAndGameOver();
        }

        private void HandleTimeUp()
        {
            LoseLifeAndGameOver();
        }

        private void LoseLifeAndGameOver()
        {
            if (State != GameState.Playing && State != GameState.Paused)
            {
                return;
            }
            _runInProgress = false;
            int lives = LivesManager.AddLives(-1);
            GameEvents.NotifyLivesChanged(lives);
            GameEvents.NotifyGameLost();
            SetState(GameState.GameOver);
        }

        private IEnumerator StartNextWordDeferred()
        {
            yield return null;
            _pendingNextWord = null;
            StartNextWord();
        }

        private void StartNextWord()
        {
            string word = WordManager.Instance.NextWord();
            if (word == null)
            {
                GameEvents.NotifyLevelCompleted(CurrentLevel);
                SetState(GameState.LevelCompleted);
                return;
            }
            GameEvents.NotifyWordStarted(word);
        }
    }
}