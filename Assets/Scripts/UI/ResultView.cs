using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Matatiempo.Core;
using Matatiempo.Monetization;

namespace Matatiempo.UI
{
    public class ResultView : View
    {
        private TextMeshProUGUI _title;
        private Button _nextButton;
        private Button _retryButton;
        private Button _resumeButton;
        private Button _addLifeButton;
        private Button _menuButton;

        private string _titleText = string.Empty;
        private int _resultLevel;
        private bool _resultLost;
        private bool _nextVisible;
        private bool _retryVisible;
        private bool _resumeVisible;
        private bool _addLifeVisible;
        private bool _menuVisible;

        private void Awake()
        {
            GameEvents.LevelCompleted += HandleLevelCompleted;
            GameEvents.GameLost += HandleGameLost;
        }

        private void OnDestroy()
        {
            GameEvents.LevelCompleted -= HandleLevelCompleted;
            GameEvents.GameLost -= HandleGameLost;
        }

        protected override void Build()
        {
            var root = (RectTransform)transform;
            Image background = UIUtils.CreateImage(root, "Background", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.976f, 0.949f, 0.878f, 0.97f));
            UIUtils.ApplySprite(background, "Sprites/UI/background");

            if (IsLandscape)
            {
                _title = UIUtils.CreateCenteredText("Title", root, string.Empty, 72, UIUtils.TextColor, new Vector2(0, 150), new Vector2(900, 100));
                _nextButton = UIUtils.CreateCenteredButton("Next", root, LocalizationManager.Instance.GetText("next_level"), 34, new Vector2(-340, -40), new Vector2(320, 90), () =>
                {
                    GameManager.Instance.StartLevel(GameManager.Instance.CurrentLevel + 1);
                });
                _retryButton = UIUtils.CreateCenteredButton("Retry", root, LocalizationManager.Instance.GetText("retry"), 34, new Vector2(0, -40), new Vector2(320, 90), () =>
                {
                    GameManager.Instance.StartLevel(GameManager.Instance.CurrentLevel);
                });
                _addLifeButton = UIUtils.CreateCenteredButton("AddLife", root, LocalizationManager.Instance.GetText("add_life"), 26, new Vector2(-340, -40), new Vector2(320, 90), () =>
                {
                    MonetizationManager.Instance.ShowRewardedAd(HandleAddLifeReward);
                });
                _resumeButton = UIUtils.CreateCenteredButton("Resume", root, LocalizationManager.Instance.GetText("resume_ad"), 26, new Vector2(0, 140), new Vector2(380, 70), () =>
                {
                    MonetizationManager.Instance.ShowRewardedAd(HandleReward);
                });
                _menuButton = UIUtils.CreateCenteredButton("Menu", root, LocalizationManager.Instance.GetText("menu_main"), 34, new Vector2(340, -40), new Vector2(320, 90), () =>
                {
                    GameManager.Instance.SetState(GameState.MainMenu);
                });
            }
            else
            {
                _title = UIUtils.CreateCenteredText("Title", root, string.Empty, 44, UIUtils.TextColor, new Vector2(0, 380), new Vector2(400, 66));
                _resumeButton = UIUtils.CreateCenteredButton("Resume", root, LocalizationManager.Instance.GetText("resume_ad"), 20, new Vector2(0, 235), new Vector2(400, 60), () =>
                {
                    MonetizationManager.Instance.ShowRewardedAd(HandleReward);
                });
                _nextButton = UIUtils.CreateCenteredButton("Next", root, LocalizationManager.Instance.GetText("next_level"), 24, new Vector2(0, 125), new Vector2(400, 60), () =>
                {
                    GameManager.Instance.StartLevel(GameManager.Instance.CurrentLevel + 1);
                });
                _retryButton = UIUtils.CreateCenteredButton("Retry", root, LocalizationManager.Instance.GetText("retry"), 24, new Vector2(0, 35), new Vector2(400, 60), () =>
                {
                    GameManager.Instance.StartLevel(GameManager.Instance.CurrentLevel);
                });
                _addLifeButton = UIUtils.CreateCenteredButton("AddLife", root, LocalizationManager.Instance.GetText("add_life"), 20, new Vector2(0, 125), new Vector2(400, 60), () =>
                {
                    MonetizationManager.Instance.ShowRewardedAd(HandleAddLifeReward);
                });
                _menuButton = UIUtils.CreateCenteredButton("Menu", root, LocalizationManager.Instance.GetText("menu_main"), 24, new Vector2(0, -55), new Vector2(400, 60), () =>
                {
                    GameManager.Instance.SetState(GameState.MainMenu);
                });
            }

            ApplyResult();
        }

        public override void Rebuild()
        {
            base.Rebuild();
            ApplyResult();
        }

        private void HandleReward(bool rewarded)
        {
            if (rewarded)
            {
                GameManager.Instance.StartLevel(GameManager.Instance.CurrentLevel);
            }
        }

        private void HandleAddLifeReward(bool rewarded)
        {
            if (!rewarded)
            {
                return;
            }
            int lives = MainMenuUIManager.AddLives(1);
            GameEvents.NotifyLivesChanged(lives);
            if (_addLifeButton != null)
            {
                _addLifeButton.gameObject.SetActive(false);
            }
            if (_retryButton != null && _retryVisible && LivesManager.HasLives())
            {
                _retryButton.gameObject.SetActive(true);
            }
            if (_title != null)
            {
                _title.text = LocalizationManager.Instance.GetText("title_add_life");
            }
        }

        private void HandleLevelCompleted(int level)
        {
            WordDictionaryData dictionary = LocalizationManager.Instance.Active;
            int maxLevel = dictionary != null && dictionary.levels != null && dictionary.levels.Length > 0
                ? dictionary.levels.Length
                : LevelProgression.MaxLevel;

            _resultLevel = level;
            _resultLost = false;
            _titleText = string.Format(LocalizationManager.Instance.GetText("title_level_completed"), level);
            _nextVisible = level < maxLevel;
            _retryVisible = false;
            _resumeVisible = false;
            _addLifeVisible = false;
            _menuVisible = true;
            ApplyResult();
        }

        private void HandleGameLost()
        {
            _resultLevel = 0;
            _resultLost = true;
            _titleText = LocalizationManager.Instance.GetText("title_lost");
            _nextVisible = false;
            _retryVisible = true;
            _resumeVisible = true;
            _addLifeVisible = true;
            _menuVisible = true;
            ApplyResult();
        }

        private void ApplyResult()
        {
            if (_resultLevel > 0)
            {
                _title.text = string.Format(LocalizationManager.Instance.GetText("title_level_completed"), _resultLevel);
            }
            else if (_resultLost)
            {
                _title.text = LocalizationManager.Instance.GetText("title_lost");
            }
            else
            {
                _title.text = _titleText;
            }
            bool canPlay = LivesManager.HasLives();
            bool resumeEnabled = _resumeVisible
                && canPlay
                && MonetizationManager.Instance != null
                && MonetizationManager.Instance.Enabled
                && MonetizationManager.Instance.IsRewardedAvailable;
            bool addLifeEnabled = _addLifeVisible
                && MonetizationManager.Instance != null
                && MonetizationManager.Instance.Enabled
                && MonetizationManager.Instance.IsRewardedAvailable;
            _nextButton.gameObject.SetActive(_nextVisible && canPlay);
            _retryButton.gameObject.SetActive(_retryVisible && canPlay);
            _resumeButton.gameObject.SetActive(resumeEnabled);
            _addLifeButton.gameObject.SetActive(addLifeEnabled);
            _menuButton.gameObject.SetActive(_menuVisible);
        }
    }
}