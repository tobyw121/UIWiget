using UnityEngine;
using UnityEngine.EventSystems;
using YourGame.UI.Widgets;

public class UIButton : UIWidget
{
    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        if (CurrentState == UIState.Interactive)
            transform.localScale = _originalScale * 1.1f;
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        transform.localScale = _originalScale;
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        if (CurrentState == UIState.Interactive)
            transform.localScale = _originalScale * 0.95f;
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        if (CurrentState == UIState.Interactive && isHovering)
            transform.localScale = _originalScale * 1.1f;
        else
            transform.localScale = _originalScale;
    }
}