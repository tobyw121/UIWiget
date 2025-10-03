// Dateiname: UINotification.cs
using UnityEngine;
using TMPro;
using System.Collections;

namespace YourGame.UI.Widgets
{
    // Erbt von UIWidget für Show/Hide-Animationen
    public class UINotification : UIWidget
    {
        [Header("Notification Components")]
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _messageText;
        [SerializeField] private float _displayDuration = 3f;

        // Startet ein selbstzerstörendes Coroutine
        public void Launch()
        {
            Show();
            StartCoroutine(WaitAndHide());
        }

        private IEnumerator WaitAndHide()
        {
            yield return new WaitForSeconds(_displayDuration);
            Hide();
        }

        public void SetContent(string title, string message)
        {
            if (_titleText) _titleText.text = title;
            if (_messageText) _messageText.text = message;
        }
    }
}