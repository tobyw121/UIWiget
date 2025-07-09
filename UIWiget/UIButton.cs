// Dateiname: UIButton.cs
using UnityEngine;
using UnityEngine.EventSystems;
using YourGame.UI.Widgets;

// Ein einfacher Button, der auf Hover und Press reagiert
public class UIButton : UIWidget
{
    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        if (CurrentState == UIState.Interactive)
            transform.localScale = Vector3.one * 1.1f; // Beispiel-Effekt
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        transform.localScale = Vector3.one; // Effekt zurücksetzen
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        if (CurrentState == UIState.Interactive)
            transform.localScale = Vector3.one * 0.95f; // Beispiel-Effekt
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        if (CurrentState == UIState.Interactive && _isHovering) // Nur auf Hover-Größe zurück, wenn die Maus noch über dem Button ist
            transform.localScale = Vector3.one * 1.1f;
        else
            transform.localScale = Vector3.one;
    }
}