using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class GD
{
    public static PlayerSaveData currentPlayer;
    public static WorldSaveData currentWorld;
    public static PlayerData pd;
    public static WorldData wd;
}

[DefaultExecutionOrder(-100)]
public class SaveManager : MonoBehaviour
{
    [SerializeField] InventoryManager im;
    [SerializeField] WorldGenerator wg;
    [SerializeField] ChestInventory ci;
    [SerializeField] bool inMenu;

    public void TrySaveAll()
    {
        TrySaveWorld(GD.currentWorld);
        TrySavePlayer(GD.currentPlayer);
    }

    void TryLoadAll()
    {
        if (GD.currentWorld == null)
        {
            GD.wd = new WorldData(100, 100);
        }
        else
        {
            GD.wd = GD.currentWorld.data;

            LoadWorld(GD.wd, GD.pd);
        }
    }

    void LoadWorld(WorldData worldData, PlayerData playerData)
    {
        wg.worldWidth = worldData.worldWidth;
        wg.worldHeight = worldData.worldHeight;

        if (GD.currentPlayer.inventoryItems.Count == 0) { return; }

        if (im.slotMap == null)
        {
            im.InstantiateSlots();
        }

        for (int x = 0; x < im.inventorySize.x; x++)
            for (int y = 0; y < im.inventorySize.y; y++)
            {
                string itemString = GD.currentPlayer.inventoryItems[x + (y * im.inventorySize.x)];

                if (itemString.Length == 0)
                {
                    continue;
                }

                im.slotMap[x, y].TryModifyItem(ItemPackageFromString(itemString));
            }
    }

    public void TrySavePlayer(PlayerSaveData psd)
    {
        if (psd == null) { return; }

        List<string> items = new List<string>();

        for (int y = 0; y < im.inventorySize.y; y++)
            for (int x = 0; x < im.inventorySize.x; x++)
            {
                if (im.slotMap[x, y].CurrentItem == null) { items.Add(""); continue; }

                items.Add($"{im.slotMap[x, y].CurrentCount} {im.slotMap[x, y].CurrentItem.name}");
            }

        psd.Save(items);
    }

    public void TrySaveWorld(WorldSaveData wsd)
    {
        if (wsd == null) { return; }

        if (!inMenu && ci.open)
        {
            ci.CloseChest();
        }

        wsd.Save(GD.wd);
    }

    ItemPackage ItemPackageFromString(string itemString)
    {
        int count = 0;
        int digits = 0;

        for (int i = 0; i < itemString.Length; i++)
        {
            if (char.IsNumber(itemString[i]))
            {
                digits++;
            }
            else
            {
                break;
            }
        }

        for (int i = 0; i < digits; i++)
        {
            count += (int)(Mathf.Pow(10, digits - 1 - i)) * (int)char.GetNumericValue(itemString[i]);
        }

        itemString = itemString.Remove(0, digits + 1);

        string itemName = itemString;

        ItemDataContainer item = DataManager.D.GetItem(itemName);

        ItemPackage package = new ItemPackage(item, count);

        if (package.count > 0)
        {
            if (package.item == null)
            {
                Debug.LogError("WHAT HAPPENED TO THIS ITEM");
            }
        }

        //slotMap[x, y].TryModifyItem(package);
        return package;
    }

    private void Awake()
    {
        if (!inMenu)
        {
            TryLoadAll();
        }
    }

    private void OnApplicationQuit()
    {
        if (inMenu) { return; }
        TrySaveAll();
    }
}
