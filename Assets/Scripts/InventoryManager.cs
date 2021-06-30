using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    [SerializeField] List<ItemPackage> startingItems;
    [SerializeField] InventorySlot slotPrefab;
    [SerializeField] TMP_Text cursorText;
    [SerializeField] Vector2Int inventorySize;
    GameObject extendedInventoryHolder;
    public bool canEditInventory;
    InventorySlot[,] slotMap;
    int selectedSlotIndex;
    bool started;

    public InventorySlot CurrentSlot()
    {
        return slotMap[selectedSlotIndex, 0];
    }

    public ItemPackage CurrentItemPackage()
    {
        return slotMap[selectedSlotIndex, 0].itemPackage;
    }

    void Awake()
    {
        slotMap = new InventorySlot[inventorySize.x, inventorySize.y];

        for (int y = 0; y < inventorySize.y; y++)
        {
            for (int x = 0; x < inventorySize.x; x++)
            {
                slotMap[x, y] = Instantiate(slotPrefab, transform);
                
                if(startingItems.Count > 0)
                {
                    slotMap[x, y].AddItem(startingItems[0].count, startingItems[0].item);
                    startingItems.RemoveAt(0);
                }
            }
        }

        extendedInventoryHolder = new GameObject("Extended Inventory Holder");
        extendedInventoryHolder.transform.SetParent(transform);

        ScrollSlot(0);
        //LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        //ToggleExtendedInventory();
        Invoke("ToggleExtendedInventory", .05f);
    }

    public void AddItem(ItemPackage newItemPackage)
    {
        bool seenEmpty = false;
        Vector2Int emptyIndex = Vector2Int.zero;

        for (int y = 0; y < inventorySize.y; y++)
        {
            for (int x = 0; x < inventorySize.x; x++)
            {
                InventorySlot slot = slotMap[x, y];

                if (!seenEmpty && slot.itemPackage.item == null)
                {
                    emptyIndex = new Vector2Int(x, y);
                    seenEmpty = true;
                }

                if (slot.itemPackage.item == newItemPackage.item && slot.itemPackage.count < slot.itemPackage.item.maxStack + newItemPackage.count)
                {
                    // Stack the items into one slot
                    slot.AddItem(newItemPackage.count);
                    return;
                }
            }
        }

        if (seenEmpty)
        {
            slotMap[emptyIndex.x, emptyIndex.y].AddItem(newItemPackage.count, newItemPackage.item);
        }
        else
        {
            //Drop rest of it
        }
    }

    void ToggleExtendedInventory()
    {
        extendedInventoryHolder.SetActive(!extendedInventoryHolder.activeSelf);
        canEditInventory = extendedInventoryHolder.activeSelf;

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
