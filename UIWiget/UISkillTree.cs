// Dateiname: UISkillTree.cs (Final Korrigiert)
using UnityEngine;
using TMPro;
using System.Collections.Generic;

namespace YourGame.UI.Widgets
{
    public class UISkillTree : UIWidget
    {
        private TextMeshProUGUI _skillPointsText;
        private List<UISkillNode> _allNodes;
        
        // KORREKTUR: Wir speichern das GameObject des Prefabs, nicht die Komponente.
        private GameObject _lineRendererPrefabGO;

        private int _availableSkillPoints = 10;
        private List<UILineRenderer> _lines = new List<UILineRenderer>();

        // KORREKTUR: Die Signatur wurde an das GameObject-Prefab angepasst.
        public void Initialize(TextMeshProUGUI pointsText, List<UISkillNode> nodes, GameObject linePrefabGO)
        {
            _skillPointsText = pointsText;
            _allNodes = nodes;
            _lineRendererPrefabGO = linePrefabGO;

            DrawConnectionLines();
            RefreshTree();
        }

        public void TryUnlockNode(UISkillNode node)
        {
            if (node.IsUnlocked || _availableSkillPoints < node.cost) return;

            bool dependenciesMet = true;
            foreach (var dep in node.dependencies)
            {
                if (!dep.IsUnlocked)
                {
                    dependenciesMet = false;
                    break;
                }
            }

            if (dependenciesMet)
            {
                _availableSkillPoints -= node.cost;
                node.UnlockForDemo();
                Debug.Log($"Skill '{node.skillName}' freigeschaltet!");
                RefreshTree();
            }
        }

        private void RefreshTree()
        {
            if (_skillPointsText != null)
                _skillPointsText.text = $"Verfügbare Punkte: {_availableSkillPoints}";
            
            if (_allNodes == null) return;
            foreach (var node in _allNodes)
            {
                if (node == null) continue;
                bool canUnlock = true;
                foreach (var dep in node.dependencies)
                {
                    if (dep == null || !dep.IsUnlocked)
                    {
                        canUnlock = false;
                        break;
                    }
                }
                node.SetState(node.IsUnlocked, canUnlock);
            }
        }

        private void DrawConnectionLines()
        {
            foreach(var line in _lines) { if(line != null) Destroy(line.gameObject); }
            _lines.Clear();
            
            if (_allNodes == null || _lineRendererPrefabGO == null) return;

            foreach (var node in _allNodes)
            {
                if (node == null) continue;
                foreach (var dep in node.dependencies)
                {
                    if (dep == null) continue;

                    // KORREKTUR: Wir instanziieren das GameObject, stellen sicher, dass es aktiv ist,
                    // und holen uns DANN die Komponente.
                    GameObject newLineGO = Instantiate(_lineRendererPrefabGO, transform);
                    newLineGO.SetActive(true);
                    UILineRenderer line = newLineGO.GetComponent<UILineRenderer>();
                    
                    if (line != null)
                    {
                        line.transform.SetAsFirstSibling();
                        line.SetPositions(node.RectTransform.anchoredPosition, dep.RectTransform.anchoredPosition);
                        _lines.Add(line);
                    }
                }
            }
        }
    }
}