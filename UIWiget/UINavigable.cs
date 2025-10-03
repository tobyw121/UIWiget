using UnityEngine;

namespace YourGame.UI.Widgets
{
    public class UINavigable : MonoBehaviour
    {
        private UIWidget _widgetController;

        [Header("Navigation")]
        public UIWidget selectOnUp;
        public UIWidget selectOnDown;
        public UIWidget selectOnLeft;
        public UIWidget selectOnRight;

        void Awake()
        {
            _widgetController = GetComponent<UIWidget>();
        }

        public void OnFocusGained()
        {
            _widgetController.OnFocusGainedEvent?.Invoke();
            transform.localScale *= 1.05f;
        }

        public void OnFocusLost()
        {
            _widgetController.OnFocusLostEvent?.Invoke();
            transform.localScale /= 1.05f;
        }
    }
}