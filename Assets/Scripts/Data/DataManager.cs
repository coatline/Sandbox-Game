using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[DefaultExecutionOrder(-200)]
public class DataManager : MonoBehaviour
{
    [SerializeField] ItemDataContainer[] items;
    [SerializeField] Structure[] structures;

    Dictionary<string, Structure> structureFromName;
    Dictionary<string, short> itemIDfromName;

    private void Awake()
    {
        structureFromName = new Dictionary<string, Structure>();
        itemIDfromName = new Dictionary<string, short>();
        int tiles = 0;
        int weapons = 0;
        int tools= 0;

        for (short i = 0; i < items.Length; i++)
        {
            items[i].itemData.id = i;
            items[i].itemData.itemName = items[i].name;
            itemIDfromName.Add(items[i].itemData.itemName, i);

            if(items[i].itemType== ItemType.block)
            {
                tiles++;
            }
            else if (items[i].itemType == ItemType.tool)
            {
                tools++;
            }
            else if(items[i].itemType==ItemType.rangedWeapon || items[i].itemType == ItemType.meleeWeapon)
            {
                weapons++;
            }
        }

        for (int k = 0; k < structures.Length; k++)
        {
            structures[k]._name = structures[k].name;
            structureFromName.Add(structures[k]._name, structures[k]);
        }

        Debug.Log($"Database updated! It now contains {itemIDfromName.Count} items.\n{tiles} of them are tiles. {weapons} of them are weapons. {tools} of them are tools.");
    }

    public Structure GetStructure(string n)
    {
        return structureFromName[n];
    }

    public short GetItemID(string n)
    {
        return itemIDfromName[n];
    }

    public TileBase GetTile(int id)
    {
        return items[id].tileData.tile;
    }

    public ItemDataContainer GetItem(string n)
    {
        return items[itemIDfromName[n]];
    }

    public ItemDataContainer GetItem(int i)
    {
        return items[i];
    }
}
