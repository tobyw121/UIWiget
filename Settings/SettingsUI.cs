using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using YourGame.UI.Widgets;
using YourGame.UI;


public class SettingsUI : UIWidget
{
    public static SettingsUI Instance { get; private set; }

    [Header("UI Panels")]
    public GameObject settingsPanel;
    public GameObject tabsParent;
    public GameObject tabButtonPrefab;
    public GameObject graphicsPanel;
    public GameObject controlsPanel;
    public GameObject audioPanel;
    public GameObject languagePanel;
    public GameObject gameplayPanel;

    [Header("Graphics Settings")]
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;
    public TMP_Dropdown qualityDropdown;

    [Header("Audio Settings")]
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;

    [Header("Language Settings")]
    public TMP_Dropdown languageDropdown;

    [Header("Gameplay Settings")]
    public Slider mouseSensitivitySlider;
    public Toggle invertYAxisToggle;

    [Header("Control Settings")]
    public GameObject keyBindingEntryPrefab;
    public Transform keyBindingsParent;

    private GameObject _currentActiveSubPanel;
    private Resolution[] _availableResolutions;
    private string _rebindingKeyFieldName;
    private Button _activeRebindButton;

    private UIManager _uiManager; // Referenz zum UIManager

    protected override void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Sicherstellen, dass settingsPanel auf dieses GameObject zeigt, falls nicht im Editor zugewiesen
        if (settingsPanel == null) settingsPanel = gameObject;
        base.Awake(); 

        // Temporär alle Panels aktivieren, um Referenzen zu finden
        bool wasActive = settingsPanel.activeSelf;
        if (!wasActive) settingsPanel.SetActive(true); // Temporär aktivieren

        FindOrAssignUIReferences();

        if (!wasActive) settingsPanel.SetActive(false); // Ursprünglichen Zustand wiederherstellen


        // UIManager Instanz cachen
        _uiManager = UIManager.Instance;
        if (_uiManager == null)
        {
            Debug.LogError("[SettingsUI] UIManager.Instance ist null! Die Einstellungen werden möglicherweise nicht korrekt mit der Spielersteuerung interagieren.");
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        SetupTabs();
        ShowSubPanel(graphicsPanel); // Standardmäßig Grafikpanel anzeigen

        // Setup der einzelnen Panels
        SetupLanguageSettings();
        SetupGraphicsSettings();
        SetupAudioSettings();
        SetupGameplaySettings();
        SetupControlSettings();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        // Hier können Aufräumarbeiten stattfinden, z.B. wenn Listener nicht durch OnEnable/OnDisable von Unity selbst verwaltet werden.
    }

    public override void Show()
    {
        // Nur anzeigen, wenn der Spieler geladen ist (für Spielszenen)
        if (GameManager.Instance == null || !GameManager.Instance.playerLoaded)
        {
            Debug.LogWarning("[SettingsUI] Einstellungen können nur in Spielszenen geöffnet werden, wenn der Spieler geladen ist.");
            base.Hide(); // Verstecken, falls irrtümlich aufgerufen
            return;
        }

        // UIManager bitten, dieses Panel zu öffnen
        if (_uiManager != null)
        {
            _uiManager.OpenPanel(this.settingsPanel);
        }
        else
        {
            // Fallback: Direkt aktivieren, aber es wird nicht über den UIManager verwaltet.
            Debug.LogWarning("[SettingsUI] UIManager.Instance ist null. Settings-Panel direkt aktivieren.");
            base.Show(); 
        }

        // Panels neu einrichten, um frische Daten und Listener zu gewährleisten
        SetupTabs();
        ShowSubPanel(graphicsPanel); 
        SetupLanguageSettings();
        SetupGraphicsSettings();
        SetupAudioSettings();
        SetupGameplaySettings();
        SetupControlSettings();
    }

    public override void Hide()
    {
        // UIManager bitten, das Panel zu schließen
        if (_uiManager != null)
        {
            _uiManager.CloseCurrentPanel();
        }
        else
        {
            // Fallback: Direkt deaktivieren
            Debug.LogWarning("[SettingsUI] UIManager.Instance ist null. Settings-Panel direkt deaktivieren.");
            base.Hide();
        }
    }

