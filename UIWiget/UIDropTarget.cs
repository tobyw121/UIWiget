// Dateiname: UIDropTarget.cs (Korrigiert)
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace YourGame.UI.Widgets
{
    public class UIDropTarget : UIWidget, IDropHandler
    {
        public UnityEvent<UIDraggable> OnItemDropped;

        // KORREKTUR: Wir fügen eine Awake-Methode hinzu, um das Event zu initialisieren.
        protected override void Awake()
        {
            base.Awake();
            if (OnItemDropped == null)
            {
                OnItemDropped = new UnityEvent<UIDraggable>();
            }
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (eventData.pointerDrag != null)
            {
                var draggable = eventData.pointerDrag.GetComponent<UIDraggable>();
                if (draggable != null)
                {
                    // Das Event wird hier sicher aufgerufen
                    OnItemDropped?.Invoke(draggable);
                    
                    // Die Logik, das Item zum Kind zu machen, sollte vom Listener gesteuert werden,
                    // nicht vom DropTarget selbst, um es flexibler zu machen.
                    // Beispiel: draggable.transform.SetParent(transform, true);
                }
            }
        }
    }
}