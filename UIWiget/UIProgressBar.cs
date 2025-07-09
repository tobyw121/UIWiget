using UnityEngine;
using UnityEngine.UI;

namespace YourGame.UI.Widgets
{
    public class UIProgressBar : UIWidget
    {
        [Header("Progress Bar Settings")]
        [SerializeField] private Image _fillImage;
        [SerializeField, Range(0f, 1f)] private float _progress;

        public float Progress
        {
            get => _progress;
            set
            {
                _progress = Mathf.Clamp01(value);
                UpdateFill();
            }
        }

        public Image FillImage
        {
            get => _fillImage;
            set => _fillImage = value;
        }

        protected override void Awake()
        {
            base.Awake();
            if (_fillImage == null) Debug.LogError("Fill Image nicht zugewiesen in UIProgressBar: " + name);
            UpdateFill();
        }

        private void OnValidate()
        {
            UpdateFill();
        }

        private void UpdateFill()
        {
            if (_fillImage != null)
            {
                _fillImage.fillAmount = _progress;
            }
        }
    }
}