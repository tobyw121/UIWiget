// Dateiname: UISlider.cs (Korrigiert mit Theming für Style States)
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace YourGame.UI.Widgets
{
    public class UISlider : UIWidget
    {
        [Header("Slider Components")]
        [SerializeField] private Image _fillRect;
        [SerializeField] private Image _handleRect;

        [Header("Slider Settings")]
        [SerializeField, Range(0f, 1f)] private float _value = 1f;
        public UnityEvent<float> OnValueChanged;

        private RectTransform _backgroundRect;

        public float Value
        {
            get => _value;
            set
            {
                float clampedValue = Mathf.Clamp01(value);
                if (_value == clampedValue) return;
                _value = clampedValue;
                UpdateVisuals();
                OnValueChanged?.Invoke(_value);
            }
        }

        protected override void Awake()
        {
            base.Awake();
            if (OnValueChanged == null) OnValueChanged = new UnityEvent<float>();
            _backgroundRect = GetComponent<RectTransform>();
            if(targetGraphic == null) targetGraphic = GetComponent<Image>();
            UpdateVisuals();
        }

        // KORRIGIERTE METHODE: Wendet den Stil auf alle Teile des Sliders an
        protected override void ApplyThemeStyle()
        {
            // Ruft die Basis-Implementierung auf. Diese wendet den Stil
            // auf die 'targetGraphic' an (den Slider-Hintergrund).
            base.ApplyThemeStyle();

            if (UIThemeManager.Instance == null || string.IsNullOrEmpty(styleKey)) return;
            var style = UIThemeManager.Instance.GetStyle(styleKey);
            if (style == null) return;

            // Wende den Stil des "Normal"-Zustands auf die Füllung an.
            if (_fillRect != null)
            {
                if (style.Normal.sprite != null)
                {
                    _fillRect.sprite = style.Normal.sprite;
                }
                _fillRect.color = style.Normal.color;
            }

            // Wende den Stil des "Hover"-Zustands auf den Griff an, um ihn visuell abzuheben.
            if (_handleRect != null)
            {
                if (style.Hover.sprite != null)
                {
                    _handleRect.sprite = style.Hover.sprite;
                }
                _handleRect.color = style.Hover.color;
            }
        }

        public override void OnDrag(PointerEventData eventData)
        {
            base.OnDrag(eventData);
            if (CurrentState == UIState.Interactive)
            {
                UpdateValueFromInput(eventData);
            }
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            if (CurrentState == UIState.Interactive)
            {
                UpdateValueFromInput(eventData);
            }
        }

        private void UpdateValueFromInput(PointerEventData eventData)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_backgroundRect, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
            {
                float sliderWidth = _backgroundRect.rect.width;
                float localX = Mathf.Clamp(localPoint.x - _backgroundRect.rect.xMin, 0, sliderWidth);
                Value = localX / sliderWidth;
            }
        }

        private void UpdateVisuals()
        {
            if (_fillRect)
            {
                _fillRect.rectTransform.anchorMax = new Vector2(_value, 1);
            }
            if (_handleRect)
            {
                _handleRect.rectTransform.anchorMin = new Vector2(_value, 0);
                _handleRect.rectTransform.anchorMax = new Vector2(_value, 1);
            }
        }
    }
}