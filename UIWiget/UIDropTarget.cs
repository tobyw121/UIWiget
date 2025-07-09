// Dateiname: UIDropTarget.cs
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace YourGame.UI.Widgets
{
    public class UIDropTarget : UIWidget, IDropHandler
    {
        public UnityEvent<UIDraggable> OnItemDropped;

        public void OnDrop(PointerEventData eventData)
        {
            if (eventData.pointerDrag != null)
            {
                var draggable = eventData.pointerDrag.GetComponent<UIDraggable>();
                if (draggable != null)
                {
                    // Setze das gedraggte Objekt als Kind dieses DropTargets
                    draggable.transform.SetParent(transform, true);
                    draggable.transform.position = transform.position; // Zentriere es
                    
                    OnItemDropped?.Invoke(draggable);
                }
            }
        }
    }
}