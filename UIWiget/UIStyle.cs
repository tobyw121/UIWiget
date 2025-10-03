using UnityEngine;
using TMPro;
using YourGame.UI.Widgets;
using System;

[Serializable]
public class UIStyle
{
    [Tooltip("Eindeutiger Schlüssel für diesen Stil.")]
    public string styleKey;

    [Header("Graphic States (Background)")]
    public StyleState Normal = new StyleState();
    public StyleState Hover = new StyleState();
    public StyleState Pressed = new StyleState();
    public StyleState Disabled = new StyleState();
    
    // HINZUGEFÜGT: Ein neuer Zustand für leere oder nicht-interaktive Slots.
    public StyleState NotInteractive = new StyleState();
    
    [Tooltip("Dauer für den Farbübergang der Hauptgrafik in Sekunden.")]
    public float graphicFadeDuration = 0.1f;

    [Header("Text Color States")]
    [Tooltip("Definiert die Textfarben für die verschiedenen Interaktionszustände.")]
    public UIWidget.ColorTintBlock textColorTints = new UIWidget.ColorTintBlock();

    [Header("General Font Settings")]
    public TMP_FontAsset font;
    public FontStyles fontStyle = FontStyles.Normal;
    public float characterSpacing = 0f;
    public float wordSpacing = 0f;
    public float lineSpacing = 0f;

    [Header("Content Localization")]
    [Tooltip("Der Lokalisierungs-Schlüssel für den Tooltip-Text, der diesem Stil zugeordnet ist.")]
    public string tooltipLocalizationKey;

    [Header("Animation & Transitions")]
    public float animationDuration = 0.25f;
    public Easing.EaseType animationEaseType = Easing.EaseType.EaseOutQuad;
    
    [Tooltip("The type of visual transitions to use for Show/Hide.")]
    public UIWidget.VisualTransition transition = UIWidget.VisualTransition.Fade;

    [Tooltip("Settings for the slide transition, if enabled.")]
    public UIWidget.SlideTransition slideTransition = new UIWidget.SlideTransition();
}