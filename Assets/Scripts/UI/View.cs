using UnityEngine;
using UnityEngine.UI;
using Matatiempo.Core;

namespace Matatiempo.UI
{
    public abstract class View : MonoBehaviour
    {
        public static bool IsLandscape => Screen.width >= Screen.height;

        // Estado cacheado para dirty-check en Rebuild().
        // _cachedLanguage null = "nunca cacheado" → fuerza rebuild la primera vez (no depende de Awake).
        private bool _cachedLandscape;
        private string _cachedLanguage;

        public virtual void Rebuild()
        {
            // Dirty check: solo destruir + reconstruir si cambió la orientación
            // o el idioma activo. Evita GC y pérdida de estado por rebuilds espúriosm.
            bool landscape = IsLandscape;
            string language = LocalizationManager.Instance?.ActiveLanguage ?? string.Empty;
            if (_cachedLanguage != null && landscape == _cachedLandscape && language == _cachedLanguage)
            {
                return;
            }
            _cachedLandscape = landscape;
            _cachedLanguage = language;

            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
            Build();
        }

        /// <summary>
        /// Actualiza textos localizados SIN destruir el UI (llamado tras cambio
        /// de idioma vía LocalizationManager.LanguageChanged). Las Views que
        /// poseen textos estáticos deben sobreescribir este método.
        /// </summary>
        public virtual void RefreshLocalization() { }

        protected abstract void Build();

        protected void BuildBackground(RectTransform root)
        {
            Image background = UIUtils.CreateImage(root, "Background", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, UIUtils.PanelColor);
            UIUtils.ApplySprite(background, "Sprites/UI/background");
        }
    }
}
