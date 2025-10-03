// Dateiname: EquipmentData.cs
using UnityEngine;

[CreateAssetMenu(fileName = "New EquipmentData", menuName = "UI-Demo/Equipment Data")]
public class EquipmentData : ItemData
{
    [Header("Equipment Stats")]
    public int attack;
    public int defense;
    public int speed;
}