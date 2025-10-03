// Dateiname: IngredientData.cs
using UnityEngine;
using System.Collections.Generic;

// Enum für alle möglichen Buff-Typen
public enum BuffType { Gesundheit, Ausdauer, Angriff, Verteidigung, Geschwindigkeit }

// Eine Struktur, die einen einzelnen Buff definiert
[System.Serializable]
public struct Buff
{
    public BuffType type;
    public int value;
}

[CreateAssetMenu(fileName = "New Ingredient", menuName = "UI-Demo/Ingredient Data")]
public class IngredientData : ItemData
{
    public enum IngredientCategory { Fleisch, Fisch, Gemüse }
    public IngredientCategory category;
    
    [Header("Ingredient Boni")]
    // Jede Zutat hat eine Liste von Boni, die sie gewährt.
    public List<Buff> buffs = new List<Buff>();
}