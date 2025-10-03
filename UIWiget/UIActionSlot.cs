using UnityEngine;
using UnityEngine.UI;
using TMPro;
using YourGame.UI.Widgets;

// Annahme: Es existiert eine Datenklasse für Aktionen, die dem Slot zugewiesen werden.
// public class ActionData
// {
//     public string Name;
//     public string Description;
//     public Sprite Icon;
//     public float Cooldown;
//     public AudioClip Sound;
// }

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
        
        // Registriert den Shortcut-Key (z.B. '1', '2', '3') beim UIInputHandler.
        if (ActivationKey != KeyCode.None && UIInputHandler.Instance != null)
        {
            UIInputHandler.Instance.RegisterToggleKey(ActivationKey, this);
        }
        
        // Fügt einen Listener für Mausklicks hinzu.
        OnClickEvent.AddListener((widget, data) => TriggerAction());
    }
    
    /// <summary>
    /// Initialisiert den Slot beim Erstellen (z.B. beim Spielstart).
    /// </summary>
    public void Initialize()
    {
        if (_keybindText) _keybindText.text = ActivationKey.ToString().Replace("Alpha", "");
        if (_icon != null) _icon.enabled = false;
        if (_cooldownOverlay != null) _cooldownOverlay.fillAmount = 0;
        SetState(UIState.NotInteractive); // Ein leerer Slot ist nicht interaktiv.
    }

    /// <summary>
    /// Weist dem Slot eine Aktion zu und aktualisiert die Anzeige sowie den Tooltip.
    /// </summary>
    /// <param name="actionData">Die Daten der zuzuweisenden Aktion.</param>
    public void Assign(Sprite newIcon, string actionName, string actionDescription) // Ersetze dies durch deine ActionData-Klasse
    {
        if (_icon == null) return;
        _icon.sprite = newIcon;
        _icon.enabled = true;

        // VERBESSERUNG: Nutzt das eingebaute Tooltip-System von UIWidget.
        Tooltip.Enabled = true;
        Tooltip.TooltipText = $"<b>{actionName}</b>\n{actionDescription}";

        SetState(UIState.Interactive); // Slot ist jetzt benutzbar.
    }
    
    /// <summary>
    /// Leert den Slot und macht ihn wieder nicht-interaktiv.
    /// </summary>
    public void Clear()
    {
        if (_icon != null)
        {
             _icon.sprite = null;
             _icon.enabled = false;
        }
        Tooltip.Enabled = false;
        Tooltip.TooltipText = "";
        SetState(UIState.NotInteractive);
    }


    /// <summary>
    /// Startet den Cooldown für diesen Slot.
    /// </summary>
    /// <param name="duration">Die Dauer des Cooldowns in Sekunden.</param>
    public void StartCooldown(float duration)
    {
        if (_cooldownCoroutine != null) StopCoroutine(_cooldownCoroutine);
        _cooldownCoroutine = StartCoroutine(CooldownRoutine(duration));
    }

    private System.Collections.IEnumerator CooldownRoutine(float duration)
    {
        // VERBESSERUNG: Nutzt das Zustandssystem von UIWidget.
        // Der Slot ist während des Cooldowns "Disabled", was Klicks verhindert
        // und visuelles Feedback über das Theming-System ermöglicht.
        SetState(UIState.Disabled);
        
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

        // VERBESSERUNG: Setzt den Slot wieder auf "Interactive", wenn der Cooldown abgelaufen ist.
        SetState(UIState.Interactive);
    }

    /// <summary>
    /// Löst die Aktion dieses Slots aus.
    /// </summary>
    public void TriggerAction()
    {
        // VERBESSERUNG: Die Prüfung erfolgt nun über den allgemeinen Zustand des Widgets.
        // Dies deckt leere Slots, Cooldowns und andere Zustände ab.
        if (CurrentState != UIState.Interactive)
        {
            Debug.Log($"Aktion auf Slot {name} kann nicht ausgelöst werden (Zustand: {CurrentState}).");
            return;
        }
        
        // VERBESSERUNG: Nutzt das Sound-System von UIWidget für konsistentes Feedback.
        PlaySound(clickSound); 
        
        Debug.Log($"Aktion auf Slot {name} ausgelöst!");
        StartCooldown(5f); // Beispiel-Cooldown
    }

    /// <summary>
    /// Überschreibt die Toggle-Methode von UIWidget, die vom UIInputHandler per Tastendruck aufgerufen wird.
    /// </summary>
    public override void Toggle()
    {
        TriggerAction();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        // Stellt sicher, dass der Key-Listener sauber entfernt wird, um Fehler zu vermeiden.
        if (ActivationKey != KeyCode.None && UIInputHandler.Instance != null)
        {
            UIInputHandler.Instance.UnregisterToggleKey(ActivationKey, this);
        }
    }
}