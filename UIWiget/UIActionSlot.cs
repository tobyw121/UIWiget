// Dateiname: UIActionSlot.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace YourGame.UI.Widgets
{
    // Ist ein DropTarget, um Fähigkeiten/Items anzunehmen
    public class UIActionSlot : UIDropTarget
    {
        [Header("Action Slot Components")]
        [SerializeField] private Image _icon;
        [SerializeField] private Image _cooldownOverlay;
        [SerializeField] private TextMeshProUGUI _keybindText;
        [SerializeField] public KeyCode ActivationKey = KeyCode.None;

        // Hier würde die Referenz zur Fähigkeit/Item gespeichert
        // public AbilitySO AssignedAbility { get; private set; }

        private Coroutine _cooldownCoroutine;

        protected override void Awake()
        {
            base.Awake();
            if (_keybindText) _keybindText.text = ActivationKey.ToString().Replace("Alpha", "");
            if(_icon != null) _icon.enabled = false;
            if(_cooldownOverlay != null) _cooldownOverlay.fillAmount = 0;

            // Registriere die Aktivierung beim Input Handler
            if (ActivationKey != KeyCode.None && UIInputHandler.Instance != null)
            {
                UIInputHandler.Instance.RegisterToggleKey(ActivationKey, this);
            }
            
            // Erlaube auch Klick-Aktivierung
            OnClickEvent.AddListener((widget, data) => TriggerAction()); 
        }
        
        // Diese Methode wird aufgerufen, wenn etwas in den Slot gezogen wird
        public void Assign(Sprite newIcon /*, AbilitySO ability */)
        {
            // AssignedAbility = ability;
            _icon.sprite = newIcon;
            _icon.enabled = true;
        }

        public void StartCooldown(float duration)
        {
            if (_cooldownCoroutine != null) StopCoroutine(_cooldownCoroutine);
            _cooldownCoroutine = StartCoroutine(CooldownRoutine(duration));
        }

        private System.Collections.IEnumerator CooldownRoutine(float duration)
        {
            float timer = duration;
            _cooldownOverlay.fillAmount = 1;
            while (timer > 0)
            {
                timer -= Time.deltaTime;
                _cooldownOverlay.fillAmount = timer / duration;
                yield return null;
            }
            _cooldownOverlay.fillAmount = 0;
            _cooldownCoroutine = null;
        }

        // Wird vom UIInputHandler (via Toggle) oder per Klick aufgerufen
        public void TriggerAction()
        {
            // Beispiel-Bedingung: Nur auslösen, wenn ein Item/eine Fähigkeit zugewiesen ist
            if (_icon == null || !_icon.enabled)
            {
                Debug.Log($"Slot {name} ist leer. Aktion nicht ausgelöst.");
                return;
            }

            if (_cooldownCoroutine != null)
            {
                Debug.Log($"Aktion auf Slot {name} ist noch auf Cooldown.");
                return; // Aktion auf Cooldown
            }
            
            Debug.Log($"Aktion auf Slot {name} ausgelöst!");
            // Hier würde die Logik der Fähigkeit aufgerufen
            // AssignedAbility?.Use();

            // Beispiel: 5s Cooldown starten
            StartCooldown(5f); 
        }

        // Überschreibt die Toggle-Funktion, um TriggerAction aufzurufen.
        // Dies ist die KORREKTE Implementierung mit "override".
        public override void Toggle()
        {
            TriggerAction();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
             // Wichtig: Deregistriere den Key, wenn das Objekt deaktiviert wird
            if (ActivationKey != KeyCode.None && UIInputHandler.Instance != null)
            {
                UIInputHandler.Instance.UnregisterToggleKey(ActivationKey, this);
            }
        }
    }
}