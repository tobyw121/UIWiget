// Dateiname: UIWidgetManager.cs
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using YourGame.UI.Widgets.Cursors;

namespace YourGame.UI.Widgets
{
    public class UIWidgetManager : MonoBehaviour
    {
        private static UIWidgetManager _instance;
        private static bool _isShuttingDown;

        [SerializeField] private CursorDescriptor defaultCursorDescriptor = CursorDescriptor.FromId("Arrow");
        [SerializeField] private CursorDescriptor loadingCursorDescriptor = CursorDescriptor.FromId("Loading");

        public static UIWidgetManager Instance
        {
            get
            {
                if (_isShuttingDown)
                {
                    Debug.LogWarning("[UIWidgetManager] Instance requested while shutting down. Returning null.");
                    return null;
                }

                if (_instance == null)
                {
                    _instance = LocateExistingManager();
                    if (_instance == null)
                    {
                        GameObject managerGO = new GameObject("UIWidgetManager_AutoCreated");
                        _instance = managerGO.AddComponent<UIWidgetManager>();
                    }
                }

                return _instance;
            }
        }

        public static event Action<CursorDescriptor> OnCursorChanged;

        private readonly Dictionary<string, List<UIWidget>> _widgetCache = new Dictionary<string, List<UIWidget>>();
        private CursorDescriptor _currentCursor;
        private bool _isExclusiveCursorActive;
        private ICursorProvider _cursorProvider;

        public ICursorProvider CursorProvider
        {
            get
            {
                if (_cursorProvider == null)
                {
                    _cursorProvider = new EventCursorProvider(defaultCursorDescriptor, loadingCursorDescriptor);
                }

                return _cursorProvider;
            }
            set
            {
                _cursorProvider = value ?? new EventCursorProvider(defaultCursorDescriptor, loadingCursorDescriptor);
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            _isShuttingDown = false;
            DontDestroyOnLoad(gameObject);
            _currentCursor = defaultCursorDescriptor;
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            RefreshCache();
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

        private static UIWidgetManager LocateExistingManager()
        {
            var managers = Resources.FindObjectsOfTypeAll<UIWidgetManager>();
            return managers.FirstOrDefault(m => m != null && m.hideFlags == HideFlags.None && m.gameObject.scene.IsValid());
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            RegisterSceneWidgets(scene);
        }

        private void OnSceneUnloaded(Scene scene)
        {
            var widgetsToRemove = _widgetCache
                .SelectMany(kv => kv.Value)
                .Where(w => w != null && w.gameObject.scene == scene)
                .ToList();

            foreach (var widget in widgetsToRemove)
            {
                UnregisterWidget(widget);
            }
        }

        private void RefreshCache()
        {
            foreach (var key in _widgetCache.Keys.ToList())
            {
                _widgetCache[key].RemoveAll(widget => widget == null);
                if (_widgetCache[key].Count == 0)
                {
                    _widgetCache.Remove(key);
                }
            }

            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.isLoaded)
                {
                    RegisterSceneWidgets(scene);
                }
            }

            RegisterPersistentWidgets();
        }

        private void RegisterSceneWidgets(Scene scene)
        {
            if (!scene.IsValid() || !scene.isLoaded)
            {
                return;
            }

            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var widget in root.GetComponentsInChildren<UIWidget>(true))
                {
                    RegisterWidget(widget);
                }
            }
        }

        private void RegisterPersistentWidgets()
        {
            var persistentWidgets = Resources.FindObjectsOfTypeAll<UIWidget>()
                .Where(widget => widget != null && widget.hideFlags == HideFlags.None && widget.gameObject.scene.name == "DontDestroyOnLoad");

            foreach (var widget in persistentWidgets)
            {
                RegisterWidget(widget);
            }
        }

        public void RegisterWidget(UIWidget widget)
        {
            if (widget == null || string.IsNullOrEmpty(widget.Name))
            {
                return;
            }

            if (!_widgetCache.TryGetValue(widget.Name, out var widgets))
            {
                widgets = new List<UIWidget>();
                _widgetCache[widget.Name] = widgets;
            }

            if (!widgets.Contains(widget))
            {
                widgets.Add(widget);
            }
        }

        public void UnregisterWidget(UIWidget widget)
        {
            if (widget == null || string.IsNullOrEmpty(widget.Name))
            {
                return;
            }

            if (_widgetCache.TryGetValue(widget.Name, out var widgets))
            {
                widgets.Remove(widget);
                if (widgets.Count == 0)
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
            var result = new List<T>();
            if (Instance != null)
            {
                foreach (var list in Instance._widgetCache.Values)
                {
                    result.AddRange(list.OfType<T>());
                }
            }

            return result.Distinct().ToList();
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

        public static void SetDefaultCursor(string cursorId)
        {
            SetDefaultCursor(CursorDescriptor.FromId(cursorId));
        }

        public static void SetDefaultCursor(CursorDescriptor descriptor = null)
        {
            if (Instance == null) return;
            if (Instance._isExclusiveCursorActive) return;

            Instance._currentCursor = descriptor ?? Instance.CursorProvider.DefaultCursor;
            Instance.ApplyCursor(Instance._currentCursor);
        }

        public static void SetExclusiveLoadingGear(bool status)
        {
            if (Instance == null) return;

            Instance._isExclusiveCursorActive = status;
            if (status)
            {
                Instance.ApplyCursor(Instance.CursorProvider.LoadingCursor);
            }
            else
            {
                var fallback = Instance._currentCursor ?? Instance.CursorProvider.DefaultCursor;
                Instance.ApplyCursor(fallback);
            }
        }

        private void ApplyCursor(CursorDescriptor descriptor)
        {
            var provider = CursorProvider;
            provider.ApplyCursor(descriptor);
            OnCursorChanged?.Invoke(descriptor ?? provider.DefaultCursor);
        }
    }
}
