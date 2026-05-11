using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class OverlayEffects : MonoBehaviour
    {
        private GameObject _overlayCanvas;
        private Image _overlayImage;

        /// <summary>
        /// Displays a texture as a screen overlay.
        /// </summary>
        /// <param name="texture">The Texture2D to display.</param>
        /// <param name="alpha">Transparency level (0 to 1).</param>
        public void ShowOverlay(Texture2D texture, float alpha = 0.5f)
        {
            // Initialize Canvas and Image if they don't exist
            if (_overlayCanvas == null)
            {
                _overlayCanvas = new GameObject("OverlayCanvas");
                Canvas canvas = _overlayCanvas.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 100; // Ensures it draws on top
                _overlayCanvas.AddComponent<CanvasScaler>();
                _overlayCanvas.AddComponent<GraphicRaycaster>();

                GameObject imageObj = new GameObject("OverlayImage");
                imageObj.transform.SetParent(_overlayCanvas.transform);
                _overlayImage = imageObj.AddComponent<Image>();

                // Make the image stretch to fill the screen
                RectTransform rect = imageObj.GetComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }

            // Convert Texture2D to Sprite
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

            _overlayImage.sprite = sprite;
            _overlayImage.color = new Color(1, 1, 1, alpha);
            _overlayCanvas.SetActive(true);
        }

        public void HideOverlay()
        {
            if (_overlayCanvas != null)
            {
                _overlayCanvas.SetActive(false);
            }
        }
    }
}