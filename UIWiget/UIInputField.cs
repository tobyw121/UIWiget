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

        // KORREKTUR: "override" hinzugefügt
        public override void OnFocusGained()
        {
            base.OnFocusGained();
            _inputField.ActivateInputField();
        }
        
        // KORREKTUR: "override" hinzugefügt (gute Praxis, auch wenn hier leer)
        public override void OnFocusLost()
        {
            base.OnFocusLost();
            // Hier könnte man z.B. visuelles Feedback für den Fokusverlust hinzufügen
        }
        
        public override void SetText(string text)
        {
            if (_inputField != null)
            {
                _inputField.text = text;
            }
        }
    }
}