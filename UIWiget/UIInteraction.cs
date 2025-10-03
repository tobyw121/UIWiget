using UnityEngine;
using UnityEngine.EventSystems;
using YourGame.UI.Widgets;

public class UIInteraction : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private UIWidget _widgetController;

    void Awake()
    {
        _widgetController = GetComponent<UIWidget>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _widgetController.isHovering = true;
        _widgetController.RaiseGlobalPointerEnter();
        if (_widgetController.CurrentState == UIWidget.UIState.Interactive)
        {
            _widgetController.OnHoverEvent?.Invoke(_widgetController, true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _widgetController.isHovering = false;
        _widgetController.RaiseGlobalPointerExit();
        if (_widgetController.CurrentState == UIWidget.UIState.Interactive)
        {
            _widgetController.OnHoverEvent?.Invoke(_widgetController, false);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_widgetController.CurrentState == UIWidget.UIState.Interactive)
        {
            _widgetController.isPressed = true;
            _widgetController.OnPressEvent?.Invoke(_widgetController, true);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (_widgetController.CurrentState == UIWidget.UIState.Interactive)
        {
            _widgetController.isPressed = false;
            _widgetController.OnPressEvent?.Invoke(_widgetController, false);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_widgetController.CurrentState == UIWidget.UIState.Interactive)
        {
            _widgetController.OnClickEvent?.Invoke(_widgetController, eventData);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!_widgetController.IsDraggable || _widgetController.CurrentState != UIWidget.UIState.Interactive) return;
        _widgetController.isDragging = true;
        _widgetController.OnDragStartEvent?.Invoke(_widgetController, eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_widgetController.isDragging) return;
        _widgetController.OnDragEvent?.Invoke(_widgetController, eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!_widgetController.isDragging) return;
        _widgetController.isDragging = false;
        _widgetController.OnDragEndEvent?.Invoke(_widgetController, eventData);
    }
}