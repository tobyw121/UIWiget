// Dateiname: UIStatusEffectDisplay.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace YourGame.UI
{
    // Diese Klasse verwaltet eine Liste von Buff/Debuff-Icons
    public class UIStatusEffectDisplay : MonoBehaviour
    {
        [Header("Status Effect Settings")]
        [SerializeField] private YourGame.UI.Widgets.UINotification _effectIconPrefab; // Wir können die UINotification-Klasse hier wiederverwenden!
        [SerializeField] private Transform _container;
        private Queue<YourGame.UI.Widgets.UINotification> _pooledIcons = new Queue<YourGame.UI.Widgets.UINotification>();
        
        private void Awake()
        {
            // Initialisiere einen Pool von Icons
            for (int i = 0; i < 10; i++)
            {
                var icon = Instantiate(_effectIconPrefab, _container);
                icon.gameObject.SetActive(false);
                _pooledIcons.Enqueue(icon);
            }
        }
        
        // Fügt einen neuen Effekt hinzu
        public void AddEffect(Sprite iconSprite, float duration)
        {
            if (_pooledIcons.Count == 0) return; // Kein freies Icon verfügbar

            var effectIcon = _pooledIcons.Dequeue();
            effectIcon.GetComponent<Image>().sprite = iconSprite;
            effectIcon.gameObject.SetActive(true);
            
            effectIcon.Show(); // Startet die Einblend-Animation
            
            // Starte einen Coroutine, um das Icon nach Ablauf der Dauer auszublenden und in den Pool zurückzulegen
            StartCoroutine(EffectRoutine(effectIcon, duration));
        }

        private System.Collections.IEnumerator EffectRoutine(YourGame.UI.Widgets.UINotification icon, float duration)
        {
            // Hier könnte man den Cooldown-Overlay des Icons aktualisieren, falls vorhanden
            yield return new WaitForSeconds(duration);
            icon.Hide();
            
            // Warte, bis die Ausblend-Animation fertig ist, bevor es in den Pool geht
            yield return new WaitWhile(() => icon.IsVisible);

            icon.gameObject.SetActive(false);
            _pooledIcons.Enqueue(icon);
        }
    }
}