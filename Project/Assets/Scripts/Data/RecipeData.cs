using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "New Recipe", menuName = "Recipe")]

public class RecipeData : ScriptableObject
{
    public TileBase nearbyTile;
    public ItemPackage[] ingredients;
    public ItemPackage product;
    public ItemDataContainer[] nearbyTilesForCrafting;
}
