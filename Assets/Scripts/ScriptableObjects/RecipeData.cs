using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Recipe", menuName = "Recipe")]

public class RecipeData : ScriptableObject
{
    public RuleTile nearbyTile;
    public int amountPerCraft;
    public Ingredient[] ingredients;
    public TileData nearbyTilesForCrafting;
}
