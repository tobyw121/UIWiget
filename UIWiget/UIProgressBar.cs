// Dateiname: UIProgressBar.cs (Final Korrigiert)
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
            // Die Prüfung in Awake wurde entfernt, um Fehler bei der dynamischen Erstellung zu vermeiden.
            // Der Null-Check in UpdateFill() ist ausreichend.
            UpdateFill();
        }

        private void OnValidate()
        {
            UpdateFill();
        }

        public void UpdateFill()
        {
            // Dieser Null-Check verhindert Laufzeitfehler.
            if (_fillImage != null)
            {
                _fillImage.fillAmount = _progress;
            }
        }
    }
}