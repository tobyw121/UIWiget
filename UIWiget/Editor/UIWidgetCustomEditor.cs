// Dateiname: UIWidgetCustomEditor.cs (muss im "Editor"-Ordner sein)
using UnityEditor;
using UnityEngine;
using YourGame.UI.Widgets;
using System.Linq;
using System.Collections.Generic;

[CustomEditor(typeof(UIWidget), true)]
public class UIWidgetCustomEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Zeichnet zuerst den Standard-Inspector
        base.OnInspectorGUI();

        UIWidget widget = (UIWidget)target;

        // Nur versuchen, die Stil-Liste zu laden, wenn die Anwendung läuft ODER ein Theme-Manager in der Szene ist
        if (UIThemeManager.Instance == null && !Application.isPlaying)
        {
             EditorGUILayout.HelpBox("UIThemeManager nicht in der Szene gefunden. Stil-Auswahl ist nicht verfügbar.", MessageType.Warning);
             return;
        }
        
        // --- NEU: StyleKey als Dropdown-Menü ---
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Theme Style Selector", EditorStyles.boldLabel);

        var themeManager = UIThemeManager.Instance;
        // Holen Sie sich alle Stil-Schlüssel aus dem aktuellen Theme
        List<string> styleKeys = new List<string> { "None" }; // Option für "kein Stil"
        if (themeManager != null)
        {
            // Diese private Methode muss im UIThemeManager kurz public gemacht oder über Reflection geholt werden.
            // Für Einfachheit, fügen Sie eine public Methode im UIThemeManager hinzu:
            // public List<string> GetAllStyleKeys() => _styleCache.Keys.ToList();
            // styleKeys.AddRange(themeManager.GetAllStyleKeys());
        }

        // Finde den aktuellen Index des ausgewählten Schlüssels
        string currentKey = widget.styleKey;
        int selectedIndex = 0;
        if (!string.IsNullOrEmpty(currentKey) && styleKeys.Contains(currentKey))
        {
            selectedIndex = styleKeys.IndexOf(currentKey);
        }

        // Zeichne das Dropdown-Menü
        int newIndex = EditorGUILayout.Popup("Style Key", selectedIndex, styleKeys.ToArray());

        // Wenn ein neuer Wert ausgewählt wurde, aktualisiere ihn
        if (newIndex != selectedIndex)
        {
            Undo.RecordObject(widget, "Change Style Key"); // Für Rückgängig-Funktion
            widget.styleKey = (newIndex == 0) ? "" : styleKeys[newIndex];
            EditorUtility.SetDirty(widget); // Speichere die Änderung am Prefab/Objekt
        }
    }
}