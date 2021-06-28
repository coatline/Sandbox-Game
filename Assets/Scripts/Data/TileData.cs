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
    public WorldLayer layer;
    public RuleTile tile;
    public Structure multiBlockItem;
    public ItemDataContainer dropOnBreak;
    public PartInMutliTileItemData pimtid;
    public int amountDropped;
    public bool treeTile;
}
