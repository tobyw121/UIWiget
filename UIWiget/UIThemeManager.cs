// Dateiname: UIThemeManager.cs (Verbessert)
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class UIThemeManager : MonoBehaviour
{
    public static UIThemeManager Instance { get; private set; }

    // Event, das bei Theme-Wechsel ausgelöst wird
    public static event Action OnThemeChanged;

    [Header("Theme Settings")]
    [Tooltip("Das UI-Theme, das für die gesamte Anwendung aktiv sein soll.")]
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

    /// <summary>
    /// Setzt ein neues aktives Theme zur Laufzeit und benachrichtigt alle Widgets.
    /// </summary>
    public void SetActiveTheme(UIThemeData newTheme)
    {
        if (newTheme == null || newTheme == _activeTheme) return;

        _activeTheme = newTheme;
        CacheActiveTheme(); // Baut den Cache mit den neuen Stilen neu auf.
        OnThemeChanged?.Invoke(); // Löst das Event aus, um alle UIs zu aktualisieren.
        Debug.Log($"[UIThemeManager] Aktives Theme zu '{newTheme.name}' geändert.");
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
    
    /// <summary>
    /// Öffentliche Methode, die vom Custom Editor zur Anzeige der verfügbaren Stile verwendet wird.
    /// </summary>
    public List<string> GetAllStyleKeys()
    {
        if (_styleCache == null)
        {
            CacheActiveTheme();
        }
        return _styleCache.Keys.ToList();
    }
}