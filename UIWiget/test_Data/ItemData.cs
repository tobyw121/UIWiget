// Dateiname: ItemData.cs
using UnityEngine;

[CreateAssetMenu(fileName = "New ItemData", menuName = "UI-Demo/Item Data")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    [TextArea] public string description;
}