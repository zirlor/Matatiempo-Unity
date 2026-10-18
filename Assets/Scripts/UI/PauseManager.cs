using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Matatiempo.Core;

namespace Matatiempo.UI
{
    public class PauseManager : MonoBehaviour
    {
        [SerializeField] private GameObject pauseModal;
        [SerializeField] private TextMeshProUGUI livesText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button quitButton;

        private bool _wired;
        private bool _isPaused;

        private void OnEnable()
        {
            GameEvents.StateChanged += HandleStateChanged;
            GameEvents.LivesChanged += HandleLivesChanged;
            if (livesText != null)
            {
                livesText.text = string.Format(LocalizationManager.Instance.GetText("lives_format"), LivesManager.GetLives());
            }
            if (GameManager.Instance != null && GameManager.Instance.State == GameState.Paused)
            {
                ApplyPausedState();
            }
        }

        private void OnDisable()
        {
            GameEvents.StateChanged -= HandleStateChanged;
            GameEvents.LivesChanged -= HandleLivesChanged;
        }

        public void Setup(GameObject modal, TextMeshProUGUI lives, TextMeshProUGUI level, Button resume, Button quit)
        {
            pauseModal = modal;
            livesText = lives;
            levelText = level;
            resumeButton = resume;
            quitButton = quit;
            _wired = false;
            WireListeners();
            if (GameManager.Instance != null && GameManager.Instance.State == GameState.Paused)
            {
                ApplyPausedState();
            }
        }

        public void TogglePause()
        {
            if (GameManager.Instance == null)
            {
                return;
            }
            if (GameManager.Instance.State == GameState.Playing)
            {
                PauseGame();
            }
            else if (GameManager.Instance.State == GameState.Paused)
            {
                ResumeGame();
            }
        }

        public void PauseGame()
        {
            if (GameManager.Instance == null || GameManager.Instance.State != GameState.Playing)
            {
                return;
            }
            GameManager.Instance.SetState(GameState.Paused);
        }

        public void ResumeGame()
        {
            if (GameManager.Instance == null || GameManager.Instance.State != GameState.Paused)
            {
                return;
            }
            GameManager.Instance.SetState(GameState.Playing);
        }

        public void QuitToMenu()
        {
            Time.timeScale = 1f;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SuspendRun();
            }
        }

        public void UpdateLives(int lives)
        {
            int current = LivesManager.AddLives(0);
            if (livesText != null)
            {
                livesText.text = string.Format(LocalizationManager.Instance.GetText("lives_format"), current);
            }
        }

        private void HandleLivesChanged(int lives)
        {
            if (livesText != null)
            {
                livesText.text = string.Format(LocalizationManager.Instance.GetText("lives_format"), Mathf.Max(0, lives));
            }
        }

        private void ApplyPausedState()
        {
            _isPaused = true;
            Time.timeScale = 0f;
            if (levelText != null && GameManager.Instance != null)
            {
                levelText.text = string.Format(LocalizationManager.Instance.GetText("level_format"), GameManager.Instance.CurrentLevel);
            }
            if (pauseModal != null)
            {
                pauseModal.SetActive(true);
            }
        }

        private void HandleStateChanged(GameState previous, GameState current)
        {
            if (current == GameState.Paused)
            {
                ApplyPausedState();
            }
            else
            {
                if (_isPaused)
                {
                    _isPaused = false;
                    Time.timeScale = 1f;
                }
                if (pauseModal != null)
                {
                    pauseModal.SetActive(false);
                }
            }
        }

        private void WireListeners()
        {
            if (_wired)
            {
                return;
            }
            _wired = true;
            if (resumeButton != null)
            {
                resumeButton.onClick.AddListener(ResumeGame);
            }
            if (quitButton != null)
            {
                quitButton.onClick.AddListener(QuitToMenu);
            }
        }
    }
}