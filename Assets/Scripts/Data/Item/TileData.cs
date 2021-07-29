using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEngine;

[System.Serializable]

public class TileData
{
    [Header("General Tile Options")]
    public ItemPackage itemDroppedOnBreak;
    public byte hardness;
    public Tool requiredTool;
    public TileBase tile;
    public byte attachToLayer;
    public byte placeOnLayer;
    public SoundData breakSound;
    public SoundData hitSound;
    public bool interactable;

    [Header("Attachment Options")]
    public List<ItemDataContainer> otherAttachTo;
    public bool placeBehindForeground;
    public bool noAttachToBackground;
    public bool noAttachToCeiling;
    public bool noAttachToWalls;
    public bool noPlaceOnFloor;

    // Mostly for multi tile parts
    public bool canFloat;

    [Header("MultiTileItem Options")]
    public PartInMutliTileItemData partOfMultiTileData;
    public Structure multiBlockStructure;
    public bool isPartOfMultiTile;
    public bool isChest;

    [Header("Item Specifics")]
    public bool hideBreakingGraphic;
    public bool cementBlockBelow;
    public bool treeTile;

}
