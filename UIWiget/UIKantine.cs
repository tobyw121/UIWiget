// Dateiname: UIKantine.cs (Korrigiert)
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace YourGame.UI.Widgets
{
    public class UIKantine : UIWidget
    {
        private List<UIInventorySlot> _ingredientSlots;
        private TextMeshProUGUI _resultBuffsText;
        private UIButton _cookButton;
        private UIScrollList _ingredientSelectionList;

        public void Initialize(List<UIInventorySlot> slots, TextMeshProUGUI resultText, UIButton cookButton, UIScrollList selectionList)
        {
            _ingredientSlots = slots;
            _resultBuffsText = resultText;
            _cookButton = cookButton;
            _ingredientSelectionList = selectionList;

            foreach (var slot in _ingredientSlots)
            {
                slot.OnItemDropped.AddListener((draggable) => UpdateResultingBuffs());
            }

            _cookButton.OnClickEvent.AddListener((w, d) => Cook());
            UpdateResultingBuffs();
        }

        public void PopulateIngredients(List<IngredientData> allIngredients)
        {
            if (_ingredientSelectionList == null) return;
            _ingredientSelectionList.ClearItems();

            foreach (var ingredient in allIngredients)
            {
                var itemWidget = _ingredientSelectionList.AddWidget(ingredient.itemName, ingredient);

                // KORREKTUR: GetComponent statt AddComponent verwenden.
                var image = itemWidget.gameObject.GetComponent<Image>();
                if (image != null)
                {
                    image.sprite = ingredient.icon;
                }
                
                itemWidget.gameObject.AddComponent<UIDraggable>();
            }
        }

        private void UpdateResultingBuffs()
        {
            var totalBuffs = new Dictionary<BuffType, int>();
            
            foreach (var slot in _ingredientSlots)
            {
                var draggable = slot.GetComponentInChildren<UIDraggable>();
                if (draggable != null && draggable.UserData is IngredientData ingredient)
                {
                    foreach (var buff in ingredient.buffs)
                    {
                        if (!totalBuffs.ContainsKey(buff.type))
                        {
                            totalBuffs[buff.type] = 0;
                        }
                        totalBuffs[buff.type] += buff.value;
                    }
                }
            }

            if (totalBuffs.Count == 0)
            {
                _resultBuffsText.text = "Wähle Zutaten aus...";
                _cookButton.SetState(UIState.Disabled);
                return;
            }

            string buffsText = "Resultierende Boni:\n";
            foreach (var buffEntry in totalBuffs)
            {
                buffsText += $"• {buffEntry.Key} +{buffEntry.Value}\n";
            }

            _resultBuffsText.text = buffsText;
            _cookButton.SetState(UIState.Interactive);
        }

        private void Cook()
        {
            if (_cookButton.CurrentState == UIState.Disabled) return;

            Debug.Log("Mahlzeit wird gekocht! Folgende Boni sind jetzt aktiv:");
            Debug.Log(_resultBuffsText.text.Replace("Resultierende Boni:\n", "").Trim());
        }
    }
}