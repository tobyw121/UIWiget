using UnityEngine;
using UnityEngine.UI;

namespace YourGame.UI.Widgets
{
    public class UIChatBox : UIWidget
    {
        [Header("Chat Components")]
        [SerializeField] private UIScrollList _messageList;
        [SerializeField] private UIInputField _chatInput;
        [SerializeField] private Button _sendButton;
        // Optional: Tabs für verschiedene Kanäle (Global, Clan, Flüstern)

        protected override void Awake()
        {
            base.Awake();
            _chatInput.InputField.onSubmit.AddListener(SendMessage);
            _sendButton?.onClick.AddListener(() => SendMessage(_chatInput.InputField.text));
        }

        public void AddMessage(string playerName, string message, Color nameColor)
        {
            // Ein neues Widget zur _messageList hinzufügen
            var msgWidget = _messageList.AddWidget($"{playerName}_message");
            // Hier würde man den Text formatieren (z.B. mit Rich Text)
            msgWidget.SetText($"<color=#{ColorUtility.ToHtmlStringRGB(nameColor)}>{playerName}:</color> {message}");
        }

        private void SendMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;

            // Hier Logik zum Senden der Nachricht an den Server
            Debug.Log($"Sending message: {message}");

            // Für Testzwecke die eigene Nachricht direkt hinzufügen
            AddMessage("You", message, Color.yellow);
            
            _chatInput.SetText("");
            _chatInput.InputField.ActivateInputField(); // Fokus beibehalten
        }
    }
}