using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChestManager : MonoBehaviour
{
    [System.Serializable]
    public struct SpawnItem
    {
        public ItemPackage item;
        public int spawnWeight;
    }

    public const int INVENTORY_SIZE_X = 9;
    public const int INVENTORY_SIZE_Y = 4;

    [SerializeField] SpawnItem[] primaryItems;
    [SerializeField] SpawnItem[] secondaryItems;

    public ChestData CreateNewChestDataAt(int x, int y, bool generateItems = false)
    {
        ChestData chest = NewChestData(x, y, generateItems);
        GD.wd.chests.Add(new Vector2Int(x, y), chest);
        return chest;
    }

    public List<ItemPackage> GetChestContentsAt(int x, int y)
    {
        List<ItemPackage> items = new List<ItemPackage>();
        var citems = GD.wd.chests[new Vector2Int(x, y)].items;

        for (int i = 0; i < citems.Length; i++)
        {
            if (citems[i].item)
            {
                items.Add(citems[i]);
            }
        }

        return items;
    }

    ChestData NewChestData(int x, int y, bool generateItems = false)
    {
        if (generateItems)
        {
            ChestData cd = new ChestData(new ItemPackage[INVENTORY_SIZE_X * INVENTORY_SIZE_Y], new Vector2Int(x, y));

            cd.AddItem(RandItem(primaryItems));

            for (int k = 0; k < 20; k++)
            {
                cd.AddItem(RandItem(secondaryItems));
            }

            return cd;
        }

        return new ChestData(new ItemPackage[INVENTORY_SIZE_X * INVENTORY_SIZE_Y], new Vector2Int(x, y));
    }

    public void SaveChestAsSlots(Vector2Int key, InventorySlot[,] slots)
    {
        // Could keep a list of changes made
        for (int y = 0; y < INVENTORY_SIZE_Y; y++)
            for (int x = 0; x < INVENTORY_SIZE_X; x++)
            {
                    GD.wd.chests[key].items[x + (y * INVENTORY_SIZE_X)] = slots[x, y].CurrentItemPackage;
                    slots[x, y].ClearItem();
            }
    }

    public ItemPackage RandItem(SpawnItem[] pool)
    {
        int total = primaryItems.Sum(a => a.spawnWeight);
        int threshold = Random.Range(0, total);

        for (int i = 0; i < pool.Length; i++)
        {
            if (pool[i].spawnWeight > threshold)
            {
                return pool[i].item;
            }
            else
            {
                threshold -= primaryItems[i].spawnWeight;
            }
        }

        //Debug.Log("Did not choose an item for some reason");
        return new ItemPackage();
    }
}
