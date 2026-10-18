using UnityEngine;
using UnityEngine.UI;

namespace Matatiempo.UI
{
    public abstract class View : MonoBehaviour
    {
        public static bool IsLandscape => Screen.width >= Screen.height;

        public virtual void Rebuild()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
            Build();
        }

        protected abstract void Build();

        protected void BuildBackground(RectTransform root)
        {
            Image background = UIUtils.CreateImage(root, "Background", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, UIUtils.PanelColor);
            UIUtils.ApplySprite(background, "Sprites/UI/background");
        }
    }
}