// Dateiname: UIScrollList.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace YourGame.UI.Widgets
{
    [RequireComponent(typeof(ScrollRect))]
    public class UIScrollList : UIMenu // Erbt von UIMenu, um Grundfunktionen wiederzuverwenden
    {
        private readonly Queue<UIWidget> _pooledItems = new Queue<UIWidget>();
        private ScrollRect _scrollRect;

        protected override void Awake()
        {
            base.Awake();
            _scrollRect = GetComponent<ScrollRect>();
        }

        // Überschreibt die ClearItems-Methode, um Pooling zu implementieren
        public override void ClearItems()
        {
            while(Items.Count > 0)
            {
                var item = Items[0];
                Items.RemoveAt(0);
                item.gameObject.SetActive(false);
                _pooledItems.Enqueue(item);
            }
            SelectedWidget = null;
        }

        // Überschreibt AddWidget, um Objekte aus dem Pool wiederzuverwenden
        public override UIWidget AddWidget(string name, object userData = null)
        {
            UIWidget newWidget;
            if (_pooledItems.Count > 0)
            {
                newWidget = _pooledItems.Dequeue();
            }
            else
            {
                if (_itemTemplate == null) return null;
                newWidget = Instantiate(_itemTemplate, _contentContainer);
                 // Klick-Listener nur einmal beim Erstellen hinzufügen
                newWidget.OnClickEvent.AddListener((widget, data) => OnItemClick(widget));
            }

            newWidget.gameObject.name = name;
            newWidget.UserData = userData;
            newWidget.ParentMenu = this;
            newWidget.gameObject.SetActive(true);
            newWidget.transform.SetAsLastSibling(); // Stellt sicher, dass es am Ende der Liste ist
            Items.Add(newWidget);

            return newWidget;
        }
    }
}