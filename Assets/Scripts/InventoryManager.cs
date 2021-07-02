using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(-10)]
public class InventoryManager : MonoBehaviour
{
    [SerializeField] List<ItemPackage> startingItems;
    [SerializeField] InventorySlot slotPrefab;
    [SerializeField] TMP_Text currentItemText;
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

    bool savable;

    void Awake()
    {
        bool newPlayer = true;
        savable = (SaveData.currentPlayer != null);

        if (savable)
        {
            newPlayer = (SaveData.currentPlayer.inventoryItems.Count == 0);
        }

        slotMap = new InventorySlot[inventorySize.x, inventorySize.y];

        for (int y = 0; y < inventorySize.y; y++)
        {
            for (int x = 0; x < inventorySize.x; x++)
            {
                slotMap[x, y] = Instantiate(slotPrefab, transform);

                if (newPlayer)
                {
                    if (startingItems.Count > 0)
                    {
                        slotMap[x, y].AddItem(startingItems[0].count, startingItems[0].item);
                        startingItems.RemoveAt(0);
                    }
                }
            }
        }

        if (savable && !newPlayer)
        {
            LoadItems();
        }

        extendedInventoryHolder = new GameObject("Extended Inventory Holder");
        extendedInventoryHolder.transform.SetParent(transform);

        ScrollSlot(0);
        //LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        //ToggleExtendedInventory();
        Invoke("ToggleExtendedInventory", .2f);
    }

    public void Save()
    {
        if (!savable) { return; }

        print("Saving Inventory");

        List<ItemPackage> items = new List<ItemPackage>();

        for (int y = 0; y < inventorySize.y; y++)
            for (int x = 0; x < inventorySize.x; x++)
            {
                items.Add(slotMap[x, y].itemPackage);
            }

        SaveData.currentPlayer.Save(items);
    }

    void LoadItems()
    {
        for (int y = 0; y < inventorySize.y; y++)
            for (int x = 0; x < inventorySize.x; x++)
            {
                ItemPackage package = SaveData.currentPlayer.inventoryItems[x + (y * inventorySize.x)];
                slotMap[x, y].AddItem(package.count, package.item);
            }
    }

    private void OnApplicationQuit()
    {
        Save();
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

        var newSlot = slotMap[selectedSlotIndex, 0];

        newSlot.SelectSlot();

        if (newSlot.itemPackage.item)
        {
            currentItemText.text = newSlot.itemPackage.item.itemName;
        }
        else
        {
            currentItemText.text = "";
        }
    }

    void Update()
    {
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
