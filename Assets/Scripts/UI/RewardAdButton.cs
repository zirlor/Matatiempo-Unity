using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Matatiempo.Core;
using Matatiempo.Monetization;

namespace Matatiempo.UI
{
    public static class RewardAdButton
    {
        private static readonly Color DisabledLabelColor = new Color(0.40f, 0.38f, 0.34f, 0.60f);

        public static Button Create(RectTransform parent, string name, string label, int fontSize, Vector2 size)
        {
            Button button = UIUtils.CreateButton(name, parent, label, fontSize, UIUtils.TextColor, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-size.x * 0.5f, -size.y * 0.5f), new Vector2(size.x * 0.5f, size.y * 0.5f), OnClick);
            UIUtils.ApplyPaperButton(button.image);
            UIUtils.AddPaperShadow(button.gameObject);
            LayoutElement element = button.gameObject.AddComponent<LayoutElement>();
            element.preferredWidth = size.x;
            element.preferredHeight = size.y;
            ReadinessGate.Attach(button);
            return button;
        }

        private static void OnClick()
        {
            if (MonetizationManager.Instance == null)
            {
                return;
            }
            MonetizationManager.Instance.ShowRewardedAd(HandleReward);
        }

        private static void HandleReward(bool rewarded)
        {
            if (rewarded)
            {
                int lives = MainMenuUIManager.AddLives(1);
                GameEvents.NotifyLivesChanged(lives);
            }
        }

        private sealed class ReadinessGate : MonoBehaviour
        {
            private Button _button;
            private TextMeshProUGUI _label;
            private Color _enabledColor;
            private float _nextPoll;

            public static void Attach(Button button)
            {
                ReadinessGate gate = button.gameObject.AddComponent<ReadinessGate>();
                gate._button = button;
                gate._label = button.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
                if (gate._label != null)
                {
                    gate._enabledColor = gate._label.color;
                }
                gate.Refresh();
            }

            private void Update()
            {
                if (Time.unscaledTime < _nextPoll)
                {
                    return;
                }
                _nextPoll = Time.unscaledTime + 0.5f;
                Refresh();
            }

            private void Refresh()
            {
                if (_button == null)
                {
                    return;
                }
                MonetizationManager manager = MonetizationManager.Instance;
                bool available = manager != null && manager.IsRewardedAvailable;
                _button.interactable = available;
                if (_label != null)
                {
                    _label.color = available ? _enabledColor : DisabledLabelColor;
                }
            }
        }
    }
}