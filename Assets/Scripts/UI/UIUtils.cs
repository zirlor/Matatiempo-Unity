using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using Matatiempo.Core;

namespace Matatiempo.UI
{
    public static class UIUtils
    {
        public static readonly Color PanelColor = new Color(0.976f, 0.949f, 0.878f, 0.97f);
        public static readonly Color PrimaryColor = new Color(0.949f, 0.639f, 0.318f, 1f);
        public static readonly Color TextColor = new Color(0.290f, 0.231f, 0.165f, 1f);
        public static readonly Color FigureColor = new Color(0.44f, 0.33f, 0.22f, 1f);

        public static readonly Color PaperBeige = new Color(0.965f, 0.918f, 0.780f, 1f);
        public static readonly Color PaperAccent = new Color(0.949f, 0.639f, 0.318f, 1f);
        public static readonly Color InkCharcoal = new Color(0.290f, 0.231f, 0.165f, 1f);

        public const string PaperButtonRound = "Sprites/UI/PaperStyle/button_round_depth_flat";
        public const string PaperButtonRect = "Sprites/UI/PaperStyle/button_rectangle_depth_flat";
        public const string PaperKey = "Sprites/UI/PaperStyle/button_rectangle_depth_flat";

        private static readonly Vector2 PaperShadowOffset = new Vector2(2f, -2f);
        private static readonly Color PaperShadowColor = new Color(0.24f, 0.17f, 0.08f, 0.30f);

        // --- Cache de sprites para evitar Resources.Load repetidos ---
        private static readonly Dictionary<string, Sprite> _spriteCache =
            new Dictionary<string, Sprite>();

        public static RectTransform CreateRect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            return rect;
        }

        public static RectTransform CreateCenteredRect(string name, Transform parent, Vector2 position, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.offsetMin = position - size * 0.5f;
            rect.offsetMax = position + size * 0.5f;
            return rect;
        }

        public static Image CreateImage(RectTransform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Color color)
        {
            RectTransform rect = CreateRect(name, parent, anchorMin, anchorMax, offsetMin, offsetMax);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            return image;
        }

        public static Image CreateCenteredImage(string name, Transform parent, Vector2 position, Vector2 size, Color color)
        {
            RectTransform rect = CreateCenteredRect(name, parent, position, size);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            return image;
        }

        public static TextMeshProUGUI CreateText(string name, Transform parent, string content, float fontSize, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            RectTransform rect = CreateRect(name, parent, anchorMin, anchorMax, offsetMin, offsetMax);
            var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            text.text = content;
            text.fontSize = fontSize;
            text.color = color;
            text.enableWordWrapping = false;
            return text;
        }

        public static TextMeshProUGUI CreateCenteredText(string name, Transform parent, string content, float fontSize, Color color, Vector2 position, Vector2 size)
        {
            RectTransform rect = CreateCenteredRect(name, parent, position, size);
            var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            text.text = content;
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = TextAlignmentOptions.Center;
            text.enableWordWrapping = false;
            return text;
        }

        public static Button CreateButton(string name, Transform parent, string label, int fontSize, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, UnityAction onClick = null)
        {
            RectTransform rect = CreateRect(name, parent, anchorMin, anchorMax, offsetMin, offsetMax);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            var button = rect.gameObject.AddComponent<Button>();
            if (onClick != null)
            {
                button.onClick.AddListener(onClick);
            }
            AddClickSound(button);
            ApplyPaperButton(image);
            AddPaperShadow(rect.gameObject);
            TextMeshProUGUI text = CreateText("Label", rect, label, fontSize, InkCharcoal, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            text.alignment = TextAlignmentOptions.Center;
            return button;
        }

        public static Button CreateCenteredButton(string name, Transform parent, string label, int fontSize, Vector2 position, Vector2 size, UnityAction onClick = null)
        {
            RectTransform rect = CreateCenteredRect(name, parent, position, size);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = PrimaryColor;
            var button = rect.gameObject.AddComponent<Button>();
            if (onClick != null)
            {
                button.onClick.AddListener(onClick);
            }
            AddClickSound(button);
            ApplyPaperButton(image);
            AddPaperShadow(rect.gameObject);
            TextMeshProUGUI text = CreateText("Label", rect, label, fontSize, InkCharcoal, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            text.alignment = TextAlignmentOptions.Center;
            return button;
        }

        public static void ApplySprite(Image image, string resourcePath)
        {
            if (image == null)
            {
                return;
            }
            Sprite sprite = LoadSprite(resourcePath);
            if (sprite == null)
            {
                return;
            }
            image.sprite = sprite;
            image.color = Color.white;
        }

        public static void ApplyPaperButton(Image image)
        {
            if (image == null)
            {
                return;
            }
            Sprite sprite = LoadSprite(PaperButtonRound);
            if (sprite != null)
            {
                image.sprite = sprite;
                image.color = PaperBeige;
            }
        }

        public static void ApplyPaperKey(Image image)
        {
            if (image == null)
            {
                return;
            }
            Sprite sprite = LoadSprite(PaperKey);
            if (sprite != null)
            {
                image.sprite = sprite;
                image.color = PaperAccent;
            }
        }

        public static Shadow AddPaperShadow(GameObject target)
        {
            if (target == null)
            {
                return null;
            }
            Shadow existing = target.GetComponent<Shadow>();
            if (existing != null)
            {
                return existing;
            }
            Shadow shadow = target.AddComponent<Shadow>();
            shadow.effectColor = PaperShadowColor;
            shadow.effectDistance = PaperShadowOffset;
            shadow.useGraphicAlpha = true;
            return shadow;
        }

        // Delegate único reutilizado por todos los botones → cero allocations.
        // Antes se usaba una lambda anónima por botón, lo que creaba una clase
        // closure + delegate instance en cada CreateButton/CreateCenteredButton.
        private static readonly UnityAction _playClickAction = PlayClick;

        private static void PlayClick()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayClick();
            }
        }

        private static void AddClickSound(Button button)
        {
            button.onClick.AddListener(_playClickAction);
        }

        // --- Cache de sprites ---
        public static Sprite LoadSprite(string resourcePath)
        {
            if (string.IsNullOrEmpty(resourcePath))
            {
                return null;
            }
            if (_spriteCache.TryGetValue(resourcePath, out Sprite cached))
            {
                return cached;
            }
            Sprite sprite = Resources.Load<Sprite>(resourcePath);
            if (sprite != null)
            {
                _spriteCache.Add(resourcePath, sprite);
            }
            return sprite;
        }

        /// <summary>
        /// Lava el cache de sprites. Útil al recargar assets en modo editor.
        /// </summary>
        public static void ClearSpriteCache()
        {
            _spriteCache.Clear();
        }
    }
}
