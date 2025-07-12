// Dateiname: UIDraggable.cs
using UnityEngine;
using UnityEngine.EventSystems;

namespace YourGame.UI.Widgets
{
    public class UIDraggable : UIWidget
    {
        private Vector2 _offset;
        private Transform _originalParent;
        private Canvas _rootCanvas;

        protected override void Awake()
        {
            base.Awake();
            _rootCanvas = GetComponentInParent<Canvas>();
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            base.OnBeginDrag(eventData);
            _originalParent = transform.parent;
            // Verschiebe das Objekt auf die oberste Ebene des Canvas, damit es über allem gerendert wird
            transform.SetParent(_rootCanvas.transform, true);
            
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_rectTransform, eventData.position, eventData.pressEventCamera, out _offset);
            _canvasGroup.blocksRaycasts = false; // Erlaube Raycasts, das DropTarget unter dem Mauszeiger zu treffen
        }

        public override void OnDrag(PointerEventData eventData)
        {
            base.OnDrag(eventData);
            _rectTransform.anchoredPosition = eventData.position / _rootCanvas.scaleFactor - _offset;
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            base.OnEndDrag(eventData);
            _canvasGroup.blocksRaycasts = true;

            // Wenn es kein gültiges DropTarget gab, kehre zur ursprünglichen Position zurück.
            // UIDropTarget wird das Parenting selbst übernehmen, wenn der Drop erfolgreich war.
            if (transform.parent == _rootCanvas.transform)
            {
                transform.SetParent(_originalParent, true);
                // Optional: Tweene zurück zur Startposition
                TweenPosition(Vector2.zero, 0.1f, Easing.EaseType.EaseOutQuad);
            }
        }
    }
}