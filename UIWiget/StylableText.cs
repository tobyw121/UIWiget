using UnityEngine;
using TMPro;

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
            
            textComponent.color = style.Normal.color; 
            textComponent.fontStyle = style.fontStyle; 
            textComponent.characterSpacing = style.characterSpacing;
            textComponent.wordSpacing = style.wordSpacing;
            textComponent.lineSpacing = style.lineSpacing;
        }
    }
}