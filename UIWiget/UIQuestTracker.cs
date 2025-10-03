using UnityEngine;
using TMPro;
using System.Collections.Generic;
using YourGame.Quests;

namespace YourGame.UI.Widgets
{
    public class UIQuestTracker : UIWidget
    {
        [Header("Quest Tracker Components")]
        [SerializeField] private TextMeshProUGUI _questTitleText;
        [SerializeField] private RectTransform _objectivesContainer;
        [SerializeField] private GameObject _objectiveTemplate; // Ein Prefab für eine einzelne Ziel-Zeile

        private List<GameObject> _activeObjectives = new List<GameObject>();

        protected override void Awake()
        {
            base.Awake();
            if (_objectiveTemplate != null) _objectiveTemplate.SetActive(false);
            // QuestManager.OnQuestUpdated += UpdateTracker; // An ein Event binden
        }

        public void UpdateTracker(Quest currentQuest) // 'Quest' wäre eine eigene Datenklasse
        {
            // Alte Ziele löschen
            foreach (var obj in _activeObjectives)
            {
                Destroy(obj);
            }
            _activeObjectives.Clear();

            if (currentQuest == null || currentQuest.IsComplete)
            {
                Hide();
                return;
            }

            _questTitleText.text = currentQuest.Title;

            foreach (var objective in currentQuest.Objectives)
            {
                GameObject newObjectiveGO = Instantiate(_objectiveTemplate, _objectivesContainer);
                var objectiveText = newObjectiveGO.GetComponentInChildren<TextMeshProUGUI>();
                if (objectiveText != null)
                {
                    // z.B. "Sammle 5/10 Fische"
                    objectiveText.text = $"{objective.Description} ({objective.CurrentProgress}/{objective.RequiredAmount})";
                }
                newObjectiveGO.SetActive(true);
                _activeObjectives.Add(newObjectiveGO);
            }
            Show();
        }
    }
}