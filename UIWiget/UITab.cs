// Dateiname: UITab.cs
using UnityEngine;

namespace YourGame.UI.Widgets
{
    // Erbt von UIButton, um alle Button-Funktionen zu erhalten
    public class UITab : UIButton
    {
        [Header("Tab Settings")]
        [Tooltip("Das Panel, das dieser Tab aktivieren soll.")]
        [SerializeField] private GameObject _tabPage;

        public GameObject TabPage => _tabPage;
    }
}