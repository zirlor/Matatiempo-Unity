using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Matatiempo.Core;
using Matatiempo.Gameplay;

namespace Matatiempo.UI
{
    public class GameView : View
    {
        private TextMeshProUGUI _levelText;
        private TextMeshProUGUI _timerText;
        private TextMeshProUGUI _wordText;
        private TextMeshProUGUI _livesDisplayText;
        private TextMeshProUGUI _pauseLevelText;
        private GameObject _pauseModal;
        private Button _resumeButton;
        private Button _quitButton;
        private PauseManager _pauseManager;
        private TextMeshProUGUI _livesText;
        private InstructionsModal _instructionsModal;
        private HintModal _hintModal;
        private readonly Dictionary<char, Button> _keys = new Dictionary<char, Button>();
        private readonly List<Image> _figureParts = new List<Image>();

        private void Awake()
        {
            GameEvents.WordStarted += HandleWordStarted;
            GameEvents.LetterGuessed += HandleLetterGuessed;
            GameEvents.AttemptsChanged += HandleAttemptsChanged;
            GameEvents.TimeChanged += HandleTimeChanged;
            GameEvents.WordCompleted += HandleWordCompleted;
        }

        private void OnDestroy()
        {
            GameEvents.WordStarted -= HandleWordStarted;
            GameEvents.LetterGuessed -= HandleLetterGuessed;
            GameEvents.AttemptsChanged -= HandleAttemptsChanged;
            GameEvents.TimeChanged -= HandleTimeChanged;
            GameEvents.WordCompleted -= HandleWordCompleted;
        }

        protected override void Build()
        {
            var root = (RectTransform)transform;
            BuildBackground(root);
            _keys.Clear();
            _figureParts.Clear();

            BuildTopHud(root);

            if (IsLandscape)
            {
                _levelText = UIUtils.CreateCenteredText("Level", root, string.Format(LocalizationManager.Instance.GetText("level_format"), 1), 36, UIUtils.PrimaryColor, new Vector2(0, 470), new Vector2(500, 60));
                _timerText = UIUtils.CreateCenteredText("Timer", root, string.Format(LocalizationManager.Instance.GetText("time_format"), 0), 44, UIUtils.TextColor, new Vector2(600, 470), new Vector2(400, 60));
                _wordText = UIUtils.CreateCenteredText("Word", root, string.Empty, 66, UIUtils.TextColor, new Vector2(0, 170), new Vector2(1600, 110));
                _livesText = UIUtils.CreateCenteredText("Lives", root, string.Empty, 32, UIUtils.TextColor, new Vector2(-720, -150), new Vector2(420, 60));
                BuildFigure(root, new Vector2(-640, 60), new Vector2(180, 210));
                BuildKeyboard(root, new Vector2(0, -250), new Vector2(1250, 320), 100.8f, 86.4f, 19.2f, 100f, 46);
            }
            else
            {
                _levelText = UIUtils.CreateCenteredText("Level", root, string.Format(LocalizationManager.Instance.GetText("level_format"), 1), 24, UIUtils.PrimaryColor, new Vector2(-95, 448), new Vector2(130, 32));
                _timerText = UIUtils.CreateCenteredText("Timer", root, string.Format(LocalizationManager.Instance.GetText("time_format"), 0), 24, UIUtils.TextColor, new Vector2(95, 448), new Vector2(130, 30));
                _livesText = UIUtils.CreateCenteredText("Lives", root, string.Empty, 20, UIUtils.TextColor, new Vector2(0, 404), new Vector2(340, 32));
                _wordText = UIUtils.CreateCenteredText("Word", root, string.Empty, 34, UIUtils.TextColor, new Vector2(0, 330), new Vector2(410, 56));
                _wordText.enableAutoSizing = true;
                _wordText.fontSizeMin = 18f;
                _wordText.fontSizeMax = 40f;
                BuildFigure(root, new Vector2(0, 170), new Vector2(190, 200));
                BuildKeyboard(root, new Vector2(0, -320), new Vector2(410, 210), 43.2f, 40.8f, 6f, 52f, 22);
            }

            _wordText.enableAutoSizing = true;
            _wordText.fontSizeMin = IsLandscape ? 26f : 18f;
            _wordText.fontSizeMax = IsLandscape ? 66f : 40f;
            _wordText.textWrappingMode = TextWrappingModes.NoWrap;
            _wordText.overflowMode = TextOverflowModes.Overflow;
            _wordText.alignment = TextAlignmentOptions.Center;

            BuildPauseOverlay(root);
            _pauseManager = GetComponent<PauseManager>();
            if (_pauseManager == null)
            {
                _pauseManager = gameObject.AddComponent<PauseManager>();
            }
            BuildInstructionsModal(root);
            BuildHintModal(root);
            _pauseManager.Setup(_pauseModal, _livesDisplayText, _pauseLevelText, _resumeButton, _quitButton);
            _pauseManager.UpdateLives(LivesManager.GetLives());

            ApplyGameState();
        }

