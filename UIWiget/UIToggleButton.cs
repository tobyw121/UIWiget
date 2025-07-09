// UIToggleButton.cs
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace YourGame.UI.Widgets
{
    // Ein Button, der einen "Checked"-Zustand haben kann
    public class UIToggleButton : UIButton
    {
        [Header("Toggle Settings")]
        [SerializeField] private bool _isChecked;
        [SerializeField] private GameObject _checkmark;

        public UnityEvent<bool> OnCheckChanged;

        public bool IsChecked
        {
            get => _isChecked;
            set {
                if (_isChecked == value) return;
                _isChecked = value;
                UpdateCheckmark();
                OnCheckChanged?.Invoke(_isChecked);
            }
        }

        protected override void Awake()
        {
            base.Awake();
            UpdateCheckmark();
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            if (CurrentState == UIState.Interactive)
            {
                IsChecked = !IsChecked;
                base.OnPointerClick(eventData);
            }
        }
        
        private void UpdateCheckmark()
        {
            if (_checkmark != null)
            {
                _checkmark.SetActive(_isChecked);
            }
        }
    }
}