// Dateiname: UISkillNode.cs (Final Korrigiert)
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace YourGame.UI.Widgets
{
    public class UISkillNode : UIButton
    {
        [Header("Skill Node Settings")]
        public string skillName;
        [TextArea] public string skillDescription;
        public int cost = 1;
        
        // KORREKTUR: Die Liste wird hier direkt initialisiert, um Null-Fehler zu vermeiden.
        public List<UISkillNode> dependencies = new List<UISkillNode>();

        public bool IsUnlocked { get; private set; }

        private UISkillTree _skillTree;

        protected override void Awake()
        {
            base.Awake();
            _skillTree = GetComponentInParent<UISkillTree>();
            OnClickEvent.AddListener((w,d) => 
            {
                if (_skillTree != null)
                {
                    _skillTree.TryUnlockNode(this);
                }
            });
        }

        public void SetState(bool unlocked, bool canUnlock)
        {
            IsUnlocked = unlocked;
            if (unlocked)
            {
                if(targetGraphic != null) targetGraphic.color = Color.yellow; 
                base.SetState(UIState.Disabled);
            }
            else
            {
                if(targetGraphic != null) targetGraphic.color = canUnlock ? Color.white : Color.grey;
                base.SetState(canUnlock ? UIState.Interactive : UIState.Disabled);
            }
        }
        
        public void UnlockForDemo()
        {
            IsUnlocked = true;
        }
    }
}