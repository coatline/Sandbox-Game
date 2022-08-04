using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChestInventory : MonoBehaviour
{
    [SerializeField] InventorySlot slotPrefab;
    [SerializeField] GridLayoutGroup glg;
    [SerializeField] InventoryManager im;
    [SerializeField] ChestManager cm;
    InventorySlot[,] slotMap;
    Vector2Int inventorySize;
    Vector2Int currentData;
    public bool open;

    void Start()
    {
        inventorySize = new Vector2Int(ChestManager.INVENTORY_SIZE_X, ChestManager.INVENTORY_SIZE_Y);

        slotMap = new InventorySlot[inventorySize.x, inventorySize.y];
        currentData = -Vector2Int.one;

        for (int y = 0; y < inventorySize.y; y++)
        {
            for (int x = 0; x < inventorySize.x; x++)
            {
                slotMap[x, y] = Instantiate(slotPrefab, glg.transform);
            }
        }
    }

    public void OpenWithData(ChestData data)
    {
        if (open)
        {
            if (currentData == data.pos)
            {
                im.ToggleExtendedInventory(); return;
            }
            else
            {
                CloseChest();
            }
        }

        currentData = data.pos;

        for (int y = 0; y < inventorySize.y; y++)
        {
            for (int x = 0; x < inventorySize.x; x++)
            {
                if (data.items[x + (y * inventorySize.x)].item != null)
                {
                    slotMap[x, y].TryModifyItem(data.items[x + (y * inventorySize.x)]);
                }
            }
        }

        if (!open)
        {
            open = true;
            glg.gameObject.SetActive(true);

            if (!im.canEditInventory)
            {
                im.ToggleExtendedInventory();
            }
        }
    }

    public void CloseChest()
    {
        if (!open) { return; }

        cm.SaveChestAsSlots(currentData, slotMap);

        open = false;
        glg.gameObject.SetActive(false);
    }

    //I SHOULD HOOK UP ALL THINGS THAT NEED TO KNOW IF THE INVENTORY IS OPEN OR NOT TO AN EVENT THAT IT SENDS OUT

    private void Update()
    {
        if (!im.canEditInventory && open)
        {
            CloseChest();
        }
    }
}
