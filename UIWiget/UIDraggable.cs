// File name: UIDraggable.cs
using UnityEngine;
using UnityEngine.EventSystems;

namespace YourGame.UI.Widgets
{
    public class UIDraggable : UIWidget
    {
        private Vector2 _offset;
        private Transform _originalParent;
        private Canvas _rootCanvas;
        private bool _droppedSuccessfully = false; // Flag for successful drop

        protected override void Awake()
        {
            base.Awake();
            _rootCanvas = GetComponentInParent<Canvas>();
            IsDraggable = true; // Set the widget as draggable
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            base.OnBeginDrag(eventData);
            if (CurrentState == UIState.Interactive) // Only draggable if interactive
            {
                _originalParent = transform.parent;
                // Move the object to the top level of the Canvas so it renders above everything
                transform.SetParent(_rootCanvas.transform, true);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(_rectTransform, eventData.position, eventData.pressEventCamera, out _offset);
                _canvasGroup.blocksRaycasts = false; // Allow raycasts to hit the DropTarget under the pointer
                _droppedSuccessfully = false; // Reset at the start of the drag
            }
        }

        public override void OnDrag(PointerEventData eventData)
        {
            base.OnDrag(eventData);
            if (CurrentState == UIState.Interactive) // Only draggable if interactive
            {
                _rectTransform.anchoredPosition = eventData.position / _rootCanvas.scaleFactor - _offset;
            }
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            base.OnEndDrag(eventData);
            if (CurrentState == UIState.Interactive) // Only draggable if interactive
            {
                _canvasGroup.blocksRaycasts = true;

                // Check if a valid DropTarget was hit
                if (eventData.pointerCurrentRaycast.gameObject != null)
                {
                    UIDropTarget dropTarget = eventData.pointerCurrentRaycast.gameObject.GetComponentInParent<UIDropTarget>();
                    if (dropTarget != null && dropTarget.IsDropTarget && dropTarget.CurrentState == UIState.Interactive)
                    {
                        // The OnDrop handler of the DropTarget will handle parenting.
                        _droppedSuccessfully = true;
                    }
                }

                // If no valid DropTarget accepted the drop, revert to the original position.
                // We check _droppedSuccessfully because the DropTarget will handle SetParent.
                if (!_droppedSuccessfully)
                {
                    transform.SetParent(_originalParent, true);
                    // Optional: Tween back to the start position
                    TweenPosition(_originalPosition, 0.1f, Easing.EaseType.EaseOutQuad);
                    OnDragCancelled?.Invoke(); // Trigger event if drag is cancelled
                }
            }
        }
        
        // NEW: Method to mark the drop as successful
        public void MarkDropSuccessful()
        {
            _droppedSuccessfully = true;
        }
    }
}