// Dateiname: UIModalDialog.cs
using UnityEngine;
using UnityEngine.Events;
using YourGame.UI.Widgets;

namespace YourGame.UI
{
    public class UIModalDialog : UIWidget
    {
        [Header("Modal Dialog Components")]
        [SerializeField] private UIWidget titleWidget;
        [SerializeField] private UIWidget messageWidget;
        [SerializeField] private UIButton okButton;
        [SerializeField] private UIButton cancelButton;
        [SerializeField] private UIButton closeButton;

        public UnityEvent OnOK;
        public UnityEvent OnCancel;

        protected override void Awake()
        {
            base.Awake();
            if (okButton) okButton.OnClickEvent.AddListener((w, d) => { OnOK?.Invoke(); Hide(); });
            if (cancelButton) cancelButton.OnClickEvent.AddListener((w, d) => { OnCancel?.Invoke(); Hide(); });
            if (closeButton) closeButton.OnClickEvent.AddListener((w, d) => { OnCancel?.Invoke(); Hide(); });
        }

        public void ShowDialog(string title, string message, UnityAction onOkAction, UnityAction onCancelAction = null)
        {
            if (titleWidget) titleWidget.SetText(title);
            if (messageWidget) messageWidget.SetText(message);
            
            OnOK.RemoveAllListeners();
            OnCancel.RemoveAllListeners();

            OnOK.AddListener(() => onOkAction?.Invoke());
            if (onCancelAction != null) {
                OnCancel.AddListener(() => onCancelAction.Invoke());
                cancelButton?.gameObject.SetActive(true);
            } else {
                cancelButton?.gameObject.SetActive(false);
            }
            
            Show();
            UIWidgetManager.SetExclusive(this, new Color(0, 0, 0, 0.7f));
        }

        public override void Hide()
        {
            base.Hide();
            UIWidgetManager.RemoveExclusive(this);
        }
    }
}