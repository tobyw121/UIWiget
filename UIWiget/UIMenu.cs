// Dateiname: UIMenu.cs (Korrigiert)
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace YourGame.UI.Widgets
{
    public class UIMenu : UIWidget
    {
        [Header("Menu Settings")]
        [Tooltip("Das Template-Prefab für jedes Item in der Liste.")]
        // ÄNDERUNG: private -> protected
        [SerializeField] protected UIWidget _itemTemplate; 

        [Tooltip("Das UI-Element, das als Container für die Items dient (z.B. mit einem LayoutGroup).")]
        // ÄNDERUNG: private -> protected
        [SerializeField] protected RectTransform _contentContainer; 

        [Tooltip("Optional: Ein Highlight-Objekt, das über das ausgewählte Item gelegt wird.")]
        [SerializeField] private GameObject _selectionHighlight;

        private readonly List<UIWidget> _items = new List<UIWidget>();
        private UIWidget _selectedWidget;

        public List<UIWidget> Items => _items;
        public UnityEvent<UIWidget> OnItemSelected;
        public UIWidget SelectedWidget
        {
            get => _selectedWidget;
            set
            {
                if (_selectedWidget == value) return;
                _selectedWidget = value;
                UpdateHighlight();
                OnItemSelected?.Invoke(_selectedWidget);
            }
        }

        protected override void Awake()
        {
            base.Awake();
            if (_itemTemplate != null) _itemTemplate.gameObject.SetActive(false);
            if (_selectionHighlight != null) _selectionHighlight.SetActive(false);
            if (_contentContainer == null) _contentContainer = transform as RectTransform;
        }
        
        public virtual UIWidget AddWidget(string name, object userData = null)
        {
            if (_itemTemplate == null)
            {
                Debug.LogError("[UIMenu] Kein Item-Template zugewiesen!");
                return null;
            }

            UIWidget newWidget = Instantiate(_itemTemplate, _contentContainer);
            newWidget.gameObject.name = name;
            newWidget.UserData = userData;
            newWidget.ParentMenu = this;
            newWidget.gameObject.SetActive(true);
            newWidget.OnClickEvent.AddListener((widget, data) => OnItemClick(widget));
            _items.Add(newWidget);
            return newWidget;
        }
        
        public virtual void RemoveWidget(UIWidget widget)
        {
            if (widget == null || !_items.Contains(widget)) return;
            if (SelectedWidget == widget)
            {
                SelectedWidget = null;
            }
            _items.Remove(widget);
            Destroy(widget.gameObject);
        }
        
        public virtual void ClearItems()
        {
            for (int i = _items.Count - 1; i >= 0; i--)
            {
                Destroy(_items[i].gameObject);
            }
            _items.Clear();
            SelectedWidget = null;
        }

        protected virtual void OnItemClick(UIWidget item)
        {
            SelectedWidget = item;
        }

        private void UpdateHighlight()
        {
            if (_selectionHighlight == null) return;
            if (_selectedWidget != null)
            {
                _selectionHighlight.SetActive(true);
                _selectionHighlight.transform.SetParent(_selectedWidget.transform, false);
                _selectionHighlight.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            }
            else
            {
                _selectionHighlight.SetActive(false);
                _selectionHighlight.transform.SetParent(transform, false);
            }
        }
    }
}