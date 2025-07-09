using UnityEngine;

[CreateAssetMenu(fileName = "UITheme", menuName = "YourGame/UI/New UI Theme")]
public class UIThemeData : ScriptableObject
{
    [Header("Colors")]
    public Color primaryTextColor;
    public Color buttonNormalColor;
    public Color buttonHighlightColor;
    
    [Header("Sprites")]
    public Sprite windowBackground;
    public Sprite buttonSprite;

    [Header("Fonts")]
    public TMPro.TMP_FontAsset mainFont;
}