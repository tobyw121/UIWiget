// Dateiname: UIMinimap.cs (Aktualisiert)
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace YourGame.UI.Widgets
{
    // Hilfsklasse für die Icons auf der Karte
    public class UIMinimapIcon : MonoBehaviour
    {
        public Image IconImage;
        public Transform WorldTarget; // Das Objekt in der Spielwelt
    }

    public class UIMinimap : UIWidget
    {
        [Header("Minimap Components")]
        [SerializeField] private RectTransform _iconContainer;
        [SerializeField] private UIMinimapIcon _playerIcon;
        [SerializeField] private GameObject _iconPrefab; // Ein Prefab für NPCs, Quests etc.

        [Header("Minimap Settings")]
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private float _mapRadius = 100f; // Radius, in dem Objekte auf der Karte angezeigt werden
        [SerializeField] private float _zoomLevel = 1f;

        private List<UIMinimapIcon> _trackedIcons = new List<UIMinimapIcon>();

        // Weisen Sie den Spieler zu, z.B. beim Spielstart
        public void SetPlayer(Transform player)
        {
            _playerTransform = player;
        }
        
        // Fügt ein neues Icon hinzu, das ein Welt-Objekt verfolgt
        public UIMinimapIcon AddIcon(Transform target, Sprite sprite, Color color)
        {
            if (_iconPrefab == null) return null;

            GameObject iconGO = Instantiate(_iconPrefab, _iconContainer);
            UIMinimapIcon newIcon = iconGO.GetComponent<UIMinimapIcon>();
            if (newIcon == null) newIcon = iconGO.AddComponent<UIMinimapIcon>();
            
            newIcon.IconImage = iconGO.GetComponent<Image>();
            newIcon.IconImage.sprite = sprite;
            newIcon.IconImage.color = color;
            newIcon.WorldTarget = target;
            
            _trackedIcons.Add(newIcon);
            return newIcon;
        }
        
        // Aktualisiert die Positionen aller Icons
        void LateUpdate()
        {
            if (_playerTransform == null) return;

            // Rotiere den Icon-Container entgegen der Spieler-Rotation, damit die Karte rotiert
            _iconContainer.rotation = Quaternion.Euler(0, 0, _playerTransform.eulerAngles.y);

            // Aktualisiere die Positionen der anderen Icons relativ zum Spieler
            foreach (var icon in _trackedIcons)
            {
                if (icon.WorldTarget == null)
                {
                    icon.gameObject.SetActive(false);
                    continue;
                }

                Vector3 direction = icon.WorldTarget.position - _playerTransform.position;
                float distance = Vector3.Distance(icon.WorldTarget.position, _playerTransform.position);

                // Blende Icons aus, die zu weit weg sind
                if (distance > _mapRadius)
                {
                    icon.gameObject.SetActive(false);
                    continue;
                }
                
                icon.gameObject.SetActive(true);

                // Berechne die 2D-Position auf der Minimap
                float mapX = direction.x / _mapRadius * (_iconContainer.rect.width / 2) * _zoomLevel;
                float mapY = direction.z / _mapRadius * (_iconContainer.rect.height / 2) * _zoomLevel;

                icon.GetComponent<RectTransform>().anchoredPosition = new Vector2(mapX, mapY);
            }
        }
    }
}