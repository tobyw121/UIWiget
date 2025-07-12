// Dateiname: SettingsUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using YourGame.UI.Widgets;
using System.Collections.Generic;
using System.Linq; // Added for LINQ
using System.Collections; // Added for Coroutines

namespace YourGame.UI // Assuming YourGame.UI is your main UI namespace
{
    public class SettingsUI : UIWidget
    {
        [Header("Language Settings")]
        public TMP_Dropdown languageDropdown;

        [Header("Volume Settings")]
        public Slider musicVolumeSlider;
        public Slider soundEffectsVolumeSlider;

        [Header("Graphics Settings")]
        public TMP_Dropdown graphicsQualityDropdown;

        [Header("Keybinding Settings")]
        public GameObject keybindsContainer; // Parent for all keybinding rows
        public GameObject keybindRowPrefab; // Prefab for a single keybind row (Button + Text)
        public Button applyKeybindsButton;
        public Button resetButton;

        // Internal: Mapping from button name to KeyCode (for rebinding)
        private Dictionary<string, KeyCode> _currentKeyBindings = new Dictionary<string, KeyCode>();
        private string _keyToRebind = null; // Stores the name of the key currently being rebound
        private TextMeshProUGUI _currentKeybindText; // Reference to the TextMeshProUGUI of the key being rebound

        [Header("Action Buttons")]
        public Button saveAndCloseButton;
        public Button closeButton;

        protected override void Awake()
        {
            base.Awake(); // Initialize UIWidget properties

            // Initialize dropdowns
            InitializeLanguageDropdown();
            InitializeGraphicsDropdown();

            // Add listeners
            languageDropdown?.onValueChanged.AddListener(SetLanguage);
            musicVolumeSlider?.onValueChanged.AddListener(SetMusicVolume);
            soundEffectsVolumeSlider?.onValueChanged.AddListener(SetSoundEffectsVolume);
            graphicsQualityDropdown?.onValueChanged.AddListener(SetGraphicsQuality);

            applyKeybindsButton?.onClick.AddListener(ApplyKeybindings);
            resetButton?.onClick.AddListener(ResetToDefaultKeybindings);
            saveAndCloseButton?.onClick.AddListener(SaveAndCloseSettings);
            closeButton?.onClick.AddListener(CloseSettings);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            LoadSettings(); // Load current settings when panel becomes active
            PopulateKeybindsUI(); // Populate keybinding UI when panel becomes active
        }

        private void InitializeLanguageDropdown()
        {
            languageDropdown.ClearOptions();
            // In a real game, you'd get these from LanguageManager
            // For this example, hardcode some languages
            List<string> languages = new List<string> { "English", "Deutsch", "Español" };
            languageDropdown.AddOptions(languages);
            // Set initial value based on current language (e.g., from LanguageManager)
            // languageDropdown.value = languages.IndexOf(LanguageManager.Instance.GetCurrentLanguageName());
        }

        private void InitializeGraphicsDropdown()
        {
            graphicsQualityDropdown.ClearOptions();
            List<string> qualityNames = QualitySettings.names.ToList();
            graphicsQualityDropdown.AddOptions(qualityNames);
            graphicsQualityDropdown.value = QualitySettings.GetQualityLevel();
        }

        private void PopulateKeybindsUI()
        {
            // Clear existing keybind rows
            foreach (Transform child in keybindsContainer.transform)
            {
                Destroy(child.gameObject);
            }

            // Get current keybindings from InputManager (assuming it exists and has public properties)
            if (GameManager.Instance?.inputManager != null)
            {
                InputManager inputManager = GameManager.Instance.inputManager;
                _currentKeyBindings = new Dictionary<string, KeyCode>
                {
                    { "Jump", inputManager.jumpKey },
                    { "Sprint", inputManager.sprintKey },
                    { "Interact", inputManager.interactKey },
                    { "Menu", inputManager.menuKey },
                    { "Forward", inputManager.forwardKey },
                    { "Backward", inputManager.backwardKey },
                    { "Left", inputManager.leftKey },
                    { "Right", inputManager.rightKey },
                    { "Inventory", inputManager.inventoryKey },
                    { "Chat", inputManager.chatKey },
                    { "QuestLog", inputManager.questLogKey }
                };

                foreach (var kvp in _currentKeyBindings)
                {
                    GameObject row = Instantiate(keybindRowPrefab, keybindsContainer.transform);
                    TextMeshProUGUI actionText = row.transform.Find("ActionText")?.GetComponent<TextMeshProUGUI>();
                    TextMeshProUGUI keyText = row.transform.Find("KeyText")?.GetComponent<TextMeshProUGUI>();
                    Button rebindButton = row.GetComponent<Button>();

                    if (actionText != null) actionText.text = kvp.Key;
                    if (keyText != null) keyText.text = kvp.Value.ToString();
                    
                    if (rebindButton != null)
                    {
                        string actionName = kvp.Key; // Capture for lambda
                        rebindButton.onClick.AddListener(() => StartRebind(actionName, keyText));
                    }
                }
            }
            else
            {
                Debug.LogWarning("[SettingsUI] InputManager not found. Keybinding UI cannot be populated.");
            }
        }

        private void LoadSettings()
        {
            // Load language setting (example, assuming LanguageManager stores/retrieves current language)
            // languageDropdown.value = LanguageManager.Instance.GetLanguageIndex();

            // Load volume settings (example, assuming AudioManager stores/retrieves volumes)
            if (GameManager.Instance?.LoadedGameData != null)
            {
                musicVolumeSlider.value = GameManager.Instance.LoadedGameData.musicVolume;
                // Add sound effects volume if stored in GameData
            }
            else
            {
                Debug.LogWarning("[SettingsUI] LoadedGameData is null. Cannot load volume settings.");
            }
            
            // Load graphics quality
            graphicsQualityDropdown.value = QualitySettings.GetQualityLevel();
        }

