using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Matatiempo.Core;

namespace Matatiempo.UI
{
    public class HintModal : MonoBehaviour
    {
        private GameObject _modal;
        private TextMeshProUGUI _clueText;
        private bool IsLandscape => Screen.width > Screen.height;

        public void Build(RectTransform parent)
        {
            if (_modal != null)
            {
                Destroy(_modal);
            }

            RectTransform modal = UIUtils.CreateRect("HintModal", parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            UIUtils.CreateImage(modal, "Dim", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0.6f));
            Button dimButton = modal.gameObject.AddComponent<Button>();
            dimButton.transition = Selectable.Transition.None;
            dimButton.onClick.AddListener(Close);

            _modal = modal.gameObject;

            Vector2 windowSize;
            Vector2 titlePos;
            Vector2 titleSize;
            int titleFont;
            Vector2 cluePos;
            Vector2 clueSize;
            int clueFont;
            Vector2 closePos;
            Vector2 closeSize;
            int closeFont;

            if (IsLandscape)
            {
                windowSize = new Vector2(760f, 360f);
                titlePos = new Vector2(0f, 120f);
                titleSize = new Vector2(600f, 60f);
                titleFont = 34;
                cluePos = new Vector2(0f, -10f);
                clueSize = new Vector2(680f, 160f);
                clueFont = 28;
                closePos = new Vector2(0f, -130f);
                closeSize = new Vector2(240f, 64f);
                closeFont = 24;
            }
            else
            {
                windowSize = new Vector2(420f, 400f);
                titlePos = new Vector2(0f, 150f);
                titleSize = new Vector2(360f, 50f);
                titleFont = 30;
                cluePos = new Vector2(0f, 10f);
                clueSize = new Vector2(380f, 170f);
                clueFont = 24;
                closePos = new Vector2(0f, -150f);
                closeSize = new Vector2(260f, 64f);
                closeFont = 24;
            }

            RectTransform window = UIUtils.CreateCenteredRect("PaperWindow", modal, Vector2.zero, windowSize);
            Image windowBg = window.gameObject.AddComponent<Image>();
            windowBg.color = UIUtils.PanelColor;
            UIUtils.AddPaperShadow(window.gameObject);
            windowBg.raycastTarget = true;

            string titleKey = LocalizationManager.Instance != null
                ? LocalizationManager.Instance.GetText("clue_title")
                : "Pista";

            TextMeshProUGUI title = UIUtils.CreateCenteredText("HintTitle", window, titleKey, titleFont, UIUtils.TextColor, titlePos, titleSize);
            title.fontStyle = FontStyles.Bold;

            Vector2 closeXPos = new Vector2(windowSize.x * 0.5f - 32f, windowSize.y * 0.5f - 32f);
            Button closeX = UIUtils.CreateCenteredButton("CloseX", window, "X", IsLandscape ? 26 : 22, closeXPos, new Vector2(48f, 48f), Close);
            Image xImg = closeX.GetComponent<Image>();
            if (xImg != null)
            {
                UIUtils.ApplyPaperButton(xImg);
            }

            _clueText = UIUtils.CreateCenteredText("HintClue", window, string.Empty, clueFont, UIUtils.TextColor, cluePos, clueSize);
            _clueText.alignment = TextAlignmentOptions.Center;
            _clueText.textWrappingMode = TextWrappingModes.Normal;
            _clueText.overflowMode = TextOverflowModes.Truncate;
            _clueText.enableAutoSizing = false;

            string closeLabel = LocalizationManager.Instance != null
                ? LocalizationManager.Instance.GetText("clue_close")
                : "Cerrar";

            UIUtils.CreateCenteredButton("CloseButton", window, closeLabel, closeFont, closePos, closeSize, Close);

            _modal.SetActive(false);
        }

        public void Open(string clue)
        {
            if (_modal != null)
            {
                if (_clueText != null)
                {
                    _clueText.text = string.IsNullOrEmpty(clue)
                        ? GetFallbackText()
                        : clue;
                }
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

        private string GetFallbackText()
        {
            return LocalizationManager.Instance != null
                ? LocalizationManager.Instance.GetText("clue_fallback")
                : "No hay pista para esta palabra...";
        }
    }
}