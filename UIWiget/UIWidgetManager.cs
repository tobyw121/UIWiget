// Dateiname: UIWidgetManager.cs
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
        private static bool _isShuttingDown = false;

        public static UIWidgetManager Instance
        {
            get
            {
                if (_isShuttingDown)
                {
                    Debug.LogWarning("[UIWidgetManager] Instance called during application quit. Returning null.");
                    return null;
                }

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

        private readonly Dictionary<string, List<UIWidget>> _widgetCache = new Dictionary<string, List<UIWidget>>();
        
        // Event für Cursor-Änderungen
        public static event Action<string> OnCursorChanged;
        private static string _currentCursorName = "Arrow";
        private static bool _isExclusiveCursorActive = false;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            _isShuttingDown = false; // Wichtig für Szenen-Neuladen
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            PopulateWidgetCache();
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            _widgetCache.Clear();
        }
        
        private void OnApplicationQuit()
        {
            _isShuttingDown = true;
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
            if (Instance != null && Instance._widgetCache.TryGetValue(name, out var widgets))
            {
                return widgets.OfType<T>().FirstOrDefault();
            }
            return null;
        }

        public static List<T> FindAll<T>() where T : UIWidget
        {
            List<T> allOfType = new List<T>();
            if (Instance != null)
            {
                 foreach (var list in Instance._widgetCache.Values)
                {
                    allOfType.AddRange(list.OfType<T>());
                }
            }
            return allOfType.Distinct().ToList();
        }
        
        public static void SetExclusive(UIWidget widget, Color maskColor)
        {
            if (Instance == null) return;
            Debug.Log($"[UIWidgetManager] Widget '{widget?.Name}' set as exclusive with mask color {maskColor}.");
            UIOverlayManager.Instance.ActivateExclusiveOverlay(maskColor);
        }

        public static void RemoveExclusive(UIWidget widget)
        {
            if (Instance == null) return;
            Debug.Log($"[UIWidgetManager] Exclusive status removed for widget '{widget?.Name}'.");
            UIOverlayManager.Instance.DeactivateExclusiveOverlay();
        }
        
        public static void SetDefaultCursor(string cursorName = "Arrow")
        {
            if (_isExclusiveCursorActive) return;
            _currentCursorName = cursorName;
            OnCursorChanged?.Invoke(_currentCursorName);
        }

        public static void SetExclusiveLoadingGear(bool status)
        {
            _isExclusiveCursorActive = status;
            if (status)
            {
                OnCursorChanged?.Invoke("Loading");
            }
            else
            {
                OnCursorChanged?.Invoke(_currentCursorName);
            }
        }
    }
}