        private void BuildTopHud(RectTransform root)
        {
            RectTransform hud = UIUtils.CreateRect("TopHUD", root, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -88), Vector2.zero);

            Vector2 livesSize = IsLandscape ? new Vector2(360, 40) : new Vector2(100, 28);
            Vector2 rewardSize = IsLandscape ? new Vector2(150, 40) : new Vector2(110, 28);
            int rewardFont = IsLandscape ? 30 : 16;
            Vector2 hudLeftMin = IsLandscape ? new Vector2(24, -20) : new Vector2(16, -14);
            Vector2 hudLeftMax = IsLandscape ? new Vector2(544, 20) : new Vector2(236, 14);

            RectTransform hudLeft = UIUtils.CreateRect("HudLeft", hud, new Vector2(0, 0.5f), new Vector2(0, 0.5f), hudLeftMin, hudLeftMax);
            HorizontalLayoutGroup layout = hudLeft.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 10;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            _livesDisplayText = UIUtils.CreateCenteredText("LivesDisplay", hudLeft, string.Format(LocalizationManager.Instance.GetText("lives_format"), 0), IsLandscape ? 30 : 16, UIUtils.TextColor, Vector2.zero, livesSize);
            LayoutElement livesLayout = _livesDisplayText.gameObject.AddComponent<LayoutElement>();
            livesLayout.preferredWidth = livesSize.x;
            livesLayout.preferredHeight = livesSize.y;

            RewardAdButton.Create(hudLeft, "RewardedAdButton", "+1", rewardFont, rewardSize);

