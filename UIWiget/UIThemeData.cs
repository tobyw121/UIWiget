// Dateiname: UIThemeData.cs (Aktualisiert)
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "UITheme", menuName = "YourGame/UI/New UI Theme")]
public class UIThemeData : ScriptableObject
{
    [Header("UI Style Definitions")]
    [Tooltip("Eine Liste aller visuellen Stile, die in diesem Theme verfügbar sind.")]
    public List<UIStyle> styles = new List<UIStyle>();
}