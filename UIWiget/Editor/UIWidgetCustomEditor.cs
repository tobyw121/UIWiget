// Dateiname: UIWidgetCustomEditor.cs
// Muss im "Editor"-Ordner platziert werden.
using UnityEditor;
using UnityEngine;
using YourGame.UI.Widgets;

[CustomEditor(typeof(UIWidget), true)] // true = auch für abgeleitete Klassen
public class UIWidgetCustomEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Zeichnet den Standard-Inspector
        base.OnInspectorGUI();

        UIWidget widget = (UIWidget)target;

        // Zeigt den Live-Zustand des Widgets im Play-Modus an
        if (Application.isPlaying)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Live Status", EditorStyles.boldLabel);
            GUI.enabled = false; // Macht die Felder schreibgeschützt
            EditorGUILayout.Toggle("Is Visible", widget.IsVisible);
            EditorGUILayout.EnumPopup("Current State", widget.CurrentState);
            GUI.enabled = true;
        }

        // Fügt benutzerdefinierte Buttons für das Testen im Editor hinzu
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Editor Testing", EditorStyles.boldLabel);
        
        // Horizontale Anordnung für Show/Hide/Toggle
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Show"))
        {
            widget.Show();
        }
        if (GUILayout.Button("Hide"))
        {
            widget.Hide();
        }
        if (GUILayout.Button("Toggle"))
        {
            widget.Toggle();
        }
        EditorGUILayout.EndHorizontal();

        // Horizontale Anordnung für Zustandsänderungen
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Set Interactive"))
        {
            widget.SetState(UIWidget.UIState.Interactive);
        }
        if (GUILayout.Button("Set Disabled"))
        {
            widget.SetState(UIWidget.UIState.Disabled);
        }
        EditorGUILayout.EndHorizontal();

        // Button für Tweening-Demonstration
        if (GUILayout.Button("Demo Tween Animation"))
        {
            // Führt eine vordefinierte Animation aus, um die Funktion zu testen
            var rt = widget.GetComponent<RectTransform>();
            if (rt != null)
            {
                Vector2 originalPos = rt.anchoredPosition;
                widget.TweenPosition(originalPos + new Vector2(50, 0), 0.5f, Easing.EaseType.EaseOutQuad);
            }
        }
    }
}