// Dateiname: StylableText.cs (Korrigiert)
using UnityEngine;
using TMPro;

/// <summary>
/// Eine einfache Komponente, die an ein GameObject mit einer TextMeshProUGUI-Komponente
/// angehängt wird, um ihr einen spezifischen UIStyle über einen Key zuzuweisen.
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class StylableText : MonoBehaviour
{
    [Tooltip("Der Style Key aus dem UIThemeData, der auf dieses Textelement angewendet werden soll.")]
    public string styleKey;

    public void ApplyStyle(UIStyle style)
    {
        if (style == null) return;

        var textComponent = GetComponent<TextMeshProUGUI>();
        if (textComponent != null)
        {
            if (style.font != null)
            {
                textComponent.font = style.font;
            }
            
            // Greift auf die Farbe des "Normal"-Zustands zu.
            textComponent.color = style.Normal.color; 
            
            // KORREKTUR: Greift direkt auf den Schriftstil des Haupt-Stils zu.
            textComponent.fontStyle = style.fontStyle; 

            textComponent.characterSpacing = style.characterSpacing;
            textComponent.wordSpacing = style.wordSpacing;
            textComponent.lineSpacing = style.lineSpacing;
        }
    }
}