using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

[DefaultExecutionOrder(-2000)]
public class DataManager : MonoBehaviour
{
    [SerializeField] ItemDataContainer[] items;
    [SerializeField] Structure[] structures;
    static DataManager instance;

    public static DataManager D
    {
        get
        {
            if (instance == null)
            {
                instance = new DataManager();
            }

            return instance;
        }
    }

    //ItemDataContainer[] items;

    Dictionary<string, Structure> structureFromName;
    Dictionary<string, short> itemsIDfromName;

    private void Awake()
    {
        if (!instance)
        {
            instance = this;
        }
        else if (instance == this) { return; }
        else if (this != instance)
        {
            Destroy(gameObject);
            return;
        }

        structureFromName = new Dictionary<string, Structure>();
        itemsIDfromName = new Dictionary<string, short>();

        int tiles = 0;
        int weapons = 0;
        int tools = 0;

        //for (int p = 0; p < items.Length; p++)
        //{
        //    if (items[p].name == "Nothing")
        //    {
        //        var z = items[0];
        //        items[0] = items[p];
        //        items[p] = z;
        //        break;
        //    }
        //}

        for (int j = 0; j < this.items.Length; j++)
        {
            var item = this.items[j];
            //item.itemData.id = (short)(j);

            itemsIDfromName.Add(item.name, (short)(j));

            if (item.itemType == ItemType.block)
            {
                tiles++;
            }
            else if (item.itemType == ItemType.tool)
            {
                tools++;
            }
            else if (item.itemType == ItemType.rangedWeapon || item.itemType == ItemType.meleeWeapon)
            {
                weapons++;
            }
        }

        for (int j = 0; j < structures.Length; j++)
        {
            structures[j]._name = structures[j].name;
            structureFromName.Add(structures[j]._name, structures[j]);
        }

        //Debug.Log($"Database updated! It now contains {itemIDfromName.Count} items.\n{tiles} of them are tiles. {weapons} of them are weapons. {tools} of them are tools.");
        //DontDestroyOnLoad(this.gameObject);
    }

    public Structure GetStructure(string n)
    {
        return structureFromName[n];
    }

    public short GetItemID(string n)
    {
        return itemsIDfromName[n];
    }

    public TileBase GetTile(int id)
    {
        if (id == 0) { return null; }
        //if (items[id].itemType != ItemType.block) { print(items[id].name); }
        return items[id].tileData.tile;
    }

    public ItemDataContainer GetItem(string n)
    {
        return items[itemsIDfromName[n]];
    }

    public ItemDataContainer GetItem(int i)
    {
        return items[i];
    }
}
