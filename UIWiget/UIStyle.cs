// Dateiname: UIStyle.cs (Final verbessert)
using UnityEngine;
using TMPro;
using YourGame.UI.Widgets;

/// <summary>
/// Definiert die visuellen Eigenschaften für einen bestimmten Zustand einer UI-Grafik.
/// </summary>
[System.Serializable]
public class StyleState
{
    [Tooltip("Die Farbe der Hauptgrafik für diesen Zustand.")]
    public Color color = Color.white;

    [Tooltip("Das Sprite, das für diesen Zustand verwendet wird (optional).")]
    public Sprite sprite;
}

[System.Serializable]
public class UIStyle
{
    [Tooltip("Eindeutiger Schlüssel für diesen Stil.")]
    public string styleKey;

    [Header("Graphic States (Background)")]
    public StyleState Normal = new StyleState();
    public StyleState Hover = new StyleState();
    public StyleState Pressed = new StyleState();
    public StyleState Disabled = new StyleState();
    
    [Tooltip("Dauer für den Farbübergang der Hauptgrafik in Sekunden.")]
    public float graphicFadeDuration = 0.1f;

    // --- NEU: Separater Block für Textfarben ---
    [Header("Text Color States")]
    [Tooltip("Definiert die Textfarben für die verschiedenen Interaktionszustände.")]
    public UIWidget.ColorTintBlock textColorTints = new UIWidget.ColorTintBlock();

    [Header("General Font Settings")]
    [Tooltip("Die Schriftart, die für diesen Stil generell gilt.")]
    public TMP_FontAsset font;
    [Tooltip("Der allgemeine Schriftstil (z.B. Normal, Fett, Kursiv).")]
    public FontStyles fontStyle = FontStyles.Normal;
    [Tooltip("Der Abstand zwischen den einzelnen Zeichen.")]
    public float characterSpacing = 0f;
    [Tooltip("Der Abstand zwischen den Wörtern.")]
    public float wordSpacing = 0f;
    [Tooltip("Der Abstand zwischen den Zeilen.")]
    public float lineSpacing = 0f;

    [Header("Animation & Transitions")]
    public float animationDuration = 0.25f;
    public Easing.EaseType animationEaseType = Easing.EaseType.EaseOutQuad;
}