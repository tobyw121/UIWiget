// Dateiname: LanguageManager.cs
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace YourGame.UI
{

public class LanguageManager : MonoBehaviour
{
    public static LanguageManager Instance { get; private set; }

    public UnityEvent OnLanguageChanged = new UnityEvent();

    private Dictionary<string, string> _translationsDE = new Dictionary<string, string>
    {
        { "welcome_message", "Willkommen!" },
        { "character_name", "Held" },
        { "action_attack", "Angreifen" }
    };

    private Dictionary<string, string> _translationsEN = new Dictionary<string, string>
    {
        { "welcome_message", "Welcome!" },
        { "character_name", "Hero" },
        { "action_attack", "Attack" }
    };

    private bool _isGerman = true;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public string GetString(string key)
    {
        Dictionary<string, string> currentDict = _isGerman ? _translationsDE : _translationsEN;
        if (currentDict.TryGetValue(key, out string value))
        {
            return value;
        }
        return $"#{key}#"; // Zeigt an, dass der Schlüssel fehlt
    }

    public void ToggleLanguage()
    {
        _isGerman = !_isGerman;
        Debug.Log($"Sprache umgeschaltet auf: {(_isGerman ? "Deutsch" : "Englisch")}");
        OnLanguageChanged?.Invoke();
    }
}
}
