using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

public class TileData
{
    [Header("General Tile Options")]
    public ItemPackage itemDroppedOnBreak;
    public byte hardness;
    public Tool requiredTool;
    public RuleTile tile;
    public byte attachToLayer;
    public byte placeOnLayer;
    public SoundData breakSound;
    public bool interactable;

    [Header("Attachment Options")]
    public List<ItemDataContainer> otherAttachTo;
    public bool noAttachToBackground;
    public bool noAttachToCeiling;
    public bool noAttachToWalls;
    public bool noPlaceOnFloor;

    [Header("MultiTileItem Options")]
    public PartInMutliTileItemData partOfMultiTileData;
    public Structure multiBlockStructure;
    public bool isPartOfMultiTile;

    [Header("Item Specifics")]
    public bool hideBreakingGraphic;
    public bool treeTile;

}
