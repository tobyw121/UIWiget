using System.Collections.Generic;
using UnityEngine;

namespace YourGame.UI.Widgets
{
    public class UITabView : UIWidget
    {
        [Header("Tab View Settings")]
        [SerializeField] private List<UITab> _tabs = new List<UITab>();
        [SerializeField] private int _startingTabIndex = 0;
        
        private UITab _currentActiveTab;

        protected override void Awake()
        {
            base.Awake();
            // Registriere Klick-Events für alle Tabs
            foreach (var tab in _tabs)
            {
                if (tab != null)
                {
                    tab.OnClickEvent.AddListener((widget, data) => OnTabSelected(widget as UITab));
                }
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            // Wähle den Start-Tab (sofern gültig)
            if (_tabs.Count > 0 && _startingTabIndex < _tabs.Count && _tabs[_startingTabIndex] != null)
            {
                OnTabSelected(_tabs[_startingTabIndex]);
            }
        }

        public void OnTabSelected(UITab tab)
        {
            if (tab == null || _currentActiveTab == tab) return;
            
            // Deaktiviere das Panel des vorherigen Tabs
            if (_currentActiveTab != null)
            {
                // KORREKTUR: Die Zeile, die den Zustand des alten Tabs ändert, wurde entfernt.
                // _currentActiveTab.SetState(UIState.Interactive); 
                if(_currentActiveTab.TabPage != null) _currentActiveTab.TabPage.SetActive(false);
            }

            // Aktiviere den neuen Tab und sein Panel
            _currentActiveTab = tab;
            
            // KORREKTUR: Die Zeile, die den Zustand des neuen, aktiven Tabs ändert, wurde entfernt.
            // _currentActiveTab.SetState(UIState.Disabled); 
            if(_currentActiveTab.TabPage != null) _currentActiveTab.TabPage.SetActive(true);
        }
    }
}