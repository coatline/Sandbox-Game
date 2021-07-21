using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    }

    public void TrySavePlayer(PlayerSaveData psd)
    {
        if (psd == null) { return; }

        List<ItemPackage> items = new List<ItemPackage>();

        for (int y = 0; y < im.inventorySize.y; y++)
            for (int x = 0; x < im.inventorySize.x; x++)
            {
                items.Add(im.slotMap[x, y].CurrentItemPackage);
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
