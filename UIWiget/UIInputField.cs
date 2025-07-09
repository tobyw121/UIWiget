// Dateiname: UIInputField.cs
using UnityEngine;
using TMPro;

namespace YourGame.UI.Widgets
{
    [RequireComponent(typeof(TMP_InputField))]
    public class UIInputField : UIWidget
    {
        private TMP_InputField _inputField;
        public TMP_InputField InputField => _inputField;

        protected override void Awake()
        {
            _inputField = GetComponent<TMP_InputField>();
            base.Awake(); // Ruft Awake von UIWidget auf, NACHDEM _inputField initialisiert wurde

            // Leite Events vom InputField an das UIInputHandler-Fokussystem weiter
            _inputField.onSelect.AddListener(delegate { UIInputHandler.Instance?.SetFocus(this); });
            _inputField.onDeselect.AddListener(delegate { if(UIInputHandler.Instance?.FocusedWidget == this) UIInputHandler.Instance.SetFocus(null); });
        }

        // Wenn das Widget den Fokus vom UIInputHandler erhält, fokussiere das Textfeld
        public override void OnFocusGained()
        {
            base.OnFocusGained();
            _inputField.ActivateInputField();
        }
        
        // Überschreibe SetText, um das InputField zu aktualisieren
        public override void SetText(string text)
        {
            if (_inputField != null)
            {
                _inputField.text = text;
            }
        }
    }
}