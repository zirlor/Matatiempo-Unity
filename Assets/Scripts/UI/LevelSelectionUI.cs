using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Matatiempo.Core;

namespace Matatiempo.UI
{
    public class LevelSelectionUI : MonoBehaviour
    {
        [SerializeField] private Button[] levelButtons = new Button[0];

        private readonly Color _lockedBackground = new Color(0.72f, 0.70f, 0.66f, 0.55f);
        private readonly Color _lockedLabel = new Color(0.40f, 0.38f, 0.34f, 0.60f);
        private Button[] _buttons;

        private void Start()
        {
            if (levelButtons != null && levelButtons.Length > 0)
            {
                ApplyLevels(levelButtons);
            }
        }

        private void OnEnable()
        {
            GameEvents.LivesChanged += HandleLivesChanged;
        }

        private void OnDisable()
        {
            GameEvents.LivesChanged -= HandleLivesChanged;
        }

        private void HandleLivesChanged(int lives)
        {
            Refresh();
        }

        public void ApplyLevels(Button[] buttons)
        {
            _buttons = buttons != null ? buttons : new Button[0];
            Refresh();
        }

        public void Refresh()
        {
            int unlocked = LevelManager.Instance.GetMaxLevelUnlocked();
            bool canPlay = LivesManager.HasLives();
            for (int i = 0; i < _buttons.Length; i++)
            {
                int level = i + 1;
                Button button = _buttons[i];
                if (button == null)
                {
                    continue;
                }
                bool enabled = level <= unlocked && canPlay;
                button.interactable = enabled;
                if (button.image != null)
                {
                    button.image.color = enabled ? UIUtils.PanelColor : _lockedBackground;
                }
                TextMeshProUGUI label = button.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
                if (label != null)
                {
                    label.color = enabled ? UIUtils.TextColor : _lockedLabel;
                }
            }
        }
    }
}