    // Sucht oder weist UI-Komponenten zu, falls nicht im Editor zugewiesen
    private void FindOrAssignUIReferences()
    {
        // Debug.Log("[SettingsUI] Starte FindOrAssignUIReferences...");

        // Prüfe ob Root-Panel vorhanden ist
        if (settingsPanel == null)
        {
            Debug.LogError("[SettingsUI] settingsPanel ist nicht zugewiesen. UI kann nicht richtig funktionieren.");
            return;
        }

        // Versuche, Unterpanele und Prefabs zu finden, falls nicht zugewiesen
        Transform contentArea = settingsPanel.transform.Find("TabsAndContentArea/ContentArea");
        if (contentArea != null)
        {
            graphicsPanel = contentArea.Find("GraphicsPanel")?.gameObject;
            controlsPanel = contentArea.Find("ControlsPanel")?.gameObject;
            audioPanel = contentArea.Find("AudioPanel")?.gameObject;
            languagePanel = contentArea.Find("LanguagePanel")?.gameObject;
            gameplayPanel = contentArea.Find("GameplayPanel")?.gameObject;
        }
        else
        {
            Debug.LogWarning("[SettingsUI] 'ContentArea' wurde nicht gefunden. Überprüfen Sie die Hierarchie.");
        }

        tabsParent = settingsPanel.transform.Find("TabsAndContentArea/TabsPanel")?.gameObject;
        tabButtonPrefab = settingsPanel.transform.Find("SettingsTabButton_Prefab")?.gameObject;
        keyBindingEntryPrefab = settingsPanel.transform.Find("KeyBindingEntry_Prefab")?.gameObject;

        // Komponenten innerhalb der Panels finden
        if (graphicsPanel != null)
        {
            resolutionDropdown = graphicsPanel.transform.Find("ResolutionDropdown")?.GetComponent<TMP_Dropdown>();
            fullscreenToggle = graphicsPanel.transform.Find("FullscreenToggle")?.GetComponent<Toggle>();
            qualityDropdown = graphicsPanel.transform.Find("QualityDropdown")?.GetComponent<TMP_Dropdown>();
        }
        if (audioPanel != null)
        {
            musicVolumeSlider = audioPanel.transform.Find("MusicVolumeSlider")?.GetComponent<Slider>();
            sfxVolumeSlider = audioPanel.transform.Find("SFXVolumeSlider")?.GetComponent<Slider>();
        }
        if (languagePanel != null)
        {
            languageDropdown = languagePanel.transform.Find("LanguageDropdown")?.GetComponent<TMP_Dropdown>();
        }
        if (gameplayPanel != null)
        {
            mouseSensitivitySlider = gameplayPanel.transform.Find("MouseSensitivitySlider")?.GetComponent<Slider>();
            invertYAxisToggle = gameplayPanel.transform.Find("InvertYAxisToggle")?.GetComponent<Toggle>();
        }
        if (controlsPanel != null)
        {
            keyBindingsParent = controlsPanel.transform.Find("KeyBindingsScrollArea/Viewport/Content")?.GetComponent<Transform>();
        }
        // Debug.Log("[SettingsUI] FindOrAssignUIReferences ABGESCHLOSSEN.");
    }

    // Erstellt die Tab-Buttons dynamisch
    private void SetupTabs()
    {
        if (tabsParent == null)
        {
            Debug.LogError("[SettingsUI] tabsParent ist null, kann Tabs nicht einrichten.");
            return;
        }

        // Bestehende Tabs löschen
        for (int i = tabsParent.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(tabsParent.transform.GetChild(i).gameObject);
        }

        CreateTabButton("Grafik", graphicsPanel);
        CreateTabButton("Steuerung", controlsPanel);
        CreateTabButton("Audio", audioPanel);
        CreateTabButton("Sprache", languagePanel);
        CreateTabButton("Gameplay", gameplayPanel);
    }

    // Erstellt einen einzelnen Tab-Button
    private void CreateTabButton(string tabName, GameObject targetPanel)
    {
        if (tabButtonPrefab == null || targetPanel == null || tabsParent == null)
        {
            Debug.LogWarning($"[SettingsUI] Fehlendes Prefab, Zielpanel oder Tab-Parent für '{tabName}' Tab. Überspringe.");
            return;
        }

        GameObject buttonGO = Instantiate(tabButtonPrefab, tabsParent.transform);
        TextMeshProUGUI buttonText = buttonGO.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null) buttonText.text = tabName;

