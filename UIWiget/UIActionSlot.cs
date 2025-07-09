using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace YourGame.UI.Widgets
{
    public class UIActionSlot : UIDropTarget
    {
        [Header("Action Slot Components")]
        [SerializeField] private Image _icon;
        [SerializeField] private Image _cooldownOverlay;
        [SerializeField] private TextMeshProUGUI _keybindText;
        [SerializeField] public KeyCode ActivationKey = KeyCode.None;

        private Coroutine _cooldownCoroutine;

        protected override void Awake()
        {
            base.Awake();
            
            if (ActivationKey != KeyCode.None && UIInputHandler.Instance != null)
            {
                UIInputHandler.Instance.RegisterToggleKey(ActivationKey, this);
            }
            
            OnClickEvent.AddListener((widget, data) => TriggerAction());
        }
        
        public void Initialize()
        {
            if (_keybindText) _keybindText.text = ActivationKey.ToString().Replace("Alpha", "");
            if (_icon != null) _icon.enabled = false;
            if (_cooldownOverlay != null) _cooldownOverlay.fillAmount = 0;
        }

        public void Assign(Sprite newIcon)
        {
            if (_icon == null) return;
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
            if (_cooldownOverlay != null) _cooldownOverlay.fillAmount = 1;
            
            while (timer > 0)
            {
                timer -= Time.deltaTime;
                if (_cooldownOverlay != null) _cooldownOverlay.fillAmount = timer / duration;
                yield return null;
            }
            
            if (_cooldownOverlay != null) _cooldownOverlay.fillAmount = 0;
            _cooldownCoroutine = null;
        }

        public void TriggerAction()
        {
            if (_icon == null || !_icon.enabled)
            {
                Debug.Log($"Slot {name} ist leer. Aktion nicht ausgelöst.");
                return;
            }

            if (_cooldownCoroutine != null)
            {
                Debug.Log($"Aktion auf Slot {name} ist noch auf Cooldown.");
                return;
            }
            
            Debug.Log($"Aktion auf Slot {name} ausgelöst!");
            StartCooldown(5f);
        }

        public override void Toggle()
        {
            TriggerAction();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (ActivationKey != KeyCode.None && UIInputHandler.Instance != null)
            {
                UIInputHandler.Instance.UnregisterToggleKey(ActivationKey, this);
            }
        }
    }
}