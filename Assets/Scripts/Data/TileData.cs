using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WorldLayer
{
    foreground,
    midground,
    background
}

[System.Serializable]

public class TileData
{
    public ItemPackage itemDroppedOnBreak;
    public WorldLayer layer;
    public RuleTile tile;
    public int hardness;

    public PartInMutliTileItemData partOfMultiTileData;
    public Structure multiBlockItem;

    public bool treeTile;
}
