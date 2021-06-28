using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [SerializeField] InventorySlot slotPrefab;
    [SerializeField] TMP_Text cursorText;
    [SerializeField] Vector2Int inventorySize;
    GameObject extendedInventoryHolder;
    InventorySlot[,] slotMap;
    int selectedSlotIndex;
    bool started;

    public InventorySlot CurrentSlot()
    {
        return slotMap[selectedSlotIndex, 0];
    }

    public ItemDataContainer CurrentItem()
    {
        return slotMap[selectedSlotIndex, 0].item;
    }

    void Awake()
    {
        slotMap = new InventorySlot[inventorySize.x, inventorySize.y];

        for (int y = 0; y < inventorySize.y; y++)
        {
            for (int x = 0; x < inventorySize.x; x++)
            {
                slotMap[x, y] = Instantiate(slotPrefab, transform);
            }
        }

        extendedInventoryHolder = new GameObject("Extended Inventory Holder");
        extendedInventoryHolder.transform.SetParent(transform);

        ScrollSlot(0);
    }

    public void AddItem(ItemDataContainer item, int count)
    {
        bool seenEmpty = false;
        Vector2Int emptyIndex = Vector2Int.zero;

        for (int y = 0; y < inventorySize.y; y++)
        {
            for (int x = 0; x < inventorySize.x; x++)
            {
                InventorySlot slot = slotMap[x, y];

                if (!seenEmpty && slot.item == null)
                {
                    emptyIndex = new Vector2Int(x, y);
                    seenEmpty = true;
                }

                if (slot.item == item && slot.count < slot.item.maxStack/*+ count*/)
                {
                    slot.AddItem(item, count);
                    return;
                }
            }
        }

        if (seenEmpty)
        {
            slotMap[emptyIndex.x, emptyIndex.y].AddItem(item, count);
        }
        else
        {
            //Drop rest of it
        }
    }

    void ToggleExtendedInventory()
    {
        extendedInventoryHolder.SetActive(!extendedInventoryHolder.activeSelf);

        if (extendedInventoryHolder.transform.childCount == 0)
        {
            for (int y = 1; y < inventorySize.y; y++)
            {
                for (int x = 0; x < inventorySize.x; x++)
                {
                    slotMap[x, y].transform.SetParent(extendedInventoryHolder.transform);
                }
            }
        }
    }

    void ScrollSlot(int direction)
    {
        slotMap[selectedSlotIndex, 0].DeSelectSlot();

        selectedSlotIndex += direction;

        if (selectedSlotIndex >= inventorySize.x) { selectedSlotIndex = 0; }
        else if (selectedSlotIndex < 0) { selectedSlotIndex = inventorySize.x - 1; }

        slotMap[selectedSlotIndex, 0].SelectSlot();
    }

    void Update()
    {


        //    for (int y = 0; y < inventorySize.y; y++)
        //    {
        //        for (int x = 0; x < inventorySize.x; x++)
        //        {
        //            slotMap[x, y].transform.SetParent(extendedInventoryHolder.transform);
        //        }
        //    }
        //}

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleExtendedInventory();
        }

        var scrollInput = Input.mouseScrollDelta.y;

        if (scrollInput < 0)
        {
            // Scroll Up

            ScrollSlot(1);
        }
        else if (scrollInput > 0)
        {
            // Scroll Down

            ScrollSlot(-1);
        }
    }
}
