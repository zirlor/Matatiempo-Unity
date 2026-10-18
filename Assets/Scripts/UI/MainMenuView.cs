using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Matatiempo.Core;
using Matatiempo.Monetization;

namespace Matatiempo.UI
{
    public class MainMenuView : View
    {
        private RectTransform _levelGridRect;
        private GridLayoutGroup _levelGrid;
        private int _maxLevel;
        private MainMenuUIManager _manager;
        private LevelSelectionUI _levelSelection;
        private TextMeshProUGUI _consentStateLabel;
        private InstructionsModal _instructionsModal;

        protected override void Build()
        {
            var root = (RectTransform)transform;
            BuildBackground(root);

            _manager = GetComponent<MainMenuUIManager>();
            if (_manager == null)
            {
                _manager = gameObject.AddComponent<MainMenuUIManager>();
            }

            _levelSelection = GetComponent<LevelSelectionUI>();
            if (_levelSelection == null)
            {
                _levelSelection = gameObject.AddComponent<LevelSelectionUI>();
            }

            TextMeshProUGUI livesText = BuildTopHud(root, out Button settingsButton);
            BuildLevelArea(root);
            GameObject modal = BuildSettingsModal(root, out Button closeButton);
            modal.SetActive(false);
            _manager.Initialize(modal, livesText, settingsButton, closeButton);
            BuildConsentModal(root);
            BuildInstructionsModal(root);
        }

        private TextMeshProUGUI BuildTopHud(RectTransform root, out Button settingsButton)
        {
            RectTransform hud;
            RectTransform livesBox;
            Vector2 hudLeftMin;
            Vector2 hudLeftMax;
            Vector2 livesSize;
            Vector2 rewardSize;
            int rewardFont;
            float iconX;
            float iconSize;
            Vector2 livesTextPos;
            Vector2 livesTextSize;
            int livesFont;
            Vector2 settingsMin;
            Vector2 settingsMax;
            int settingsFont;

            if (IsLandscape)
            {
                hud = UIUtils.CreateRect("TopHUD", root, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -96), Vector2.zero);
                hudLeftMin = new Vector2(24, -21);
                hudLeftMax = new Vector2(358, 21);
                livesSize = new Vector2(204, 42);
                rewardSize = new Vector2(120, 42);
                rewardFont = 26;
                iconX = -62f;
                iconSize = 44f;
                livesTextPos = new Vector2(25, 0);
                livesTextSize = new Vector2(130, 60);
                livesFont = 32;
                settingsMin = new Vector2(-88, -32);
                settingsMax = new Vector2(-24, 32);
                settingsFont = 40;
            }
            else
            {
                hud = UIUtils.CreateRect("TopHUD", root, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -80), Vector2.zero);
                hudLeftMin = new Vector2(20, -19);
                hudLeftMax = new Vector2(320, 19);
                livesSize = new Vector2(190, 38);
                rewardSize = new Vector2(100, 38);
                rewardFont = 20;
                iconX = -57f;
                iconSize = 40f;
                livesTextPos = new Vector2(22, 0);
                livesTextSize = new Vector2(120, 56);
                livesFont = 26;
                settingsMin = new Vector2(-76, -28);
                settingsMax = new Vector2(-20, 28);
                settingsFont = 34;
            }

            RectTransform hudLeft = UIUtils.CreateRect("HudLeft", hud, new Vector2(0, 0.5f), new Vector2(0, 0.5f), hudLeftMin, hudLeftMax);
            HorizontalLayoutGroup layout = hudLeft.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 10;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            livesBox = UIUtils.CreateRect("Lives", hudLeft, Vector2.zero, Vector2.zero, Vector2.zero, livesSize);
            LayoutElement livesLayout = livesBox.gameObject.AddComponent<LayoutElement>();
            livesLayout.preferredWidth = livesSize.x;
            livesLayout.preferredHeight = livesSize.y;

            RectTransform iconRect = UIUtils.CreateCenteredRect("LivesIcon", livesBox, new Vector2(iconX, 0), new Vector2(iconSize, iconSize));
            Image icon = iconRect.gameObject.AddComponent<Image>();
            icon.color = UIUtils.PaperAccent;
            UIUtils.ApplySprite(icon, "Sprites/UI/PaperStyle/icon_star");

            TextMeshProUGUI livesText = UIUtils.CreateCenteredText("LivesText", livesBox, "x3", livesFont, UIUtils.TextColor, livesTextPos, livesTextSize);

            RewardAdButton.Create(hudLeft, "RewardedAdButton", "+1", rewardFont, rewardSize);

