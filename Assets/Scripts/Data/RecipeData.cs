using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Recipe", menuName = "Recipe")]

public class RecipeData : ScriptableObject
{
    public RuleTile nearbyTile;
    public int amountPerCraft;
    public ItemPackage[] ingredients;
    public TileData nearbyTilesForCrafting;
}
