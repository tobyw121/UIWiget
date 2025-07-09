// Dateiname: MonsterData.cs
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct MonsterLoot
{
    public ItemData item;
    public string dropChance; // z.B. "Häufig", "Selten"
}

[CreateAssetMenu(fileName = "New MonsterData", menuName = "UI-Demo/Monster Data")]
public class MonsterData : ScriptableObject
{
    public string monsterName;
    public Sprite monsterIcon;
    [TextArea] public string description;
    public string habitat;
    
    public List<string> weaknesses; // z.B. "Feuer", "Eis"
    public List<MonsterLoot> lootTable;
}