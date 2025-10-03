// Dateiname: UICraftingPanel.cs (Final Korrigiert)
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace YourGame.UI.Widgets
{
    public class UICraftingPanel : UIWidget
    {
        private UIScrollList _recipeList;
        private Image _resultIcon;
        private TextMeshProUGUI _resultName;
        private RectTransform _materialsContainer;
        private GameObject _materialTemplate;
        private UIButton _craftButton;

        private CraftingRecipe _selectedRecipe;

        // Die Zuweisungen erfolgen jetzt über Initialize
        public void Initialize(UIScrollList recipeList, Image resultIcon, TextMeshProUGUI resultName, RectTransform materialsContainer, GameObject materialTemplate, UIButton craftButton)
        {
            _recipeList = recipeList;
            _resultIcon = resultIcon;
            _resultName = resultName;
            _materialsContainer = materialsContainer;
            _materialTemplate = materialTemplate;
            _craftButton = craftButton;

            _recipeList.OnItemSelected.AddListener(OnRecipeSelected);
            _craftButton.OnClickEvent.AddListener((w, d) => CraftSelectedItem());
            if (_materialTemplate != null) _materialTemplate.SetActive(false);
        }

        public void PopulateRecipes(List<CraftingRecipe> availableRecipes)
        {
            _recipeList.ClearItems();
            foreach (var recipe in availableRecipes)
            {
                var widget = _recipeList.AddWidget(recipe.resultItem.itemName, recipe);
                widget.SetText(recipe.resultItem.itemName);
            }
        }

        private void OnRecipeSelected(UIWidget selectedWidget)
        {
            if (selectedWidget == null || !(selectedWidget.UserData is CraftingRecipe)) return;
            
            _selectedRecipe = selectedWidget.UserData as CraftingRecipe;
            
            _resultIcon.sprite = _selectedRecipe.resultItem.icon;
            _resultName.text = _selectedRecipe.resultItem.itemName;

            foreach (Transform child in _materialsContainer)
            {
                if (child.gameObject != _materialTemplate) Destroy(child.gameObject);
            }

            bool canCraft = CheckMaterialAvailability();
            _craftButton.SetState(canCraft ? UIState.Interactive : UIState.Disabled);
        }

        protected virtual bool CheckMaterialAvailability()
        {
            foreach (var material in _selectedRecipe.requiredMaterials)
            {
                GameObject materialGO = Instantiate(_materialTemplate, _materialsContainer);
                int ownedAmount = material.quantity;

                materialGO.transform.Find("Icon").GetComponent<Image>().sprite = material.item.icon;
                materialGO.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = material.item.itemName;
                var amountText = materialGO.transform.Find("Amount").GetComponent<TextMeshProUGUI>();
                amountText.text = $"{ownedAmount} / {material.quantity}";
                amountText.color = Color.white;
                
                materialGO.SetActive(true);
            }
            return true;
        }

        private void CraftSelectedItem()
        {
            if (_selectedRecipe == null || _craftButton.CurrentState == UIState.Disabled) return;
            Debug.Log($"Stelle her: {_selectedRecipe.resultItem.itemName}");
        }
    }
}