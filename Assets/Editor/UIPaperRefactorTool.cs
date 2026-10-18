using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

namespace Matatiempo.EditorTools
{
    public static class UIPaperRefactorTool
    {
        private const string FontTtfPath = "Assets/Fonts/Fredoka.ttf";
        private const string FontAssetPath = "Assets/Fonts/Fredoka-SDF.asset";

        public static readonly string[] ButtonSprites =
        {
            "Sprites/UI/PaperStyle/button_round_depth_flat",
            "Sprites/UI/PaperStyle/button_rectangle_depth_flat",
            "Sprites/UI/PaperStyle/button_round_depth_gradient",
            "Sprites/UI/PaperStyle/button_rectangle_depth_gradient"
        };

        public static readonly Color PaperBeige = new Color(0.965f, 0.918f, 0.780f, 1f);
        public static readonly Color PaperAccent = new Color(0.949f, 0.639f, 0.318f, 1f);
        public static readonly Color InkCharcoal = new Color(0.290f, 0.231f, 0.165f, 1f);
        public static readonly Color ShadowColor = new Color(0.24f, 0.17f, 0.08f, 0.30f);

        [MenuItem("Matatiempo/Aplicar Estilo Paper UI")]
        public static void ApplyPaperUi()
        {
            TMP_FontAsset font = EnsureFontAsset();
            int targetCount = 0;
            int shadowCount = 0;

            Image[] images = Object.FindObjectsOfType<Image>(true);
            foreach (Image image in images)
            {
                bool styled = TryStyleImage(image);
                if (styled)
                {
                    targetCount++;
                    if (image.GetComponent<Shadow>() == null)
                    {
                        Shadow s = image.gameObject.AddComponent<Shadow>();
                        s.effectColor = ShadowColor;
                        s.effectDistance = new Vector2(2f, -2f);
                        s.useGraphicAlpha = true;
                        shadowCount++;
                    }
                }
            }

            if (font != null)
            {
                TextMeshProUGUI[] texts = Object.FindObjectsOfType<TextMeshProUGUI>(true);
                foreach (TextMeshProUGUI text in texts)
                {
                    if (text.transform.parent != null && text.transform.parent.GetComponent<Button>() != null)
                    {
                        text.font = font;
                        text.color = InkCharcoal;
                    }
                }
            }

            Canvas canvas = Object.FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                EditorUtility.SetDirty(canvas);
            }

            Debug.Log($"[PaperUI] Aplicado a {targetCount} imágenes, {shadowCount} sombras añadidas. Fuente: {(font != null ? "OK" : "NO CREADA")}");
        }

        private static TMP_FontAsset EnsureFontAsset()
        {
            TMP_FontAsset existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
            if (existing != null)
            {
                return existing;
            }

            Font ttf = AssetDatabase.LoadAssetAtPath<Font>(FontTtfPath);
            if (ttf == null)
            {
                Debug.LogWarning("[PaperUI] No se encontró la fuente en " + FontTtfPath + ". Descarga Fredoka.ttf y colócala en Assets/Fonts.");
                return null;
            }

            TMP_FontAsset asset = TMP_FontAsset.CreateFontAsset(ttf);
            if (asset == null)
            {
                Debug.LogWarning("[PaperUI] No se pudo crear el TMP_FontAsset.");
                return null;
            }

            AssetDatabase.CreateAsset(asset, FontAssetPath);
            AssetDatabase.SaveAssets();
            return asset;
        }

        private static bool TryStyleImage(Image image)
        {
            if (image == null || !image.gameObject.activeInHierarchy)
            {
                return false;
            }

            string path = AssetDatabase.GetAssetPath(image.sprite);
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            string name = image.sprite.name.ToLowerInvariant();

            if (image.transform.parent != null && image.transform.parent.GetComponent<Button>() != null)
            {
                image.sprite = LoadSprite(GetButtonSpriteFor(name));
                image.color = PaperBeige;
                return true;
            }

            if (name.Contains("key"))
            {
                image.sprite = LoadSprite(GetButtonSpriteFor(name));
                image.color = PaperAccent;
                return true;
            }

            return false;
        }

        private static Sprite LoadSprite(string path)
        {
            return Resources.Load<Sprite>(path);
        }

        private static string GetButtonSpriteFor(string currentName)
        {
            if (currentName.Contains("round"))
            {
                return ButtonSprites[2];
            }
            if (currentName.Contains("key"))
            {
                return ButtonSprites[1];
            }
            return ButtonSprites[0];
        }
    }
}
