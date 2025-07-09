// Dateiname: UISlider.cs
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace YourGame.UI.Widgets
{
    public class UISlider : UIWidget
    {
        [Header("Slider Components")]
        [SerializeField] private RectTransform _fillRect;
        [SerializeField] private RectTransform _handleRect;

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
            _backgroundRect = GetComponent<RectTransform>();
            UpdateVisuals();
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
            Vector2 localPoint;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_backgroundRect, eventData.position, eventData.pressEventCamera, out localPoint))
            {
                float sliderWidth = _backgroundRect.rect.width;
                float newValue = (localPoint.x - _backgroundRect.rect.xMin) / sliderWidth;
                Value = Mathf.Clamp01(newValue);
            }
        }

        private void UpdateVisuals()
        {
            if (_fillRect)
            {
                _fillRect.anchorMax = new Vector2(_value, _fillRect.anchorMax.y);
            }
            if (_handleRect)
            {
                _handleRect.anchorMin = new Vector2(_value, _handleRect.anchorMin.y);
                _handleRect.anchorMax = new Vector2(_value, _handleRect.anchorMax.y);
            }
        }
    }
}