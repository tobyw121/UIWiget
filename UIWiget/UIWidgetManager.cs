// Dateiname: UIWidgetManager.cs (Verbesserungen)
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace YourGame.UI.Widgets
{
    public class UIWidgetManager : MonoBehaviour
    {
        private static UIWidgetManager _instance;
        private static readonly object _lock = new object();

        public static UIWidgetManager Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = FindObjectOfType<UIWidgetManager>();
                        if (_instance == null)
                        {
                            GameObject managerGO = new GameObject("UIWidgetManager_AutoCreated");
                            _instance = managerGO.AddComponent<UIWidgetManager>();
                        }
                    }
                    return _instance;
                }
            }
        }

        private Dictionary<string, List<UIWidget>> _widgetCache = new Dictionary<string, List<UIWidget>>();

        // Für Cursor-Management (KAUI-ähnlich)
        public static event Action<string> OnCursorChanged;
        private static string _currentCursorName = "Arrow"; // Standard-Cursor-Name
        private static bool _isExclusiveCursorActive = false; // Status für exklusiven Cursor

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

        private void OnEnable()
        {
            PopulateWidgetCache();
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            _widgetCache.Clear();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            PopulateWidgetCache();
        }

        private void OnSceneUnloaded(Scene scene)
        {
            var widgetsToRemove = _widgetCache.SelectMany(kv => kv.Value)
                                              .Where(w => w != null && w.gameObject.scene == scene)
                                              .ToList();
            foreach (var widget in widgetsToRemove)
            {
                UnregisterWidget(widget);
            }
        }

        private void PopulateWidgetCache()
        {
            _widgetCache.Clear();
            var allWidgetsInLoadedScenes = FindObjectsOfType<UIWidget>(true);
            foreach (var widget in allWidgetsInLoadedScenes)
            {
                if (widget.gameObject.scene.isLoaded)
                {
                    RegisterWidget(widget);
                }
            }
        }

        public void RegisterWidget(UIWidget widget)
        {
            if (widget == null || string.IsNullOrEmpty(widget.Name))
            {
                Debug.LogWarning($"[UIWidgetManager] Attempted to register a null or nameless widget: {widget?.gameObject.name ?? "NULL"}");
                return;
            }
            if (!_widgetCache.ContainsKey(widget.Name))
            {
                _widgetCache[widget.Name] = new List<UIWidget>();
            }
            if (!_widgetCache[widget.Name].Contains(widget))
            {
                _widgetCache[widget.Name].Add(widget);
            }
        }

        public void UnregisterWidget(UIWidget widget)
        {
            if (widget == null || string.IsNullOrEmpty(widget.Name)) return;
            if (_widgetCache.ContainsKey(widget.Name))
            {
                _widgetCache[widget.Name].Remove(widget);
                if (_widgetCache[widget.Name].Count == 0)
                {
                    _widgetCache.Remove(widget.Name);
                }
            }
        }

        public static T Find<T>(string name) where T : UIWidget
        {
            if (Instance._widgetCache.TryGetValue(name, out var widgets))
            {
                return widgets.OfType<T>().FirstOrDefault();
            }
            return null;
        }

        public static List<T> FindAll<T>() where T : UIWidget
        {
            List<T> allOfType = new List<T>();
            foreach (var list in Instance._widgetCache.Values)
            {
                allOfType.AddRange(list.OfType<T>());
            }
            return allOfType.Distinct().ToList();
        }

        /// <summary>
        /// Setzt den Standard-Cursor.
        /// Entspricht KAUICursorManager.SetDefaultCursor.
        /// </summary>
        /// <param name="cursorName">Der Name des Cursors.</param>
        public static void SetDefaultCursor(string cursorName = "Arrow")
        {
            _currentCursorName = cursorName;
            Debug.Log($"[UIWidgetManager] Cursor set to: {_currentCursorName}");
            OnCursorChanged?.Invoke(_currentCursorName); // Informiert alle Listener
        }

        /// <summary>
        /// Aktiviert oder deaktiviert einen exklusiven Lade-Cursor.
        /// Entspricht KAUICursorManager.SetExclusiveLoadingGear.
        /// </summary>
        /// <param name="status">True zum Aktivieren, False zum Deaktivieren.</param>
        public static void SetExclusiveLoadingGear(bool status)
        {
            _isExclusiveCursorActive = status;
            if (status)
            {
                SetDefaultCursor("Loading");
                Debug.Log("[UIWidgetManager] Exclusive loading cursor activated.");
                // Hier könnte Logik zum Anzeigen eines globalen Lade-Overlays stehen
                // oder das Haupt-UI als nicht interaktiv markiert werden,
                // um die Exklusivität zu simulieren.
            }
            else
            {
                SetDefaultCursor("Arrow"); // Zurück zum Standard-Cursor
                Debug.Log("[UIWidgetManager] Exclusive loading cursor deactivated.");
                // Hier könnte Logik zum Ausblenden des Overlays stehen
            }
        }

        /// <summary>
        /// Simuliert die SetExclusive-Funktion von KAUI für ein UIWidget.
        /// Dies müsste eine externe Logik sein, die ein CanvasGroup-Overlay steuert.
        /// Das UIWidget selbst kennt KAUI nicht.
        /// </summary>
        /// <param name="widget">Das Widget, das exklusiv gemacht werden soll.</param>
        /// <param name="maskColor">Die Farbe des Masken-Overlays.</param>
        public static void SetExclusive(UIWidget widget, Color maskColor)
        {
            Debug.Log($"[UIWidgetManager] Widget '{widget.Name}' set as exclusive with mask color {maskColor}.");
            UIOverlayManager.Instance.ActivateExclusiveOverlay(maskColor);
        }

        /// <summary>
        /// Simuliert die RemoveExclusive-Funktion von KAUI für ein UIWidget.
        /// </summary>
        /// <param name="widget">Das Widget, dessen Exklusiv-Status entfernt werden soll.</param>
        public static void RemoveExclusive(UIWidget widget)
        {
            Debug.Log($"[UIWidgetManager] Exclusive status removed for widget '{widget.Name}'.");
            UIOverlayManager.Instance.DeactivateExclusiveOverlay();
        }
    }
}