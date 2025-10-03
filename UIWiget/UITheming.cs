using UnityEngine;
using UnityEngine.UI;
using TMPro;
using YourGame.UI.Widgets;

public class UITheming : MonoBehaviour
{
    private UIWidget _widgetController;
    private Graphic _targetGraphic;
    private TMP_Text _textComponent;

    void Awake()
    {
        _widgetController = GetComponent<UIWidget>();
        _targetGraphic = GetComponent<Graphic>();
        if (_targetGraphic == null) _targetGraphic = GetComponentInChildren<Graphic>();
        _textComponent = GetComponentInChildren<TextMeshProUGUI>(true);
    }

    void OnEnable()
    {
        UIThemeManager.OnThemeChanged += ApplyThemeStyle;
        ApplyThemeStyle();
    }

    void OnDisable()
    {
        UIThemeManager.OnThemeChanged -= ApplyThemeStyle;
    }

    public void ApplyThemeStyle()
    {
        if (_widgetController.appliedStyle == null) return;
        DoStateTransition(true);
    }

    public void DoStateTransition(bool instant)
    {
        var style = _widgetController.appliedStyle;
        if (style == null || _targetGraphic == null) return;

        StyleState targetState = style.Normal; // Standard-Fallback

        // Logik erweitert, um alle UIStates zu berücksichtigen
        switch (_widgetController.CurrentState)
        {
            case UIWidget.UIState.Disabled:
                targetState = style.Disabled;
                break;
            case UIWidget.UIState.NotInteractive:
                targetState = style.NotInteractive; // NEUE PRÜFUNG
                break;
            case UIWidget.UIState.Interactive:
                if (_widgetController.isPressed) targetState = style.Pressed;
                else if (_widgetController.isHovering) targetState = style.Hover;
                else targetState = style.Normal;
                break;
        }

        if (_targetGraphic is Image image && targetState.sprite != null) image.sprite = targetState.sprite;
        
        if (instant)
        {
            _targetGraphic.color = targetState.color;
        }
        else
        {
            UITweener.Instance.AddTween(c => _targetGraphic.color = c, _targetGraphic.color, targetState.color, style.graphicFadeDuration, Easing.EaseType.Linear);
        }

        if (_textComponent != null)
        {
            Color targetTextColor = style.textColorTints.normalColor;
            
            switch (_widgetController.CurrentState)
            {
                case UIWidget.UIState.Disabled:
                    targetTextColor = style.textColorTints.disabledColor;
                    break;
                case UIWidget.UIState.NotInteractive:
                    // Optional: Eigene Textfarbe für nicht-interaktive Zustände hinzufügen
                    targetTextColor = style.textColorTints.disabledColor; 
                    break;
                case UIWidget.UIState.Interactive:
                    if (_widgetController.isPressed) targetTextColor = style.textColorTints.pressedColor;
                    else if (_widgetController.isHovering) targetTextColor = style.textColorTints.hoverColor;
                    else targetTextColor = style.textColorTints.normalColor;
                    break;
            }
            _textComponent.color = targetTextColor;
        }
    }
}