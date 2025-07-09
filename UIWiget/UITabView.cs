// Dateiname: UITabView.cs
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace YourGame.UI.Widgets
{
    public class UITabView : UIWidget
    {
        [Header("Tab View Settings")]
        [SerializeField] private List<UITab> _tabs;
        [SerializeField] private int _startingTabIndex = 0;
        private UITab _currentActiveTab;

        protected override void Awake()
        {
            base.Awake();
            // Registriere Klick-Events für alle Tabs
            foreach (var tab in _tabs)
            {
                tab.OnClickEvent.AddListener((widget, data) => OnTabSelected(widget as UITab));
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            // Wähle den Start-Tab (sofern gültig)
            if (_tabs.Count > 0 && _startingTabIndex < _tabs.Count)
            {
                OnTabSelected(_tabs[_startingTabIndex]);
            }
        }

        public void OnTabSelected(UITab tab)
        {
            if (tab == null || _currentActiveTab == tab) return;

            // Deaktiviere den vorherigen Tab und sein Panel
            if (_currentActiveTab != null)
            {
                _currentActiveTab.SetState(UIState.Interactive); // Setze alten Tab zurück
                if(_currentActiveTab.TabPage != null) _currentActiveTab.TabPage.SetActive(false);
            }

            // Aktiviere den neuen Tab und sein Panel
            _currentActiveTab = tab;
            _currentActiveTab.SetState(UIState.Disabled); // Visueller Hinweis auf aktiven Tab
            if(_currentActiveTab.TabPage != null) _currentActiveTab.TabPage.SetActive(true);
        }
    }
}