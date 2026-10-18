using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Matatiempo.Core;

namespace Matatiempo.UI
{
    public class InstructionsModal : MonoBehaviour
    {
        private GameObject _modal;
        private bool IsLandscape => Screen.width > Screen.height;

        public void Build(RectTransform parent)
        {
            if (_modal != null)
            {
                Destroy(_modal);
            }

            RectTransform modal = UIUtils.CreateRect("InstructionsModal", parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            UIUtils.CreateImage(modal, "Dim", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0.6f));
            Button dimButton = modal.gameObject.AddComponent<Button>();
            dimButton.transition = Selectable.Transition.None;
            dimButton.onClick.AddListener(Close);

            _modal = modal.gameObject;

            Vector2 windowSize;
            Vector2 titlePos;
            Vector2 titleSize;
            int titleFont;
            Vector2[] stepPositions;
            Vector2 stepSize;
            int stepFont;
            Vector2 closePos;
            Vector2 closeSize;
            int closeFont;

            if (IsLandscape)
            {
                windowSize = new Vector2(860f, 540f);
                titlePos = new Vector2(0f, 215f);
                titleSize = new Vector2(700f, 60f);
                titleFont = 36;
                stepSize = new Vector2(780f, 70f);
                stepPositions = new[]
                {
                    new Vector2(0f, 110f),
                    new Vector2(0f, 32f),
                    new Vector2(0f, -46f),
                    new Vector2(0f, -124f)
                };
                stepFont = 20;
                closePos = new Vector2(0f, -220f);
                closeSize = new Vector2(240f, 64f);
                closeFont = 24;
            }
            else
            {
                windowSize = new Vector2(440f, 680f);
                titlePos = new Vector2(0f, 285f);
                titleSize = new Vector2(380f, 60f);
                titleFont = 30;
                stepSize = new Vector2(400f, 78f);
                stepPositions = new[]
                {
                    new Vector2(0f, 170f),
                    new Vector2(0f, 78f),
                    new Vector2(0f, -14f),
                    new Vector2(0f, -106f)
                };
                stepFont = 18;
                closePos = new Vector2(0f, -290f);
                closeSize = new Vector2(260f, 68f);
                closeFont = 24;
            }

            RectTransform window = UIUtils.CreateCenteredRect("PaperWindow", modal, Vector2.zero, windowSize);
            Image windowBg = window.gameObject.AddComponent<Image>();
            windowBg.color = UIUtils.PanelColor;
            UIUtils.AddPaperShadow(window.gameObject);
            windowBg.raycastTarget = true;

            string titleKey = LocalizationManager.Instance != null
                ? LocalizationManager.Instance.GetText("instructions_title")
                : "Cómo jugar";

            TextMeshProUGUI title = UIUtils.CreateCenteredText("InstructionsTitle", window, titleKey, titleFont, UIUtils.TextColor, titlePos, titleSize);
            title.fontStyle = FontStyles.Bold;

            Vector2 closeXPos = new Vector2(windowSize.x * 0.5f - 32f, windowSize.y * 0.5f - 32f);
            Button closeX = UIUtils.CreateCenteredButton("CloseX", window, "X", IsLandscape ? 26 : 22, closeXPos, new Vector2(48f, 48f), Close);
            Image xImg = closeX.GetComponent<Image>();
            if (xImg != null)
            {
                UIUtils.ApplyPaperButton(xImg);
            }

            for (int i = 0; i < 4; i++)
            {
                string stepKey = "instructions_step" + (i + 1);
                string stepText = LocalizationManager.Instance != null
                    ? LocalizationManager.Instance.GetText(stepKey)
                    : stepKey;

                RectTransform row = UIUtils.CreateCenteredRect("Step" + (i + 1), window, stepPositions[i], stepSize);
                Image rowBg = row.gameObject.AddComponent<Image>();
                rowBg.color = new Color(1f, 1f, 1f, 0.45f);
                rowBg.raycastTarget = false;

                Vector2 iconSize = new Vector2(56f, 56f);
                Vector2 iconPos = new Vector2(-stepSize.x * 0.5f + 36f, 0f);
                Image iconBg = UIUtils.CreateCenteredImage("StepIcon" + (i + 1), row, iconPos, iconSize, UIUtils.PaperBeige);
                UIUtils.ApplyPaperButton(iconBg);

                Sprite stepSprite = null;
                string spritePath = GetStepSpritePath(i);
                if (!string.IsNullOrEmpty(spritePath))
                {
                    stepSprite = Resources.Load<Sprite>(spritePath);
                }
                if (stepSprite != null)
                {
                    iconBg.sprite = stepSprite;
                    iconBg.color = Color.white;
                    iconBg.preserveAspect = true;
                }
                else
                {
                    TextMeshProUGUI iconLabel = UIUtils.CreateCenteredText("IconLabel", row, (i + 1).ToString(), IsLandscape ? 28 : 24, UIUtils.InkCharcoal, iconPos, new Vector2(56f, 56f));
                    iconLabel.alignment = TextAlignmentOptions.Center;
                    iconLabel.raycastTarget = false;
                }

                Vector2 textPos = new Vector2(42f, 0f);
                Vector2 textSize = new Vector2(stepSize.x - 96f, stepSize.y - 10f);
                TextMeshProUGUI stepLabel = UIUtils.CreateCenteredText("StepText" + (i + 1), row, stepText, stepFont, UIUtils.TextColor, textPos, textSize);
                stepLabel.alignment = TextAlignmentOptions.MidlineLeft;
                stepLabel.textWrappingMode = TextWrappingModes.Normal;
                stepLabel.overflowMode = TextOverflowModes.Truncate;
                stepLabel.enableAutoSizing = false;
            }

            string closeLabel = LocalizationManager.Instance != null
                ? LocalizationManager.Instance.GetText("instructions_close")
                : "Entendido";

            Button closeButton = UIUtils.CreateCenteredButton("CloseButton", window, closeLabel, closeFont, closePos, closeSize, Close);

            _modal.SetActive(false);
        }

        private string GetStepSpritePath(int index)
        {
            switch (index)
            {
                case 0: return "Sprites/UI/Instructions/step1";
                case 1: return "Sprites/UI/Instructions/step2";
                case 2: return "Sprites/UI/Instructions/step3";
                case 3: return "Sprites/UI/Instructions/step4";
                default: return null;
            }
        }

        public void Open()
        {
            if (_modal != null)
            {
                _modal.SetActive(true);
            }
        }

        public void Close()
        {
            if (_modal != null)
            {
                _modal.SetActive(false);
            }
        }

        public bool IsOpen => _modal != null && _modal.activeSelf;
    }
}
