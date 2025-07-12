// Dateiname: UIInputHandler.cs (Korrigiert)
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.EventSystems; // KORRIGIERT: Fehlende using-Anweisung hinzugefügt

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

        // Speichert, welche Widgets auf welche Taste reagieren sollen
        private Dictionary<KeyCode, List<UIWidget>> _keyBindings = new Dictionary<KeyCode, List<UIWidget>>();
        // System für UI-Fokus und Navigation
        private UIWidget _currentlyFocusedWidget;
        public UIWidget FocusedWidget => _currentlyFocusedWidget;

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
            // Bestehende Logik für Toggle-Tasten
            if (_keyBindings.Count > 0)
            {
                // Um Modifikationen während der Iteration zu vermeiden, falls ein Toggle die Liste ändert.
                var keys = _keyBindings.Keys.ToList();
                foreach (var key in keys)
                {
                    if (Input.GetKeyDown(key))
                    {
                        // Eine Kopie der Liste erstellen
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
            
            // Logik für UI-Navigation mit dem fokussierten Widget
            if (_currentlyFocusedWidget != null)
            {
                // Beispiel für eine "Submit"-Aktion auf dem fokussierten Widget
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    // Simulieren eines Klicks auf das fokussierte Element
                    var pointerData = new PointerEventData(EventSystem.current);
                    _currentlyFocusedWidget.OnPointerClick(pointerData);
                }
            }
        }

        /// <summary>
        /// Registriert ein UIWidget für eine bestimmte Toggle-Taste.
        /// </summary>
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

        /// <summary>
        /// Deregistriert ein UIWidget von einer bestimmten Toggle-Taste.
        /// </summary>
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

        /// <summary>
        /// Setzt den Fokus auf ein bestimmtes UIWidget.
        /// Nützlich für Tastatur- und Gamepad-Navigation.
        /// </summary>
        /// <param name="widget">Das Widget, das den Fokus erhalten soll.</param>
        public void SetFocus(UIWidget widget)
        {
            if (_currentlyFocusedWidget == widget) return;
            // Altes Widget verliert den Fokus
            if (_currentlyFocusedWidget != null)
            {
                _currentlyFocusedWidget.OnFocusLost();
            }

            _currentlyFocusedWidget = widget;
            // Neues Widget erhält den Fokus
            if (_currentlyFocusedWidget != null)
            {
                _currentlyFocusedWidget.OnFocusGained();
            }
        }
    }
}