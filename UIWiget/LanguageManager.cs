using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Events;
using UnityEngine.Networking; // Required for UnityWebRequest

namespace YourGame.UI
{
public class LanguageManager : MonoBehaviour
{
    private static LanguageManager _instance;
    public static LanguageManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<LanguageManager>();
                if (_instance == null)
                {
                    GameObject lmGo = new GameObject("LanguageManager_AutoCreated");
                    _instance = lmGo.AddComponent<LanguageManager>();
                    // Debug.LogWarning("LanguageManager was auto-created as it was not found in scene. Ensure it's properly set up.");
                }
            }
            // If accessed this way before its own Awake has run and initialized.
            if (Application.isPlaying && !_instance._isInitialized) {
                // Debug.LogWarning("LanguageManager.Instance accessed but _isInitialized was false. Attempting late LoadLanguage.");
                _instance.LoadLanguage(_instance._currentLanguage); // Attempt to initialize if not done by Awake
            }
            return _instance;
        }
    }

    public UnityEvent OnLanguageChanged = new UnityEvent();

    private Dictionary<string, string> _localizedStrings;
    private string _currentLanguage = "en"; // Default language
    private bool _isInitialized = false;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            // Load language if not already initialized (e.g., by static getter)
            if (!_isInitialized)
            {
                LoadLanguage(_currentLanguage);
            }
        }
        else if (_instance != this)
        {
            Debug.LogWarning("Another instance of LanguageManager found, destroying this duplicate.");
            Destroy(gameObject);
        }
    }

    public void LoadLanguage(string languageCode)
    {
        // Prevent re-initialization with the same language if already initialized, unless forced somehow
        if (_isInitialized && _currentLanguage == languageCode && _localizedStrings != null && _localizedStrings.Count > 0) {
            // Already initialized with this language, and strings are loaded.
            // Useful if something tries to call LoadLanguage again unnecessarily.
            // However, if a force reload is intended, this check might need to be bypassed.
            // For now, if _isInitialized is false, it will proceed.
        }


        _currentLanguage = languageCode;
        _localizedStrings = new Dictionary<string, string>(); // Ensure it's a new dictionary

        string filePath = Path.Combine(Application.streamingAssetsPath, "translations.json");
        string jsonString = null;

        if (Application.platform == RuntimePlatform.Android)
        {
            UnityWebRequest reader = UnityWebRequest.Get(filePath);
            reader.SendWebRequest();
            // This is synchronous and will block; for smoother loading, use a Coroutine.
            while (!reader.isDone) { /* Block */ }

            if (reader.result == UnityWebRequest.Result.Success)
            {
                jsonString = reader.downloadHandler.text;
            }
            else
            {
                Debug.LogError($"Failed to load translations from Android StreamingAssets: {reader.error} at path {filePath}");
            }
            reader.Dispose();
        }
        else // For Editor and most other desktop/console platforms
        {
            if (File.Exists(filePath))
            {
                try
                {
                    jsonString = File.ReadAllText(filePath);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error reading translation file: {e.Message} at path {filePath}");
                }
            }
            else
            {
                Debug.LogError($"translations.json not found at path: {filePath}");
            }
        }

        if (!string.IsNullOrEmpty(jsonString))
        {
            try
            {
                TranslationData data = JsonUtility.FromJson<TranslationData>(jsonString);
                if (data != null && data.languages != null)
                {
                    bool languageFound = false;
                    foreach (LanguageData language in data.languages)
                    {
                        if (language.language == _currentLanguage)
                        {
                            languageFound = true;
                            if (language.translations != null)
                            {
                                foreach (TranslationEntry translation in language.translations)
                                {
                                    if (!string.IsNullOrEmpty(translation.key) && !_localizedStrings.ContainsKey(translation.key))
                                    {
                                        _localizedStrings.Add(translation.key, translation.value);
                                    }
                                    else if (_localizedStrings.ContainsKey(translation.key))
                                    {
                                        Debug.LogWarning($"Duplicate translation key: '{translation.key}' for language '{_currentLanguage}'. Using first instance.");
                                    }
                                }
                            }
                            break;
                        }
                    }
                    if (!languageFound) Debug.LogWarning($"Language code '{_currentLanguage}' not found in translations.json.");
                }
                else
                {
                    Debug.LogError("Failed to parse translations.json: data or languages array is null.");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error parsing translations.json: {e.Message}\nJSON content attempt: {jsonString.Substring(0, Mathf.Min(jsonString.Length, 500))}");
            }
        }
        
        _isInitialized = true; // Mark as initialized even if loading failed, to prevent repeated load attempts from GetString.
                             // The GetString method will then return "MISSING TRANSLATION" for keys.
        OnLanguageChanged.Invoke();
    }

    public string GetString(string key)
    {
        if (!_isInitialized)
        {
            // This can happen if GetString is called from an Awake/Start method of another script
            // that executes before LanguageManager's Awake. The static getter now also attempts to call LoadLanguage.
            Debug.LogWarning($"LanguageManager.GetString called for key '{key}' but manager was not fully initialized. Attempting late LoadLanguage via Instance property or it was already called.");
            // The Instance getter would have already tried to call LoadLanguage if _isInitialized was false.
            // If it's still false, it means LoadLanguage ran but might not have completed successfully or _isInitialized was not set.
            // No need to call LoadLanguage() here again if the Instance getter already did.
        }

        if (_localizedStrings != null && !string.IsNullOrEmpty(key) && _localizedStrings.ContainsKey(key))
        {
            return _localizedStrings[key];
        }
        
        // Debug.LogWarning($"Translation key not found: '{key}' for language '{_currentLanguage}'");
        return "MISSING TRANSLATION: " + key;
    }

    public string GetString(string key, string defaultValue)
    {
        string value = GetString(key); // This now uses the primary GetString
        if (value.StartsWith("MISSING TRANSLATION:") || string.IsNullOrEmpty(value) && value != key) // check if it returned the "MISSING..." string
        {
            return defaultValue;
        }
        return value;
    }

    public void SetLanguage(string languageCode)
    {
        if (_currentLanguage != languageCode || !_isInitialized)
        {
            // Debug.Log($"Setting language to: {languageCode}. Was: {_currentLanguage}. Initialized: {_isInitialized}");
            LoadLanguage(languageCode);
        }
    }
}

// Helper classes for JSON parsing (ensure these are not accidentally duplicated elsewhere)
[System.Serializable]
public class TranslationData
{
    public LanguageData[] languages;
}

[System.Serializable]
public class LanguageData
{
    public string language;
    public TranslationEntry[] translations;
}

[System.Serializable]
public class TranslationEntry
{
    public string key;
    public string value;
}
}
