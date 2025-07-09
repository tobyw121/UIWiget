// Dateiname: UINotificationManager.cs
using System.Collections.Generic;
using UnityEngine;

namespace YourGame.UI
{
    public class UINotificationManager : MonoBehaviour
    {
        public static UINotificationManager Instance { get; private set; }

        [Header("Manager Settings")]
        [SerializeField] private YourGame.UI.Widgets.UINotification _notificationPrefab;
        [SerializeField] private Transform _container; // Wo die Notifikationen erscheinen
        [SerializeField] private int _poolSize = 5;

        private Queue<YourGame.UI.Widgets.UINotification> _pooledNotifications = new Queue<YourGame.UI.Widgets.UINotification>();
        private Queue<KeyValuePair<string, string>> _messageQueue = new Queue<KeyValuePair<string, string>>();
        private bool _isShowingNotification = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializePool();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializePool()
        {
            for (int i = 0; i < _poolSize; i++)
            {
                var notification = Instantiate(_notificationPrefab, _container);
                notification.gameObject.SetActive(false);
                _pooledNotifications.Enqueue(notification);
            }
        }

        public static void Show(string title, string message)
        {
            Instance?._messageQueue.Enqueue(new KeyValuePair<string, string>(title, message));
        }

        private void Update()
        {
            // Verarbeite die Warteschlange
            if (_messageQueue.Count > 0 && !_isShowingNotification)
            {
                if (_pooledNotifications.Count > 0)
                {
                    _isShowingNotification = true;
                    var notification = _pooledNotifications.Dequeue();
                    var messageData = _messageQueue.Dequeue();
                    
                    notification.SetContent(messageData.Key, messageData.Value);
                    notification.gameObject.SetActive(true);
                    notification.Launch();

                    // Listener hinzufügen, um das Objekt nach dem Ausblenden wieder in den Pool zu legen
                    notification.OnHideComplete.AddListener(() => OnNotificationHidden(notification));
                }
            }
        }

        private void OnNotificationHidden(YourGame.UI.Widgets.UINotification notification)
        {
            notification.OnHideComplete.RemoveAllListeners(); // Wichtig, um Memory Leaks zu vermeiden
            notification.gameObject.SetActive(false);
            _pooledNotifications.Enqueue(notification);
            _isShowingNotification = false;
        }
    }
}