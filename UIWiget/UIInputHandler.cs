// Dateiname: UIInputHandler.cs (Korrigiert)
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.EventSystems;

namespace YourGame.UI.Widgets
{
    public class UIInputHandler : MonoBehaviour
    {
        private static UIInputHandler _instance;
        public static UIInputHandler Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<UIInputHandler>();
                    if (_instance == null)
                    {
                        GameObject managerGO = new GameObject("UIInputHandler_AutoCreated");
                        _instance = managerGO.AddComponent<UIInputHandler>();
                    }
                }
                return _instance;
            }
        }

        private Dictionary<KeyCode, List<UIWidget>> _keyBindings = new Dictionary<KeyCode, List<UIWidget>>();
        private UIWidget _currentlyFocusedWidget;
        public UIWidget FocusedWidget => _currentlyFocusedWidget;

        private bool _isInputFieldFocused = false;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void Update()
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
                            if (widget != null)
                            {
                                widget.Toggle();
                            }
                        }
                    }
                }
            }
            
            if (_currentlyFocusedWidget != null)
            {
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    var pointerData = new PointerEventData(EventSystem.current);
                    _currentlyFocusedWidget.OnPointerClick(pointerData);
                }

                if (!_isInputFieldFocused)
                {
                    if (Input.GetKeyDown(KeyCode.UpArrow) && _currentlyFocusedWidget.selectOnUp != null)
                    {
                        SetFocus(_currentlyFocusedWidget.selectOnUp);
                    }
                    else if (Input.GetKeyDown(KeyCode.DownArrow) && _currentlyFocusedWidget.selectOnDown != null)
                    {
                        SetFocus(_currentlyFocusedWidget.selectOnDown);
                    }
                    else if (Input.GetKeyDown(KeyCode.LeftArrow) && _currentlyFocusedWidget.selectOnLeft != null)
                    {
                        SetFocus(_currentlyFocusedWidget.selectOnLeft);
                    }
                    else if (Input.GetKeyDown(KeyCode.RightArrow) && _currentlyFocusedWidget.selectOnRight != null)
                    {
                        SetFocus(_currentlyFocusedWidget.selectOnRight);
                    }
                }
            }
        }

        public void RegisterToggleKey(KeyCode key, UIWidget widget)
        {
            if (key == KeyCode.None || widget == null) return;
            if (!_keyBindings.ContainsKey(key))
            {
                _keyBindings[key] = new List<UIWidget>();
            }
            if (!_keyBindings[key].Contains(widget))
            {
                _keyBindings[key].Add(widget);
            }
        }

        public void UnregisterToggleKey(KeyCode key, UIWidget widget)
        {
            if (key == KeyCode.None || widget == null) return;
            if (_keyBindings.ContainsKey(key))
            {
                _keyBindings[key].Remove(widget);
                if (_keyBindings[key].Count == 0)
                {
                    _keyBindings.Remove(key);
                }
            }
        }

        public void SetFocus(UIWidget widget)
        {
            if (_currentlyFocusedWidget == widget) return;
            
            if (_currentlyFocusedWidget != null)
            {
                _currentlyFocusedWidget.OnFocusLost();
            }

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
    }
}