        Button button = buttonGO.GetComponent<Button>();
        if (button != null) button.onClick.AddListener(() => ShowSubPanel(targetPanel));
    }

    // Zeigt ein spezifisches Unterpanel an und versteckt andere
    public void ShowSubPanel(GameObject panelToShow)
    {
        if (panelToShow == null) return;
        GameObject[] allSubPanels = { graphicsPanel, controlsPanel, audioPanel, languagePanel, gameplayPanel };
        foreach (GameObject panel in allSubPanels)
        {
            if (panel != null) panel.SetActive(panel == panelToShow);
        }
        _currentActiveSubPanel = panelToShow;
        UpdateTabButtonVisuals(panelToShow);
    }

    // Aktualisiert die visuelle Darstellung der Tab-Buttons (aktiv/inaktiv)
    private void UpdateTabButtonVisuals(GameObject activePanel)
    {
        if (tabsParent == null) return;
        foreach (Transform child in tabsParent.transform)
        {
            Button btn = child.GetComponent<Button>();
            TextMeshProUGUI btnText = child.GetComponentInChildren<TextMeshProUGUI>();
            if (btn != null)
            {
                bool isActiveTab = (GetPanelNameForTab(child.name) == activePanel.name);
                Color activeColor = new Color(0.2f, 0.5f, 0.8f);
                Color inactiveColor = new Color(0.15f, 0.15f, 0.15f);

                btn.GetComponent<Image>().color = isActiveTab ?
                activeColor : inactiveColor;
                if (btnText != null) btnText.color = isActiveTab ? Color.white : new Color(0.8f, 0.8f, 0.8f);
            }
        }
    }

    // Hilfsmethode zur Zuordnung des Panel-Namens zum Tab-Button-Namen
    private string GetPanelNameForTab(string buttonName)
    {
        if (buttonName.Contains("Grafik")) return graphicsPanel?.name;
        if (buttonName.Contains("Steuerung")) return controlsPanel?.name;
        if (buttonName.Contains("Audio")) return audioPanel?.name;
        if (buttonName.Contains("Sprache")) return languagePanel?.name;
        if (buttonName.Contains("Gameplay")) return gameplayPanel?.name;
        return null;
    }

    // Einrichten der Sprach-Einstellungen
    private void SetupLanguageSettings()
    {
        if (languageDropdown == null) { Debug.LogWarning("[SettingsUI] languageDropdown ist null. Überspringe Sprach-Setup."); return; }
        languageDropdown.onValueChanged.RemoveAllListeners(); 

        languageDropdown.ClearOptions();
        List<string> languages = new List<string>
        {
            LanguageManager.Instance.GetString("language_german", "Deutsch") + " (de)",
            LanguageManager.Instance.GetString("language_english", "English") + " (en)"
        };
        languageDropdown.AddOptions(languages);

        string currentLangCode = LanguageManager.Instance.GetString("current_language_code", "en");
        int selectedIndex = languages.FindIndex(l => l.Contains($"({currentLangCode})"));
        if (selectedIndex != -1) languageDropdown.value = selectedIndex;

        languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
    }

    // Callback für Sprachänderung
    private void OnLanguageChanged(int index)
    {
        string selectedOptionText = languageDropdown.options[index].text;
        string langCode = "en";
        if (selectedOptionText.Contains("(de)")) langCode = "de";
        else if (selectedOptionText.Contains("(en)")) langCode = "en";
        LanguageManager.Instance.SetLanguage(langCode);
        Debug.Log($"Sprache geändert zu: {langCode}");
    }

    // Einrichten der Grafik-Einstellungen
    private void SetupGraphicsSettings()
    {
        // Auflösung
        if (resolutionDropdown != null)
        {
            resolutionDropdown.onValueChanged.RemoveAllListeners();
            resolutionDropdown.ClearOptions();
            // Filtern nach aktueller Refresh Rate für relevantere Optionen
            _availableResolutions = Screen.resolutions
                .Where(res => Mathf.Approximately((float)res.refreshRateRatio.value, (float)Screen.currentResolution.refreshRateRatio.value))
                .ToArray();
            List<string> options = _availableResolutions.Select(res => $"{res.width} x {res.height} @ {Mathf.RoundToInt((float)res.refreshRateRatio.value)}Hz").ToList();
            resolutionDropdown.AddOptions(options);

            int currentResolutionIndex = -1;
            // Suche exakte Übereinstimmung, falls nicht gefunden, dann nur Breite/Höhe
            for (int i = 0; i < _availableResolutions.Length; i++)
            {
                if (_availableResolutions[i].width == Screen.currentResolution.width &&
                    _availableResolutions[i].height == Screen.currentResolution.height &&
                    Mathf.Approximately((float)_availableResolutions[i].refreshRateRatio.value, (float)Screen.currentResolution.refreshRateRatio.value))
                {
                    currentResolutionIndex = i;
                    break;
                }
            }
            if (currentResolutionIndex == -1) // Fallback, wenn keine exakte Übereinstimmung gefunden wurde
            {
                for (int i = 0; i < _availableResolutions.Length; i++)
                {
                    if (_availableResolutions[i].width == Screen.currentResolution.width && _availableResolutions[i].height == Screen.currentResolution.height)
                    {
                        currentResolutionIndex = i;
                        break;
                    }
                }
            }
            resolutionDropdown.value = currentResolutionIndex != -1 ? currentResolutionIndex : 0;
            resolutionDropdown.RefreshShownValue();
            resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        } else { Debug.LogWarning("[SettingsUI] resolutionDropdown ist null. Überspringe Auflösungs-Setup."); }

        // Vollbild
        if (fullscreenToggle != null)
        {
            fullscreenToggle.onValueChanged.RemoveAllListeners();
            fullscreenToggle.isOn = Screen.fullScreen;
            fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggled);
        } else { Debug.LogWarning("[SettingsUI] fullscreenToggle ist null. Überspringe Vollbild-Setup."); }

        // Qualität
        if (qualityDropdown != null)
        {
            qualityDropdown.onValueChanged.RemoveAllListeners();
            qualityDropdown.ClearOptions();
            qualityDropdown.AddOptions(QualitySettings.names.ToList());
            qualityDropdown.value = QualitySettings.GetQualityLevel();
            qualityDropdown.RefreshShownValue();
            qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
        } else { Debug.LogWarning("[SettingsUI] qualityDropdown ist null. Überspringe Qualitäts-Setup."); }
    }

    // Callback für Auflösungsänderung
    private void OnResolutionChanged(int index)
    {
        if (_availableResolutions == null || index < 0 || index >= _availableResolutions.Length) return;
        Resolution resolution = _availableResolutions[index];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode, resolution.refreshRateRatio);
    }

    // Callback für Vollbild-Umschaltung
    private void OnFullscreenToggled(bool isFullscreen)
    {
        Screen.fullScreenMode = isFullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
    }

    // Callback für Qualitätsänderung
    private void OnQualityChanged(int index)
    {
        QualitySettings.SetQualityLevel(index);
    }

    // Einrichten der Audio-Einstellungen
    private void SetupAudioSettings()
    {
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.RemoveAllListeners();
            if (GameManager.Instance != null && GameManager.Instance.LoadedGameData != null)
                musicVolumeSlider.value = GameManager.Instance.LoadedGameData.musicVolume;
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        } else { Debug.LogWarning("[SettingsUI] musicVolumeSlider ist null. Überspringe Musiklautstärke-Setup."); }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.RemoveAllListeners();
            if (GameManager.Instance != null && GameManager.Instance.LoadedGameData != null)
                sfxVolumeSlider.value = GameManager.Instance.LoadedGameData.musicVolume; // Annahme: SFX-Lautstärke ähnlich gespeichert
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        } else { Debug.LogWarning("[SettingsUI] sfxVolumeSlider ist null. Überspringe SFX-Lautstärke-Setup."); }
    }

    // Callback für Musiklautstärkeänderung
    private void OnMusicVolumeChanged(float volume)
    {
        if (GameManager.Instance != null && GameManager.Instance.LoadedGameData != null)
        {
            GameManager.Instance.LoadedGameData.musicVolume = volume;
            // GameManager.Instance.audioManager.SetMusicVolume(volume); // Auskommentieren, wenn AudioManager existiert
            GameManager.Instance.saveSystem.SaveData(GameManager.Instance.LoadedGameData);
        }
    }

    // Callback für SFX-Lautstärkeänderung
    private void OnSFXVolumeChanged(float volume)
    {
        if (GameManager.Instance != null && GameManager.Instance.LoadedGameData != null)
        {
            GameManager.Instance.LoadedGameData.musicVolume = volume; // Annahme: SFX-Lautstärke ist 'musicVolume' im aktuellen GameData
            // GameManager.Instance.audioManager.SetSoundEffectsVolume(volume); // Auskommentieren, wenn AudioManager existiert
            GameManager.Instance.saveSystem.SaveData(GameManager.Instance.LoadedGameData);
        }
    }

    // Einrichten der Gameplay-Einstellungen
    private void SetupGameplaySettings()
    {
        if (mouseSensitivitySlider != null)
        {
            mouseSensitivitySlider.onValueChanged.RemoveAllListeners();
            // Aus GameData laden, wenn Eigenschaft existiert: mouseSensitivitySlider.value = GameManager.Instance.LoadedGameData.mouseSensitivity;
            mouseSensitivitySlider.onValueChanged.AddListener(OnMouseSensitivityChanged);
        } else { Debug.LogWarning("[SettingsUI] mouseSensitivitySlider ist null. Überspringe Maus-Sensibilitäts-Setup."); }

        if (invertYAxisToggle != null)
        {
            invertYAxisToggle.onValueChanged.RemoveAllListeners();
            // Aus GameData laden, wenn Eigenschaft existiert: invertYAxisToggle.isOn = GameManager.Instance.LoadedGameData.invertYAxis;
            invertYAxisToggle.onValueChanged.AddListener(OnInvertYAxisToggled);
        } else { Debug.LogWarning("[SettingsUI] invertYAxisToggle ist null. Überspringe Y-Achsen-Invertierungs-Setup."); }
    }

    // Callback für Maus-Sensibilitätsänderung
    private void OnMouseSensitivityChanged(float sensitivity)
    {
        // In GameData speichern, wenn Eigenschaft existiert: GameManager.Instance.LoadedGameData.mouseSensitivity = sensitivity;
        // GameManager.Instance.saveSystem.SaveData(GameManager.Instance.LoadedGameData);
    }

    // Callback für Y-Achsen-Invertierung
    private void OnInvertYAxisToggled(bool inverted)
    {
        // In GameData speichern, wenn Eigenschaft existiert: GameManager.Instance.LoadedGameData.invertYAxis = inverted;
        // GameManager.Instance.saveSystem.SaveData(GameManager.Instance.LoadedGameData);
    }

    // Einrichten der Steuerungseinstellungen (Tastenbelegungen)
    private void SetupControlSettings()
    {
        if (keyBindingsParent == null || keyBindingEntryPrefab == null)
        {
            Debug.LogError("[SettingsUI] Tastenbelegungs-Parent oder Prefab ist null. Stellen Sie sicher, dass diese im Inspector zugewiesen oder von FindOrAssignUIReferences() gefunden wurden.");
            return;
        }

        // Bestehende Einträge löschen
        for (int i = keyBindingsParent.childCount - 1; i >= 0; i--)
        {
            Destroy(keyBindingsParent.GetChild(i).gameObject);
        }

        AddKeyBindingEntry("Jump", "jumpKey");
        AddKeyBindingEntry("Sprint", "sprintKey");
        AddKeyBindingEntry("Interact", "interactKey");
        AddKeyBindingEntry("Inventory", "inventoryKey");
        AddKeyBindingEntry("Menu", "menuKey");
        AddKeyBindingEntry("Forward", "forwardKey");
        AddKeyBindingEntry("Backward", "backwardKey");
        AddKeyBindingEntry("Left", "leftKey");
        AddKeyBindingEntry("Right", "rightKey");
        AddKeyBindingEntry("Status/Map", "tabKey");
        AddKeyBindingEntry("Questlog", "questLogKey");
        AddKeyBindingEntry("Chat", "chatKey");
    }

    // Fügt einen Tastenbelegungs-Eintrag hinzu
    private void AddKeyBindingEntry(string actionName, string keyBindingFieldName)
    {
        GameObject entryGO = Instantiate(keyBindingEntryPrefab, keyBindingsParent);
        TextMeshProUGUI actionText = entryGO.transform.Find("ActionText")?.GetComponent<TextMeshProUGUI>();
        Button keyButton = entryGO.transform.Find("KeyButton")?.GetComponent<Button>();
        TextMeshProUGUI keyText = keyButton?.GetComponentInChildren<TextMeshProUGUI>();

        if (actionText != null) actionText.text = LanguageManager.Instance.GetString($"keybinding_{keyBindingFieldName}", actionName);
        KeyCode currentKey = GetKeyFromInputManager(keyBindingFieldName);
        if (keyText != null) keyText.text = currentKey.ToString();
        if (keyButton != null)
        {
            keyButton.onClick.RemoveAllListeners();
            keyButton.onClick.AddListener(() => StartRebinding(keyButton, keyBindingFieldName));
        }
    }

    // Ruft den KeyCode vom InputManager ab
    private KeyCode GetKeyFromInputManager(string fieldName)
    {
        if (GameManager.Instance == null || GameManager.Instance.inputManager == null) return KeyCode.None;
        switch (fieldName)
        {
            case "jumpKey": return GameManager.Instance.inputManager.jumpKey;
            case "sprintKey": return GameManager.Instance.inputManager.sprintKey;
            case "interactKey": return GameManager.Instance.inputManager.interactKey;
            case "inventoryKey": return GameManager.Instance.inputManager.inventoryKey;
            case "menuKey": return GameManager.Instance.inputManager.menuKey;
            case "forwardKey": return GameManager.Instance.inputManager.forwardKey;
            case "backwardKey": return GameManager.Instance.inputManager.backwardKey;
            case "leftKey": return GameManager.Instance.inputManager.leftKey;
            case "rightKey": return GameManager.Instance.inputManager.rightKey;
            case "tabKey": return GameManager.Instance.inputManager.tabKey;
            case "questLogKey": return GameManager.Instance.inputManager.questLogKey;
            case "chatKey": return GameManager.Instance.inputManager.chatKey;
            default: return KeyCode.None;
        }
    }

    // Startet den Rebinding-Prozess
    public void StartRebinding(Button button, string keyBindingFieldName)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetPlayerControlActive(false);
        }

        _activeRebindButton = button;
        _rebindingKeyFieldName = keyBindingFieldName;
        _activeRebindButton.GetComponentInChildren<TextMeshProUGUI>().text = "Warte..."; 
        StartCoroutine(WaitForInputForKeybind());
        Debug.Log($"Warte auf neue Eingabe für {_rebindingKeyFieldName}");
    }

    // Wartet auf Benutzereingabe für die Tastenbelegung
    private IEnumerator WaitForInputForKeybind()
    {
        while (true)
        {
            yield return null;
            foreach (KeyCode keyCode in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(keyCode))
                {
                    SetKeyInInputManager(_rebindingKeyFieldName, keyCode);
                    _activeRebindButton.GetComponentInChildren<TextMeshProUGUI>().text = keyCode.ToString();
                    Debug.Log($"Tastenbelegung für {_rebindingKeyFieldName} geändert zu {keyCode}");
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.SetPlayerControlActive(true);
                        GameManager.Instance.inputManager.SaveKeyBindings();
                    }
                    _rebindingKeyFieldName = null;
                    _activeRebindButton = null;
                    yield break;
                }
            }
        }
    }

    // Setzt den KeyCode im InputManager
    private void SetKeyInInputManager(string fieldName, KeyCode newKey)
    {
        if (GameManager.Instance == null || GameManager.Instance.inputManager == null) return;
        switch (fieldName)
        {
            case "jumpKey": GameManager.Instance.inputManager.jumpKey = newKey; break;
            case "sprintKey": GameManager.Instance.inputManager.sprintKey = newKey; break;
            case "interactKey": GameManager.Instance.inputManager.interactKey = newKey; break;
            case "inventoryKey": GameManager.Instance.inputManager.inventoryKey = newKey; break;
            case "menuKey": GameManager.Instance.inputManager.menuKey = newKey; break;
            case "forwardKey": GameManager.Instance.inputManager.forwardKey = newKey; break;
            case "backwardKey": GameManager.Instance.inputManager.backwardKey = newKey; break;
            case "leftKey": GameManager.Instance.inputManager.leftKey = newKey; break;
            case "rightKey": GameManager.Instance.inputManager.rightKey = newKey; break;
            case "tabKey": GameManager.Instance.inputManager.tabKey = newKey; break;
            case "questLogKey": GameManager.Instance.inputManager.questLogKey = newKey; break;
            case "chatKey": GameManager.Instance.inputManager.chatKey = newKey; break;
        }
    }
}