            if (IsLandscape)
            {
                Button hintButton = UIUtils.CreateButton("HintButton", hud, GetHintLabel(), 24, UIUtils.TextColor, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-248, -32), new Vector2(-160, 32), ShowHint);
                UIUtils.ApplyPaperButton(hintButton.image);
                UIUtils.AddPaperShadow(hintButton.gameObject);
                Button infoButton = UIUtils.CreateButton("InstructionsButton", hud, "?", 30, UIUtils.TextColor, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-160, -32), new Vector2(-96, 32), () => _instructionsModal?.Open());
                UIUtils.ApplyPaperButton(infoButton.image);
                UIUtils.AddPaperShadow(infoButton.gameObject);
                Button pauseButton = UIUtils.CreateButton("PauseButton", hud, "II", 30, UIUtils.TextColor, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-88, -32), new Vector2(-24, 32), () =>
                {
                    if (_pauseManager != null)
                    {
                        _pauseManager.TogglePause();
                    }
                });
                UIUtils.ApplyPaperButton(pauseButton.image);
                UIUtils.AddPaperShadow(pauseButton.gameObject);
            }
            else
            {
                Button hintButton = UIUtils.CreateButton("HintButton", hud, GetHintLabel(), 16, UIUtils.TextColor, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-192, -28), new Vector2(-136, 28), ShowHint);
                UIUtils.ApplyPaperButton(hintButton.image);
                UIUtils.AddPaperShadow(hintButton.gameObject);
                Button infoButton = UIUtils.CreateButton("InstructionsButton", hud, "?", 18, UIUtils.TextColor, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-128, -28), new Vector2(-80, 28), () => _instructionsModal?.Open());
                UIUtils.ApplyPaperButton(infoButton.image);
                UIUtils.AddPaperShadow(infoButton.gameObject);
                Button pauseButton = UIUtils.CreateButton("PauseButton", hud, "II", 18, UIUtils.TextColor, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-64, -28), new Vector2(-16, 28), () =>
                {
                    if (_pauseManager != null)
                    {
                        _pauseManager.TogglePause();
                    }
                });
                UIUtils.ApplyPaperButton(pauseButton.image);
                UIUtils.AddPaperShadow(pauseButton.gameObject);
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

        private void BuildHintModal(RectTransform root)
        {
            var host = new GameObject("HintModalHost", typeof(RectTransform));
            host.transform.SetParent(root, false);
            var rect = (RectTransform)host.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            _hintModal = host.AddComponent<HintModal>();
            _hintModal.Build(root);
        }

        private string GetHintLabel()
        {
            return LocalizationManager.Instance != null
                ? LocalizationManager.Instance.GetText("clue_title")
                : "Pista";
        }

        private void ShowHint()
        {
            string hint = WordManager.Instance != null
                ? WordManager.Instance.CurrentClue
                : null;
            if (_hintModal != null)
            {
                _hintModal.Open(hint);
            }
        }

        private void BuildPauseOverlay(RectTransform root)
        {
            RectTransform modal = UIUtils.CreateRect("PauseModal", root, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            UIUtils.CreateImage(modal, "Dim", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0.8f));
            _pauseModal = modal.gameObject;

            Vector2 windowSize;
            Vector2 levelPos;
            Vector2 levelSize;
            int levelFont;
            Vector2 resumeSize;
            Vector2 quitSize;
            int buttonFont;

            if (IsLandscape)
            {
                windowSize = new Vector2(760, 560);
                levelPos = new Vector2(0, 185);
                levelSize = new Vector2(600, 70);
                levelFont = 46;
                resumeSize = new Vector2(360, 90);
                quitSize = new Vector2(300, 72);
                buttonFont = 34;
            }
            else
            {
                windowSize = new Vector2(440, 620);
                levelPos = new Vector2(0, 225);
                levelSize = new Vector2(400, 60);
                levelFont = 38;
                resumeSize = new Vector2(320, 84);
                quitSize = new Vector2(280, 70);
                buttonFont = 28;
            }

            RectTransform window = UIUtils.CreateCenteredRect("PaperWindow", modal, Vector2.zero, windowSize);
            Image windowBg = window.gameObject.AddComponent<Image>();
            windowBg.color = UIUtils.PanelColor;
            UIUtils.AddPaperShadow(window.gameObject);

            _pauseLevelText = UIUtils.CreateCenteredText("LevelText", window, string.Format(LocalizationManager.Instance.GetText("level_format"), 1), levelFont, UIUtils.TextColor, levelPos, levelSize);

            _resumeButton = UIUtils.CreateCenteredButton("ResumeButton", window, LocalizationManager.Instance.GetText("resume"), buttonFont, new Vector2(0, 40), resumeSize, () =>
            {
                if (_pauseManager != null)
                {
                    _pauseManager.ResumeGame();
                }
            });
            _quitButton = UIUtils.CreateCenteredButton("QuitToMenuButton", window, LocalizationManager.Instance.GetText("quit_to_menu"), buttonFont, new Vector2(0, -80), quitSize, () =>
            {
                if (_pauseManager != null)
                {
                    _pauseManager.QuitToMenu();
                }
            });

            modal.gameObject.SetActive(false);
        }

        private void ApplyGameState()
        {
            if (GameManager.Instance == null)
            {
                return;
            }

            // Cachear singletons y evaluar IsSolved una sola vez (no 28x en el loop de teclas)
            LocalizationManager localization = LocalizationManager.Instance;
            HangmanGameManager hangman = HangmanGameManager.Instance;
            _levelText.text = string.Format(localization.GetText("level_format"), GameManager.Instance.CurrentLevel);
            _timerText.text = TimerManager.Instance != null ? string.Format(localization.GetText("time_format"), TimerManager.Instance.RemainingSeconds) : string.Empty;

            if (hangman != null)
            {
                _wordText.text = hangman.DisplayWord;
                _livesText.text = string.Format(localization.GetText("attempts_format"), hangman.RemainingGuesses);
                SetFigureStage(hangman.BadGuesses);
                bool solved = hangman.IsSolved;
                foreach (KeyValuePair<char, Button> pair in _keys)
                {
                    pair.Value.interactable = !solved && !hangman.IsLetterGuessed(pair.Key);
                }
            }
        }

        public override void RefreshLocalization()
        {
            ApplyGameState();
        }

        private void BuildFigure(RectTransform root, Vector2 figurePosition, Vector2 figureSize)
        {
            RectTransform container = UIUtils.CreateCenteredRect("Figure", root, figurePosition, figureSize);
            _figureParts.Clear();

            Image baseSprite = UIUtils.CreateCenteredImage("Base", container, new Vector2(-30, -88), new Vector2(160, 10), UIUtils.FigureColor);
            UIUtils.ApplySprite(baseSprite, "Sprites/Hangman/base");
            Image pole = UIUtils.CreateCenteredImage("Pole", container, new Vector2(-82, -42), new Vector2(10, 92), UIUtils.FigureColor);
            UIUtils.ApplySprite(pole, "Sprites/Hangman/pole");
            Image beam = UIUtils.CreateCenteredImage("Beam", container, new Vector2(-20, 4), new Vector2(114, 10), UIUtils.FigureColor);
            UIUtils.ApplySprite(beam, "Sprites/Hangman/beam");
            Image rope = UIUtils.CreateCenteredImage("Rope", container, new Vector2(37, -8), new Vector2(6, 24), UIUtils.FigureColor);
            UIUtils.ApplySprite(rope, "Sprites/Hangman/rope");

            Image head = UIUtils.CreateCenteredImage("Head", container, new Vector2(37, -24), new Vector2(18, 18), UIUtils.FigureColor);
            UIUtils.ApplySprite(head, "Sprites/Hangman/head");
            _figureParts.Add(head);
            Image body = UIUtils.CreateCenteredImage("Body", container, new Vector2(37, -52), new Vector2(8, 38), UIUtils.FigureColor);
            UIUtils.ApplySprite(body, "Sprites/Hangman/body");
            _figureParts.Add(body);

            Image leftArm = UIUtils.CreateCenteredImage("LeftArm", container, new Vector2(31, -44), new Vector2(16, 8), UIUtils.FigureColor);
            UIUtils.ApplySprite(leftArm, "Sprites/Hangman/arm");
            leftArm.rectTransform.localRotation = Quaternion.Euler(0, 0, 40);
            _figureParts.Add(leftArm);

            Image rightArm = UIUtils.CreateCenteredImage("RightArm", container, new Vector2(43, -44), new Vector2(16, 8), UIUtils.FigureColor);
            UIUtils.ApplySprite(rightArm, "Sprites/Hangman/arm");
            rightArm.rectTransform.localRotation = Quaternion.Euler(0, 0, -40);
            _figureParts.Add(rightArm);

            Image leftLeg = UIUtils.CreateCenteredImage("LeftLeg", container, new Vector2(31, -70), new Vector2(16, 8), UIUtils.FigureColor);
            UIUtils.ApplySprite(leftLeg, "Sprites/Hangman/leg");
            leftLeg.rectTransform.localRotation = Quaternion.Euler(0, 0, 35);
            _figureParts.Add(leftLeg);

            Image rightLeg = UIUtils.CreateCenteredImage("RightLeg", container, new Vector2(43, -70), new Vector2(16, 8), UIUtils.FigureColor);
            UIUtils.ApplySprite(rightLeg, "Sprites/Hangman/leg");
            rightLeg.rectTransform.localRotation = Quaternion.Euler(0, 0, -35);
            _figureParts.Add(rightLeg);

            SetFigureStage(0);
        }

        private void BuildKeyboard(RectTransform root, Vector2 keyboardPosition, Vector2 keyboardSize, float keyWidth, float keyHeight, float spacing, float startY, int fontSize)
        {
            RectTransform keyboard = UIUtils.CreateCenteredRect("Keyboard", root, keyboardPosition, keyboardSize);
            string[] rows = LocalizationManager.Instance.ActiveLanguage == "es"
                ? new[] { "QWERTYUIOP", "ASDFGHJKLÑ", "ZXCVBNM" }
                : new[] { "QWERTYUIOP", "ASDFGHJKL", "ZXCVBNM" };

            for (int row = 0; row < rows.Length; row++)
            {
                string keys = rows[row];
                float totalWidth = keys.Length * keyWidth + (keys.Length - 1) * spacing;
                float startX = -totalWidth * 0.5f + keyWidth * 0.5f;
                float y = startY - row * (keyHeight + spacing);
                for (int i = 0; i < keys.Length; i++)
                {
                    char letter = keys[i];
                    float x = startX + i * (keyWidth + spacing);
                    Button button = UIUtils.CreateCenteredButton($"Key_{letter}", keyboard, letter.ToString(), fontSize, new Vector2(x, y), new Vector2(keyWidth, keyHeight), null);
                    // Handler sin allocations (antes: closure () => PressKey(letter))
                    var keyHandler = button.gameObject.AddComponent<KeyButtonHandler>();
                    keyHandler.SetKey(letter);
                    button.onClick.AddListener(keyHandler.OnClick);
                    UIUtils.ApplyPaperKey(button.image);
                    UIUtils.AddPaperShadow(button.gameObject);
                    _keys.Add(char.ToUpperInvariant(letter), button);
                }
            }
        }

        private void HandleWordStarted(string word)
        {
            _levelText.text = string.Format(LocalizationManager.Instance.GetText("level_format"), GameManager.Instance.CurrentLevel);
            _wordText.text = HangmanGameManager.Instance.DisplayWord;
            _livesText.text = string.Format(LocalizationManager.Instance.GetText("attempts_format"), HangmanGameManager.MaxBadGuesses);
            SetFigureStage(0);
            foreach (Button key in _keys.Values)
            {
                key.interactable = true;
            }
        }

        private void HandleLetterGuessed(char letter, bool correct)
        {
            if (correct)
            {
                _wordText.text = HangmanGameManager.Instance.DisplayWord;
            }
            if (_keys.TryGetValue(char.ToUpperInvariant(letter), out Button key))
            {
                key.interactable = false;
            }
        }

        private void HandleAttemptsChanged(int badGuesses)
        {
            _livesText.text = string.Format(LocalizationManager.Instance.GetText("attempts_format"), HangmanGameManager.MaxBadGuesses - badGuesses);
            SetFigureStage(badGuesses);
        }

        private void HandleTimeChanged(int remaining, int total)
        {
            _timerText.text = string.Format(LocalizationManager.Instance.GetText("time_format"), remaining);
        }

        private void HandleWordCompleted(string word)
        {
            foreach (Button key in _keys.Values)
            {
                key.interactable = false;
            }
        }

        private void SetFigureStage(int stage)
        {
            for (int i = 0; i < _figureParts.Count; i++)
            {
                _figureParts[i].gameObject.SetActive(i < stage);
            }
        }
    }
}