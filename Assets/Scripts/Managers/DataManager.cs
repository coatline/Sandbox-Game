using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

[DefaultExecutionOrder(-200)]
public class DataManager : MonoBehaviour
{
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

    ItemDataContainer[] items;
    Structure[] structures;

    Dictionary<string, Structure> structureFromName;
    Dictionary<string, short> itemIDfromName;

    private void Awake()
    {
        instance = this;
        structureFromName = new Dictionary<string, Structure>();
        itemIDfromName = new Dictionary<string, short>();

        int tiles = 0;
        int weapons = 0;
        int tools = 0;

        var it = AssetDatabase.FindAssets("t:ItemDataContainer");

        items = new ItemDataContainer[it.Length + 1];

        for (int j = 0; j < it.Length; j++)
        {
            items[j] = AssetDatabase.LoadAssetAtPath<ItemDataContainer>(AssetDatabase.GUIDToAssetPath(it[j]));
            var item = items[j];
            item.itemData.id = (short)(j);
            item.itemData.itemName = items[j].name;

            itemIDfromName.Add(item.itemData.itemName, (short)(j));

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

        var st = AssetDatabase.FindAssets("t:Structure");

        structures = new Structure[st.Length];

        for (int j = 0; j < st.Length; j++)
        {
            structures[j] = AssetDatabase.LoadAssetAtPath<Structure>(AssetDatabase.GUIDToAssetPath(st[j]));
            structures[j]._name = structures[j].name;
            structureFromName.Add(structures[j]._name, structures[j]);
        }

        //Debug.Log($"Database updated! It now contains {itemIDfromName.Count} items.\n{tiles} of them are tiles. {weapons} of them are weapons. {tools} of them are tools.");
        DontDestroyOnLoad(this.gameObject);
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
        if (id == 0) { return null; }
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
