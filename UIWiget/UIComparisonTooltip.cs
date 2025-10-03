// Dateiname: UIComparisonTooltip.cs (Final Korrigiert)
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace YourGame.UI.Widgets
{
    public class UIComparisonTooltip : UIWidget
    {
        public static UIComparisonTooltip Instance { get; private set; }

        [Header("Current Item Panel")]
        [SerializeField] private GameObject _currentItemPanel;
        [SerializeField] private TextMeshProUGUI _currentItemName;
        [SerializeField] private Image _currentItemIcon;
        [SerializeField] private TextMeshProUGUI _currentItemAttack;
        [SerializeField] private TextMeshProUGUI _currentItemDefense;

        [Header("New Item Panel")]
        [SerializeField] private GameObject _newItemPanel;
        [SerializeField] private TextMeshProUGUI _newItemName;
        [SerializeField] private Image _newItemIcon;
        [SerializeField] private TextMeshProUGUI _newItemAttack;
        [SerializeField] private TextMeshProUGUI _newItemDefense;

        protected override void Awake()
        {
            base.Awake();
            if (Instance == null) { Instance = this; } else { Destroy(gameObject); }
            Hide();
        }

        public void ShowComparison(EquipmentData newItemData, EquipmentData currentItemData)
        {
            if (newItemData == null) return;
            
            // --- Neues Item befüllen ---
            _newItemPanel.SetActive(true);
            _newItemName.text = newItemData.itemName;
            _newItemIcon.sprite = newItemData.icon;

            // --- Aktuelles Item befüllen (oder Panel ausblenden) ---
            if (currentItemData != null)
            {
                _currentItemPanel.SetActive(true);
                _currentItemName.text = currentItemData.itemName;
                _currentItemIcon.sprite = currentItemData.icon;
                
                // Stat-Vergleich
                CompareAndSetStat(_newItemAttack, _currentItemAttack, newItemData.attack, currentItemData.attack);
                CompareAndSetStat(_newItemDefense, _currentItemDefense, newItemData.defense, currentItemData.defense);
            }
            else
            {
                _currentItemPanel.SetActive(false);
                // Stats ohne Vergleich anzeigen
                _newItemAttack.text = newItemData.attack.ToString();
                _newItemDefense.text = newItemData.defense.ToString();
                _newItemAttack.color = Color.white;
                _newItemDefense.color = Color.white;
            }
            
            this.RectTransform.position = Input.mousePosition;
            Show();
        }

        private void CompareAndSetStat(TextMeshProUGUI newStatText, TextMeshProUGUI currentStatText, int newValue, int currentValue)
        {
            currentStatText.text = currentValue.ToString();
            newStatText.text = newValue.ToString();

            if (newValue > currentValue)
            {
                newStatText.color = Color.green;
            }
            else if (newValue < currentValue)
            {
                newStatText.color = Color.red;
            }
            else
            {
                newStatText.color = Color.white;
            }
        }
    }
}