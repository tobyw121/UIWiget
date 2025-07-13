// File name: UIDropTarget.cs (Corrected)
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace YourGame.UI.Widgets
{
    public class UIDropTarget : UIWidget, IDropHandler
    {
        public UnityEvent<UIDraggable> OnItemDropped;

        protected override void Awake()
        {
            base.Awake();
            if (OnItemDropped == null)
            {
                OnItemDropped = new UnityEvent<UIDraggable>();
            }
            IsDropTarget = true; // Set the widget as a drop target
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (CurrentState != UIState.Interactive) return; // Only allow drop if interactive

            if (eventData.pointerDrag != null)
            {
                var draggable = eventData.pointerDrag.GetComponent<UIDraggable>();
                if (draggable != null)
                {
                    OnItemDropped?.Invoke(draggable);
                    draggable.MarkDropSuccessful(); // Inform the draggable that the drop was successful
                }
            }
        }
    }
}