            Vector2 infoMin;
            Vector2 infoMax;
            if (IsLandscape)
            {
                infoMin = new Vector2(-160, -32);
                infoMax = new Vector2(-96, 32);
            }
            else
            {
                infoMin = new Vector2(-140, -28);
                infoMax = new Vector2(-84, 28);
            }
            Button infoButton = UIUtils.CreateButton("InstructionsButton", hud, "?", settingsFont, UIUtils.TextColor, new Vector2(1, 0.5f), new Vector2(1, 0.5f), infoMin, infoMax, () => _instructionsModal?.Open());
            UIUtils.ApplyPaperButton(infoButton.image);
            UIUtils.AddPaperShadow(infoButton.gameObject);

            settingsButton = UIUtils.CreateButton("SettingsButton", hud, "···", settingsFont, UIUtils.TextColor, new Vector2(1, 0.5f), new Vector2(1, 0.5f), settingsMin, settingsMax, () => _manager.OpenSettings());
            return livesText;
        }

        private void BuildLevelArea(RectTransform root)
        {
            if (IsLandscape)
            {
                UIUtils.CreateCenteredText("Title", root, "MATATIEMPO", 96, UIUtils.TextColor, new Vector2(0, 280), new Vector2(900, 120));
                UIUtils.CreateCenteredText("Subtitle", root, LocalizationManager.Instance.GetText("subtitle"), 52, UIUtils.PrimaryColor, new Vector2(0, 180), new Vector2(700, 80));
                UIUtils.CreateCenteredText("Hint", root, LocalizationManager.Instance.GetText("hint"), 30, UIUtils.TextColor, new Vector2(0, 110), new Vector2(900, 60));

                _levelGridRect = UIUtils.CreateCenteredRect("LevelGrid", root, new Vector2(0, -70), new Vector2(900, 140));
                _levelGrid = _levelGridRect.gameObject.AddComponent<GridLayoutGroup>();
                _levelGrid.startCorner = GridLayoutGroup.Corner.UpperLeft;
                _levelGrid.startAxis = GridLayoutGroup.Axis.Horizontal;
                _levelGrid.childAlignment = TextAnchor.MiddleCenter;
                _levelGrid.spacing = new Vector2(20, 20);
                _levelGrid.cellSize = new Vector2(150, 100);
                _levelGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            }
            else
            {
                UIUtils.CreateCenteredText("Title", root, "MATATIEMPO", 46, UIUtils.TextColor, new Vector2(0, 430), new Vector2(410, 66));
                UIUtils.CreateCenteredText("Subtitle", root, LocalizationManager.Instance.GetText("subtitle"), 26, UIUtils.PrimaryColor, new Vector2(0, 388), new Vector2(360, 38));
                UIUtils.CreateCenteredText("Hint", root, LocalizationManager.Instance.GetText("hint"), 18, UIUtils.TextColor, new Vector2(0, 352), new Vector2(410, 34));

                _levelGridRect = UIUtils.CreateCenteredRect("LevelGrid", root, new Vector2(0, 10), new Vector2(410, 300));
                _levelGrid = _levelGridRect.gameObject.AddComponent<GridLayoutGroup>();
                _levelGrid.startCorner = GridLayoutGroup.Corner.UpperLeft;
                _levelGrid.startAxis = GridLayoutGroup.Axis.Horizontal;
                _levelGrid.childAlignment = TextAnchor.MiddleCenter;
                _levelGrid.spacing = new Vector2(20, 13);
                _levelGrid.cellSize = new Vector2(190, 82);
                _levelGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            }

            RebuildLevels();
        }

        private GameObject BuildSettingsModal(RectTransform root, out Button closeButton)
        {
            RectTransform modal = UIUtils.CreateRect("SettingsModal", root, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            UIUtils.CreateImage(modal, "Dim", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0.6f));

            Vector2 windowSize;
            Vector2 titlePos;
            Vector2 titleSize;
            int titleFont;
            Vector2 languagePos;
            Vector2 languageSize;
            int languageFont;
            Vector2 groupPos;
            Vector2 groupSize;
            float langWidth;
            float langHeight;
            int langFont;
            float langSpacing;

            if (IsLandscape)
            {
                windowSize = new Vector2(760, 560);
                titlePos = new Vector2(0, 185);
                titleSize = new Vector2(500, 70);
                titleFont = 44;
                languagePos = new Vector2(0, 110);
                languageSize = new Vector2(300, 50);
                languageFont = 26;
                groupPos = new Vector2(0, 20);
                groupSize = new Vector2(700, 110);
                langWidth = 150f;
                langHeight = 80f;
                langFont = 28;
                langSpacing = 14f;
            }
            else
            {
                windowSize = new Vector2(440, 620);
                titlePos = new Vector2(0, 225);
                titleSize = new Vector2(380, 60);
                titleFont = 36;
                languagePos = new Vector2(0, 165);
                languageSize = new Vector2(300, 44);
                languageFont = 22;
                groupPos = new Vector2(0, 80);
                groupSize = new Vector2(420, 90);
                langWidth = 95f;
                langHeight = 60f;
                langFont = 18;
                langSpacing = 10f;
            }

            RectTransform window = UIUtils.CreateCenteredRect("ModalWindow", modal, Vector2.zero, windowSize);
            Image windowBg = window.gameObject.AddComponent<Image>();
            windowBg.color = UIUtils.PanelColor;
            UIUtils.AddPaperShadow(window.gameObject);

            UIUtils.CreateCenteredText("ModalTitle", window, LocalizationManager.Instance.GetText("settings"), titleFont, UIUtils.TextColor, titlePos, titleSize);
            UIUtils.CreateCenteredText("LanguageTitle", window, LocalizationManager.Instance.GetText("language"), languageFont, UIUtils.PrimaryColor, languagePos, languageSize);

            closeButton = UIUtils.CreateButton("CloseButton", window, "X", 30, UIUtils.TextColor, Vector2.one, Vector2.one, new Vector2(-72, -72), new Vector2(-16, -16), () => _manager.CloseSettings());

            RectTransform group = UIUtils.CreateCenteredRect("LanguageGroup", window, groupPos, groupSize);
            HorizontalLayoutGroup layout = group.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = langSpacing;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            BuildLanguageButtons(group, langWidth, langHeight, langFont);
            BuildConsentSection(window);
            return modal.gameObject;
        }

        private void BuildLanguageButtons(RectTransform parent, float buttonWidth, float buttonHeight, int fontSize)
        {
            string[] codes = { "ES", "EN", "PT", "FR" };
            string[] languages = { "es", "en", "pt", "fr" };
            for (int i = 0; i < codes.Length; i++)
            {
                int index = i;
                Button button = UIUtils.CreateCenteredButton($"Lang_{codes[i]}", parent, codes[i], fontSize, Vector2.zero, new Vector2(buttonWidth, buttonHeight), () => SelectLanguage(languages[index]));
                LayoutElement element = button.gameObject.AddComponent<LayoutElement>();
                element.preferredWidth = buttonWidth;
                element.preferredHeight = buttonHeight;
            }
        }

        private void SelectLanguage(string languageCode)
        {
            if (LocalizationManager.Instance.SetLanguage(languageCode) && UIManager.Instance != null)
            {
                UIManager.Instance.RebuildAllViews();
            }
        }

        private void BuildConsentSection(RectTransform window)
        {
            Vector2 labelPos;
            Vector2 labelSize;
            Vector2 buttonPos;
            Vector2 buttonSize;
            int labelFont;
            int buttonFont;

            if (IsLandscape)
            {
                labelPos = new Vector2(0, -105);
                labelSize = new Vector2(600, 40);
                labelFont = 20;
                buttonPos = new Vector2(0, -160);
                buttonSize = new Vector2(320, 76);
                buttonFont = 22;
            }
            else
            {
                labelPos = new Vector2(0, -15);
                labelSize = new Vector2(400, 36);
                labelFont = 18;
                buttonPos = new Vector2(0, -65);
                buttonSize = new Vector2(380, 64);
                buttonFont = 20;
            }

            UIUtils.CreateCenteredText("ConsentTitle", window, LocalizationManager.Instance.GetText("consent_label"), labelFont, UIUtils.PrimaryColor, labelPos, labelSize);

            string state = LocalizationManager.Instance.GetText(MonetizationManager.GetConsent() ? "consent_enabled" : "consent_disabled");
            Button toggle = UIUtils.CreateCenteredButton("ConsentToggle", window, state, buttonFont, buttonPos, buttonSize, ToggleConsent);
            _consentStateLabel = toggle.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
        }

        private void ToggleConsent()
        {
            bool next = !MonetizationManager.GetConsent();
            MonetizationManager.SetConsent(next);
            if (_consentStateLabel != null)
            {
                _consentStateLabel.text = LocalizationManager.Instance.GetText(next ? "consent_enabled" : "consent_disabled");
            }
        }

        private void BuildConsentModal(RectTransform root)
        {
            if (MonetizationManager.HasConsentOfUser)
            {
                return;
            }

            RectTransform modal = UIUtils.CreateRect("ConsentModal", root, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            UIUtils.CreateImage(modal, "Dim", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0.6f));

            Vector2 windowSize;
            Vector2 titlePos;
            Vector2 titleSize;
            Vector2 bodyPos;
            Vector2 bodySize;
            Vector2 acceptPos;
            Vector2 acceptSize;
            Vector2 declinePos;
            Vector2 declineSize;
            int titleFont;
            int acceptFont;
            int buttonFont;

            if (IsLandscape)
            {
                windowSize = new Vector2(760, 430);
                titlePos = new Vector2(0, 150);
                titleSize = new Vector2(520, 60);
                titleFont = 40;
                bodyPos = new Vector2(0, 25);
                bodySize = new Vector2(620, 120);
                acceptPos = new Vector2(0, -80);
                acceptSize = new Vector2(320, 80);
                acceptFont = 26;
                declinePos = new Vector2(0, -185);
                declineSize = new Vector2(280, 64);
                buttonFont = 22;
            }
            else
            {
                windowSize = new Vector2(440, 540);
                titlePos = new Vector2(0, 175);
                titleSize = new Vector2(400, 56);
                titleFont = 32;
                bodyPos = new Vector2(0, 60);
                bodySize = new Vector2(380, 170);
                acceptPos = new Vector2(0, -110);
                acceptSize = new Vector2(380, 76);
                acceptFont = 24;
                declinePos = new Vector2(0, -205);
                declineSize = new Vector2(300, 60);
                buttonFont = 20;
            }

            RectTransform window = UIUtils.CreateCenteredRect("ModalWindow", modal, Vector2.zero, windowSize);
            Image windowBg = window.gameObject.AddComponent<Image>();
            windowBg.color = UIUtils.PanelColor;
            UIUtils.AddPaperShadow(window.gameObject);

            UIUtils.CreateCenteredText("ModalTitle", window, LocalizationManager.Instance.GetText("consent_title"), titleFont, UIUtils.TextColor, titlePos, titleSize);

            TextMeshProUGUI bodyText = UIUtils.CreateCenteredText("ModalBody", window, LocalizationManager.Instance.GetText("consent_body"), 22, UIUtils.TextColor, bodyPos, bodySize);
            bodyText.enableWordWrapping = true;
            bodyText.enableAutoSizing = true;
            bodyText.fontSizeMin = 16;
            bodyText.fontSizeMax = 22;

            UIUtils.CreateCenteredButton("AcceptButton", window, LocalizationManager.Instance.GetText("consent_accept"), acceptFont, acceptPos, acceptSize, () => ResolveConsent(modal.gameObject, true));
            UIUtils.CreateCenteredButton("DeclineButton", window, LocalizationManager.Instance.GetText("consent_decline"), buttonFont, declinePos, declineSize, () => ResolveConsent(modal.gameObject, false));
        }

        private void ResolveConsent(GameObject modal, bool consent)
        {
            MonetizationManager.SetConsent(consent);
            if (modal != null)
            {
                modal.SetActive(false);
            }
        }

        private void BuildInstructionsModal(RectTransform root)
        {
            var host = new GameObject("InstructionsModalHost", typeof(RectTransform));
            host.transform.SetParent(root, false);
            var rect = (RectTransform)host.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            _instructionsModal = host.AddComponent<InstructionsModal>();
            _instructionsModal.Build(root);
        }

        private int ResolveMaxLevel()
        {
            WordDictionaryData dictionary = LocalizationManager.Instance.Active;
            return dictionary != null && dictionary.levels != null && dictionary.levels.Length > 0
                ? dictionary.levels.Length
                : LevelProgression.MaxLevel;
        }

        private void RebuildLevels()
        {
            if (_levelGridRect == null || _levelGrid == null)
            {
                return;
            }
            for (int i = _levelGridRect.childCount - 1; i >= 0; i--)
            {
                Object.Destroy(_levelGridRect.GetChild(i).gameObject);
            }
            _maxLevel = ResolveMaxLevel();
            _levelGrid.constraintCount = IsLandscape ? _maxLevel : 2;
            float cellWidth = IsLandscape ? 150f : 190f;
            float cellHeight = IsLandscape ? 100f : 82f;
            int fontSize = IsLandscape ? 32 : 24;
            Button[] levelButtons = new Button[_maxLevel];
            for (int level = 1; level <= _maxLevel; level++)
            {
                int captured = level;
                levelButtons[level - 1] = UIUtils.CreateCenteredButton($"Level_{level}", _levelGridRect, string.Format(LocalizationManager.Instance.GetText("level_format"), level), fontSize, Vector2.zero, new Vector2(cellWidth, cellHeight), () =>
                {
                    GameManager.Instance.StartLevel(captured);
                });
            }
            _levelSelection.ApplyLevels(levelButtons);
        }
    }
}
