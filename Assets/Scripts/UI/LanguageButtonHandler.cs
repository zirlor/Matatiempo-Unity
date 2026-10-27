using UnityEngine;
using Matatiempo.Core;

namespace Matatiempo.UI
{
    /// <summary>
    /// Handler sin allocations para botones de selección de idioma.
    /// Reemplaza la closure <c>() => SelectLanguage(languages[index])</c>
    /// que capturaba la variable del loop (4 allocations en BuildLanguageButtons).
    /// </summary>
    [DisallowMultipleComponent]
    internal sealed class LanguageButtonHandler : MonoBehaviour
    {
        private string _languageCode;

        public void SetLanguage(string languageCode)
        {
            _languageCode = languageCode;
        }

        public void OnClick()
        {
            // El refresh de textos se dispara vía LocalizationManager.LanguageChanged
            // (suscripción de UIManager), así que no se necesita RebuildAllViews aquí.
            LocalizationManager.Instance.SetLanguage(_languageCode);
        }
    }
}
