using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

public class TileData
{
    public List<ItemDataContainer> otherAttachTo;
    public ItemPackage itemDroppedOnBreak;
    public byte attachToLayer;
    public byte placeOnLayer;
    public bool noAttachToBackground;
    public bool noAttachToCeiling;
    public bool noAttachToWalls;
    public bool noPlaceOnFloor;
    public Tool requiredTool;
    public RuleTile tile;
    public byte hardness;

    public PartInMutliTileItemData partOfMultiTileData;
    public Structure multiBlockStructure;
    public bool isPartOfMultiTile;

    public bool treeTile;
}