        private void SetLanguage(int index)
        {
            string selectedLanguage = languageDropdown.options[index].text;
            Debug.Log($"[SettingsUI] Setting language to: {selectedLanguage}");
            // Replace with actual LanguageManager call
            // LanguageManager.Instance?.SetLanguage(selectedLanguageCode); 
        }

        private void SetMusicVolume(float volume)
        {
            Debug.Log($"[SettingsUI] Setting music volume to: {volume}");
            if (GameManager.Instance?.LoadedGameData != null)
            {
                GameManager.Instance.LoadedGameData.musicVolume = volume;
                // AudioManager.Instance?.SetMusicVolume(volume); // Call actual AudioManager
            }
        }

        private void SetSoundEffectsVolume(float volume)
        {
            Debug.Log($"[SettingsUI] Setting sound effects volume to: {volume}");
            // Add to GameData and call AudioManager if available
            // AudioManager.Instance?.SetSoundEffectsVolume(volume);
        }

        private void SetGraphicsQuality(int index)
        {
            QualitySettings.SetQualityLevel(index, true); // true to apply immediately
            Debug.Log($"[SettingsUI] Setting graphics quality to: {QualitySettings.names[index]}");
        }

        private void StartRebind(string keyName, TextMeshProUGUI keyText)
        {
            if (_keyToRebind != null) return; // Already rebinding a key

            _keyToRebind = keyName;
            _currentKeybindText = keyText;
            _currentKeybindText.text = "Press any key...";
            Debug.Log($"[SettingsUI] Rebinding '{keyName}'. Press any key...");
            StartCoroutine(WaitForInput());
        }

        private IEnumerator WaitForInput()
        {
            while (!Input.anyKeyDown)
            {
                yield return null;
            }

            foreach (KeyCode kc in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(kc))
                {
                    _currentKeyBindings[_keyToRebind] = kc;
                    _currentKeybindText.text = kc.ToString();
                    Debug.Log($"[SettingsUI] Rebound '{_keyToRebind}' to '{kc}'.");
                    _keyToRebind = null;
                    _currentKeybindText = null;
                    yield break;
                }
            }
            // Fallback if no key was detected (shouldn't happen with Input.anyKeyDown)
            Debug.LogWarning("[SettingsUI] No key detected during rebind. Rebind cancelled.");
            _currentKeybindText.text = _currentKeyBindings[_keyToRebind].ToString(); // Revert text
            _keyToRebind = null;
            _currentKeybindText = null;
        }

        private void ApplyKeybindings()
        {
            if (GameManager.Instance?.inputManager != null)
            {
                InputManager inputManager = GameManager.Instance.inputManager;
                inputManager.jumpKey = _currentKeyBindings["Jump"];
                inputManager.sprintKey = _currentKeyBindings["Sprint"];
                inputManager.interactKey = _currentKeyBindings["Interact"];
                inputManager.menuKey = _currentKeyBindings["Menu"];
                inputManager.forwardKey = _currentKeyBindings["Forward"];
                inputManager.backwardKey = _currentKeyBindings["Backward"];
                inputManager.leftKey = _currentKeyBindings["Left"];
                inputManager.rightKey = _currentKeyBindings["Right"];
                inputManager.inventoryKey = _currentKeyBindings["Inventory"];
                inputManager.chatKey = _currentKeyBindings["Chat"];
                inputManager.questLogKey = _currentKeyBindings["QuestLog"];

                inputManager.SaveKeyBindings();
                Debug.Log("[SettingsUI] Keybindings applied and saved.");
            }
            else
            {
                Debug.LogError("[SettingsUI] InputManager not found. Cannot apply keybindings.");
            }
        }

        private void ResetToDefaultKeybindings()
        {
            // Reset _currentKeyBindings to Unity's default or your game's initial defaults
            _currentKeyBindings["Jump"] = KeyCode.Space;
            _currentKeyBindings["Sprint"] = KeyCode.LeftShift;
            _currentKeyBindings["Interact"] = KeyCode.E;
            _currentKeyBindings["Menu"] = KeyCode.Escape;
            _currentKeyBindings["Forward"] = KeyCode.W;
            _currentKeyBindings["Backward"] = KeyCode.S;
            _currentKeyBindings["Left"] = KeyCode.A;
            _currentKeyBindings["Right"] = KeyCode.D;
            _currentKeyBindings["Inventory"] = KeyCode.I;
            _currentKeyBindings["Chat"] = KeyCode.C;
            _currentKeyBindings["QuestLog"] = KeyCode.Q;

            PopulateKeybindsUI(); // Refresh UI with default values
            ApplyKeybindings(); // Apply and save the default keybindings
            Debug.Log("[SettingsUI] Keybindings reset to default.");
        }

        private void SaveAndCloseSettings()
        {
            ApplyKeybindings(); // Ensure keybindings are saved
            // Save other settings if not already saved by their listeners (e.g., volume)
            if (GameManager.Instance?.LoadedGameData != null && GameManager.Instance?.saveSystem != null)
            {
                GameManager.Instance.saveSystem.SaveData(GameManager.Instance.LoadedGameData);
                Debug.Log("[SettingsUI] All settings saved.");
            }
            CloseSettings();
        }

        private void CloseSettings()
        {
            UIManager.Instance?.CloseCurrentPanel();
            Debug.Log("[SettingsUI] Settings UI closed.");
        }
    }
}