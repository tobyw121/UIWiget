// Dateiname: UIInventorySlot.cs
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

namespace YourGame.UI.Widgets
{
    // Ein Inventar-Slot kann sowohl ein Drop-Ziel als auch der Container für ein ziehbares Item sein.
    public class UIInventorySlot : UIDropTarget
    {
        [Header("Inventory Slot")]
        [SerializeField] private UIDraggable _itemInSlotPrefab; // Prefab des Items, das gezogen werden kann
        [SerializeField] private TextMeshProUGUI _stackSizeText;
        private UIDraggable _currentItem;

        // Wird aufgerufen, wenn ein anderes Item auf diesen Slot gezogen wird
        public void OnDrop(PointerEventData eventData)
        {
            var droppedItem = eventData.pointerDrag.GetComponent<UIDraggable>();
            if (droppedItem == null) return;
            
            var sourceSlot = droppedItem.GetComponentInParent<UIInventorySlot>();
            if (sourceSlot != null && sourceSlot != this)
            {
                // Tausche die Items zwischen diesem und dem Quell-Slot
                SwapItems(sourceSlot);
            }
        }

        // Füllt den Slot mit einem Item (z.B. beim Öffnen des Inventars)
        public void SetItem(Sprite icon, int stackSize)
        {
            if (_currentItem == null)
            {
                _currentItem = Instantiate(_itemInSlotPrefab, transform);
            }
            _currentItem.GetComponent<Image>().sprite = icon;
            _stackSizeText.text = stackSize > 1 ? stackSize.ToString() : "";
            _currentItem.gameObject.SetActive(true);
        }

        public void ClearSlot()
        {
            if (_currentItem != null)
            {
                Destroy(_currentItem.gameObject);
                _currentItem = null;
            }
            _stackSizeText.text = "";
        }

        // Tauscht den Inhalt mit einem anderen Slot
        public void SwapItems(UIInventorySlot otherSlot)
        {
            var thisItem = this._currentItem;
            var otherItem = otherSlot._currentItem;

            if (thisItem != null) thisItem.transform.SetParent(otherSlot.transform, false);
            if (otherItem != null) otherItem.transform.SetParent(this.transform, false);

            otherSlot._currentItem = thisItem;
            this._currentItem = otherItem;

            // Positionen zurücksetzen
            if (otherSlot._currentItem) otherSlot._currentItem.RectTransform.anchoredPosition = Vector2.zero;
            if (this._currentItem) this._currentItem.RectTransform.anchoredPosition = Vector2.zero;
            
            // Stack-Texte tauschen
            var tempText = _stackSizeText.text;
            _stackSizeText.text = otherSlot._stackSizeText.text;
            otherSlot._stackSizeText.text = tempText;
        }
    }
}