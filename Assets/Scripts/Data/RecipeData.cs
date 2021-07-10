using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Recipe", menuName = "Recipe")]

public class RecipeData : ScriptableObject
{
    public RuleTile nearbyTile;
    public ItemPackage[] ingredients;
    public ItemPackage product;
    public ItemDataConatainer[] nearbyTilesForCrafting;
}
