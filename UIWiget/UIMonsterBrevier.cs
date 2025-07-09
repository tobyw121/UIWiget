// Dateiname: UIMonsterBrevier.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace YourGame.UI.Widgets
{
    public class UIMonsterBrevier : UIWidget
    {
        [Header("Brevier Components")]
        [SerializeField] private UIScrollList _monsterList;
        [SerializeField] private TextMeshProUGUI _monsterNameText;
        [SerializeField] private Image _monsterIcon;
        [SerializeField] private UITabView _detailsTabView;
        
        [Header("Info Tab")]
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private TextMeshProUGUI _habitatText;

        [Header("Weakness & Loot Tabs")]
        [SerializeField] private RectTransform _weaknessContainer;
        [SerializeField] private RectTransform _lootContainer;
        [SerializeField] private GameObject _listEntryPrefab; // Simples Text-Prefab

        private MonsterData _selectedMonster;

        public void Initialize()
        {
            _monsterList.OnItemSelected.AddListener(OnMonsterSelected);
            if (_listEntryPrefab != null) _listEntryPrefab.SetActive(false);
        }

        public void PopulateMonsterList(List<MonsterData> discoveredMonsters)
        {
            _monsterList.ClearItems();
            foreach (var monster in discoveredMonsters)
            {
                var widget = _monsterList.AddWidget(monster.monsterName, monster);
                widget.SetText(monster.monsterName);
            }
        }

        private void OnMonsterSelected(UIWidget monsterWidget)
        {
            if (monsterWidget == null || !(monsterWidget.UserData is MonsterData)) return;
            
            _selectedMonster = monsterWidget.UserData as MonsterData;
            _monsterNameText.text = _selectedMonster.monsterName;
            _monsterIcon.sprite = _selectedMonster.monsterIcon;

            // Fülle die Tabs mit den neuen Daten
            PopulateInfoTab();
            PopulateWeaknessTab();
            PopulateLootTab();
        }

        private void PopulateInfoTab()
        {
            _descriptionText.text = _selectedMonster.description;
            _habitatText.text = $"Habitat: {_selectedMonster.habitat}";
        }

        private void PopulateWeaknessTab()
        {
            // Alte Einträge löschen
            foreach (Transform child in _weaknessContainer)
                if(child.gameObject.activeSelf) Destroy(child.gameObject);

            foreach (var weakness in _selectedMonster.weaknesses)
            {
                var entry = Instantiate(_listEntryPrefab, _weaknessContainer);
                entry.GetComponent<TextMeshProUGUI>().text = $"• {weakness}";
                entry.SetActive(true);
            }
        }
        
        private void PopulateLootTab()
        {
            // Alte Einträge löschen
            foreach (Transform child in _lootContainer)
                if(child.gameObject.activeSelf) Destroy(child.gameObject);

            foreach (var loot in _selectedMonster.lootTable)
            {
                var entry = Instantiate(_listEntryPrefab, _lootContainer);
                entry.GetComponent<TextMeshProUGUI>().text = $"• {loot.item.itemName} ({loot.dropChance})";
                entry.SetActive(true);
            }
        }
    }
}