using UnityEngine;
using UnityEngine.UI;

namespace YourGame.UI
{
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(GraphicRaycaster))]
    public class UIOverlayManager : MonoBehaviour
    {
        private static UIOverlayManager _instance;
        private static readonly object _lock = new object();

        public static UIOverlayManager Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = FindObjectOfType<UIOverlayManager>() ??
                                    new GameObject("UIOverlayManager_AutoCreated").AddComponent<UIOverlayManager>();
                    }
                    return _instance;
                }
            }
        }

        private Canvas _canvas;
        private CanvasGroup _overlayGroup;
        private Image _backgroundImage;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            // Setup Canvas
            _canvas = GetComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 999; // Hoher Sorting Order, um über allem zu liegen

            // Setup Overlay GameObject
            GameObject overlayGO = new GameObject("OverlayGroup");
            overlayGO.transform.SetParent(transform, false);
            _backgroundImage = overlayGO.AddComponent<Image>();
            _backgroundImage.rectTransform.anchorMin = Vector2.zero;
            _backgroundImage.rectTransform.anchorMax = Vector2.one;
            _backgroundImage.rectTransform.sizeDelta = Vector2.zero;
            _overlayGroup = overlayGO.AddComponent<CanvasGroup>();
            _overlayGroup.alpha = 0;
            _overlayGroup.blocksRaycasts = false;
        }

        public void ActivateExclusiveOverlay(Color maskColor)
        {
            _backgroundImage.color = maskColor;
            _overlayGroup.alpha = 1;
            _overlayGroup.blocksRaycasts = true;
        }

        public void DeactivateExclusiveOverlay()
        {
            _overlayGroup.alpha = 0;
            _overlayGroup.blocksRaycasts = false;
        }
    }
}