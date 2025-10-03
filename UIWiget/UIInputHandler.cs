// Dateiname: UIInputHandler.cs
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.EventSystems;

namespace YourGame.UI.Widgets
{
    public class UIInputHandler : MonoBehaviour
    {
        public static UIInputHandler Instance { get; private set; }
        private Dictionary<KeyCode, List<UIWidget>> _keyBindings = new Dictionary<KeyCode, List<UIWidget>>();
        private UIWidget _currentlyFocusedWidget;
        public UIWidget FocusedWidget => _currentlyFocusedWidget;
        private bool _isInputFieldFocused = false;

        // Für Gamepad-Navigation
        private Vector2 _navInput;
        private float _navTimer;
        private const float NAV_THRESHOLD = 0.5f;
        private const float NAV_REPEAT_DELAY = 0.2f;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void Update()
        {
            HandleKeyToggles();
            HandleNavigation();
            HandleSubmit();
        }
        
        private void HandleKeyToggles()
        {
             if (_keyBindings.Count > 0)
            {
                var keys = _keyBindings.Keys.ToList();
                foreach (var key in keys)
                {
                    if (Input.GetKeyDown(key))
                    {
                        var widgetsToToggle = _keyBindings[key].ToList();
                        foreach (var widget in widgetsToToggle)
                        {
                            if (widget != null) widget.Toggle();
                        }
                    }
                }
            }
        }
        
        private void HandleSubmit()
        {
            if (_currentlyFocusedWidget != null && !_isInputFieldFocused)
            {
                if (Input.GetButtonDown("Submit")) // Unitys Standard-Submit (Enter, Gamepad A/X)
                {
                    var pointerData = new PointerEventData(EventSystem.current);
                    _currentlyFocusedWidget.OnPointerClick(pointerData);
                }
            }
        }

        private void HandleNavigation()
        {
            if (_currentlyFocusedWidget == null || _isInputFieldFocused) return;

            // Arrow keys als Fallback für Gamepad-Achsen
            float horizontal = Input.GetAxis("Horizontal") + (Input.GetKey(KeyCode.RightArrow) ? 1 : 0) - (Input.GetKey(KeyCode.LeftArrow) ? 1 : 0);
            float vertical = Input.GetAxis("Vertical") + (Input.GetKey(KeyCode.UpArrow) ? 1 : 0) - (Input.GetKey(KeyCode.DownArrow) ? 1 : 0);
            
            _navInput = new Vector2(horizontal, vertical);

            if (_navTimer > 0)
            {
                _navTimer -= Time.unscaledDeltaTime;
                return;
            }
            
            if (Mathf.Abs(_navInput.x) > NAV_THRESHOLD)
            {
                UIWidget nextFocus = (_navInput.x > 0) ? _currentlyFocusedWidget.selectOnRight : _currentlyFocusedWidget.selectOnLeft;
                if (nextFocus != null)
                {
                    SetFocus(nextFocus);
                    _navTimer = NAV_REPEAT_DELAY;
                }
            }
            else if (Mathf.Abs(_navInput.y) > NAV_THRESHOLD)
            {
                UIWidget nextFocus = (_navInput.y > 0) ? _currentlyFocusedWidget.selectOnUp : _currentlyFocusedWidget.selectOnDown;
                if (nextFocus != null)
                {
                    SetFocus(nextFocus);
                    _navTimer = NAV_REPEAT_DELAY;
                }
            }
        }

        public void SetFocus(UIWidget widget)
        {
            if (_currentlyFocusedWidget == widget) return;
            
            if (_currentlyFocusedWidget != null) _currentlyFocusedWidget.OnFocusLost();
            _currentlyFocusedWidget = widget;
            if (_currentlyFocusedWidget != null)
            {
                _currentlyFocusedWidget.OnFocusGained();
                _isInputFieldFocused = (_currentlyFocusedWidget is UIInputField);
            }
            else
            {
                _isInputFieldFocused = false;
            }
        }
        
        public void RegisterToggleKey(KeyCode key, UIWidget widget) { if (key == KeyCode.None || widget == null) return; if (!_keyBindings.ContainsKey(key)) { _keyBindings[key] = new List<UIWidget>(); } if (!_keyBindings[key].Contains(widget)) { _keyBindings[key].Add(widget); } }
        public void UnregisterToggleKey(KeyCode key, UIWidget widget) { if (key == KeyCode.None || widget == null) return; if (_keyBindings.ContainsKey(key)) { _keyBindings[key].Remove(widget); if (_keyBindings[key].Count == 0) { _keyBindings.Remove(key); } } }
    }
}