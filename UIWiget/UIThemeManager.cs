using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class UIThemeManager : MonoBehaviour
{
    public static UIThemeManager Instance { get; private set; }

    public static event Action OnThemeChanged;

    [Header("Theme Settings")]
    [SerializeField] private UIThemeData _activeTheme;

    private Dictionary<string, UIStyle> _styleCache;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            CacheActiveTheme();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetActiveTheme(UIThemeData newTheme)
    {
        if (newTheme == null || newTheme == _activeTheme) return;

        _activeTheme = newTheme;
        CacheActiveTheme();
        OnThemeChanged?.Invoke();
    }

    private void CacheActiveTheme()
    {
        _styleCache = new Dictionary<string, UIStyle>();
        if (_activeTheme == null)
        {
            Debug.LogWarning("[UIThemeManager] Kein aktives UI-Theme zugewiesen!");
            return;
        }

        foreach (var style in _activeTheme.styles)
        {
            if (style != null && !string.IsNullOrEmpty(style.styleKey))
            {
                if (!_styleCache.ContainsKey(style.styleKey))
                {
                    _styleCache.Add(style.styleKey, style);
                }
                else
                {
                    Debug.LogWarning($"[UIThemeManager] Doppelter Style Key im Theme gefunden: {style.styleKey}");
                }
            }
        }
    }

    public UIStyle GetStyle(string key)
    {
        if (string.IsNullOrEmpty(key) || _styleCache == null)
        {
            return null;
        }
        _styleCache.TryGetValue(key, out UIStyle foundStyle);
        return foundStyle;
    }
    
    public List<string> GetAllStyleKeys()
    {
        if (_styleCache == null)
        {
            CacheActiveTheme();
        }
        return _styleCache.Keys.ToList();
    }
}