// Dateiname: UIRadialMenu.cs
using UnityEngine;
using System.Collections.Generic;

namespace YourGame.UI.Widgets
{
    public class UIRadialMenu : UIMenu
    {
        [Header("Radial Menu Settings")]
        [SerializeField] private float _radius = 150f;
        [SerializeField] private float _startAngle = 90f; // Startet oben in der Mitte

        private Vector2 _centerPosition;

        protected override void Awake()
        {
            base.Awake();
            // Verstecke das Menü standardmäßig
            if (_canvasGroup != null) _canvasGroup.alpha = 0;
            gameObject.SetActive(false);
        }

        // Überschreibt Show, um das Menü an der Mausposition zu öffnen
        public override void Show()
        {
            _centerPosition = Input.mousePosition;
            transform.position = _centerPosition;
            base.Show();
            ArrangeItems();
        }
        
        // Überschreibt AddWidget, um die Anordnung sofort zu aktualisieren
        public override UIWidget AddWidget(string name, object userData = null)
        {
            var newWidget = base.AddWidget(name, userData);
            if(newWidget != null)
            {
                ArrangeItems();
            }
            return newWidget;
        }

        // Ordnet alle Kind-Elemente im Kreis an
        private void ArrangeItems()
        {
            if (Items.Count == 0) return;

            float angleStep = 360f / Items.Count;
            for (int i = 0; i < Items.Count; i++)
            {
                float currentAngle = (_startAngle - (i * angleStep)) * Mathf.Deg2Rad;
                
                float xPos = Mathf.Cos(currentAngle) * _radius;
                float yPos = Mathf.Sin(currentAngle) * _radius;

                UIWidget item = Items[i];
                if (item != null)
                {
                    item.RectTransform.anchoredPosition = new Vector2(xPos, yPos);
                }
            }
        }

        public override void ClearItems()
        {
            base.ClearItems();
            ArrangeItems(); // Aktualisiert die Ansicht
        }
    }
}