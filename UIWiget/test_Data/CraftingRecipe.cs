// Dateiname: CraftingRecipe.cs
using UnityEngine;
using System.Collections.Generic;

// Annahme: Es gibt eine Basis-Datenklasse für Items
// [CreateAssetMenu(fileName = "New Item", menuName = "Game/Item")]
// public class ItemData : ScriptableObject { public string itemName; public Sprite icon; }

[System.Serializable]
public class RequiredMaterial
{
    public ItemData item;
    public int quantity;
}

[CreateAssetMenu(fileName = "New Recipe", menuName = "Game/Crafting Recipe")]
public class CraftingRecipe : ScriptableObject
{
    public ItemData resultItem;
    public int resultQuantity = 1;
    public List<RequiredMaterial> requiredMaterials;
}