using UnityEngine;
using UnityEngine.UI;
using Matatiempo.Gameplay;

namespace Matatiempo.UI
{
    /// <summary>
    /// Handler sin allocations para botones de teclado dinámico.
    /// Reemplaza la closure <c>() => PressKey(letter)</c> que capturaba la
    /// variable del loop y creaba una clase anónima por tecla (28 allocations en BuildKeyboard).
    /// </summary>
    [DisallowMultipleComponent]
    internal sealed class KeyButtonHandler : MonoBehaviour
    {
        private char _key;

        public void SetKey(char key)
        {
            _key = char.ToUpperInvariant(key);
        }

        public void OnClick()
        {
            HangmanGameManager.Instance?.Guess(_key);
        }
    }
}
