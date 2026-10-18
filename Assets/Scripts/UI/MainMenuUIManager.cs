using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Matatiempo.Core;

namespace Matatiempo.UI
{
    public class MainMenuUIManager : MonoBehaviour
    {
        public const string LivesKey = LivesManager.LivesKey;
        public const int DefaultLives = LivesManager.DefaultLives;

        [SerializeField] private GameObject settingsModal;
        [SerializeField] private TextMeshProUGUI livesText;
        [SerializeField] private Button openSettingsButton;
        [SerializeField] private Button closeSettingsButton;

        private bool _wired;

        private void Start()
        {
            WireListeners();
        }

        public void Initialize(GameObject modal, TextMeshProUGUI lives, Button openButton, Button closeButton)
        {
            settingsModal = modal;
            livesText = lives;
            openSettingsButton = openButton;
            closeSettingsButton = closeButton;
            _wired = false;
            WireListeners();
            UpdateLivesDisplay(LivesManager.GetLives());
        }

        public void OpenSettings()
        {
            if (settingsModal != null)
            {
                settingsModal.SetActive(true);
            }
        }

        public void CloseSettings()
        {
            if (settingsModal != null)
            {
                settingsModal.SetActive(false);
            }
        }

        public void UpdateLivesDisplay(int currentLives)
        {
            int lives = Mathf.Min(LivesManager.MaxLives, Mathf.Max(0, currentLives));
            PlayerPrefs.SetInt(LivesKey, lives);
            if (livesText != null)
            {
                livesText.text = "x" + lives;
            }
        }

        public static int AddLives(int amount)
        {
            return LivesManager.AddLives(amount);
        }

        private void OnEnable()
        {
            GameEvents.LivesChanged += HandleLivesChanged;
            UpdateLivesDisplay(LivesManager.GetLives());
        }

        private void OnDisable()
        {
            GameEvents.LivesChanged -= HandleLivesChanged;
        }

        private void HandleLivesChanged(int lives)
        {
            UpdateLivesDisplay(lives);
        }

        private void WireListeners()
        {
            if (_wired)
            {
                return;
            }
            _wired = true;
            if (openSettingsButton != null)
            {
                openSettingsButton.onClick.AddListener(OpenSettings);
            }
            if (closeSettingsButton != null)
            {
                closeSettingsButton.onClick.AddListener(CloseSettings);
            }
        }
    }
}
