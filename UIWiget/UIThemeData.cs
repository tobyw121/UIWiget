// Dateiname: UIThemeData.cs
using UnityEngine;
using TMPro;

[CreateAssetMenu(fileName = "New UITheme", menuName = "Mein Spiel/UI/Theme Data")]
public class UIThemeData : ScriptableObject
{
    [Header("Farben")]
    public Color normalColor = Color.white;
    public Color hoverColor = new Color(0.9f, 0.9f, 0.9f, 1f);
    public Color pressedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
    public Color disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    public Color selectedColor = new Color(0.8f, 0.9f, 1f, 1f);

    [Header("Sprites (Optional)")]
    public Sprite normalSprite;
    public Sprite hoverSprite;
    public Sprite pressedSprite;
    public Sprite disabledSprite;
    public Sprite selectedSprite;

    [Header("Schriftarten (Optional)")]
    public TMP_FontAsset mainFont;
    public Color fontColor = Color.black;
}