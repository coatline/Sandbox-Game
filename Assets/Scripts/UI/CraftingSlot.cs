using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraftingSlot : SelectableSlot
{
    public RecipeData recipe;
    CraftingManager cm;

    private void Start()
    {
        cm = FindObjectOfType<CraftingManager>();
    }

    public void Craft()
    {
        cm.TryCraft(recipe);
    }

    public void InteractWithCursor()
    {
        Craft();
    }
}
