using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


[DefaultExecutionOrder(1)]
public class InventoryManager : MonoBehaviour
{
    [SerializeField] List<ItemPackage> startingItems;

    //[SerializeField] delegate inventoryToggled;
    [SerializeField] UnityEvent inventoryToggled;
    //[SerializeField] UnityEvent inventoryToggled;
    [SerializeField] InventorySlot slotPrefab;
    [SerializeField] TMP_Text currentItemText;
    GameObject extendedInventoryHolder;
    public Vector2Int inventorySize;
    public InventorySlot[,] slotMap;
    public bool canEditInventory;
    int selectedSlotIndex;
    Player player;
    bool savable;
    bool newPlayer;

    void Awake()
    {
        newPlayer = true;
        savable = (GD.currentPlayer != null);

        if (savable)
        {
            newPlayer = (GD.currentPlayer.inventoryItems.Count == 0);
        }

        if (slotMap == null)
        {
            InstantiateSlots();
        }

        extendedInventoryHolder = new GameObject("Extended Inventory Holder");
        extendedInventoryHolder.transform.SetParent(transform);

        ScrollSlot(0);
        Invoke("ToggleExtendedInventory", .25f);
    }

    public void InstantiateSlots()
    {
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
                        slotMap[x, y].CurrentItemPackage = startingItems[0];
                        startingItems.RemoveAt(0);
                    }
                }
            }
        }
    }

    private void Start()
    {
        player = FindObjectOfType<Player>();
        player.SetItemAndSlot(slotMap[selectedSlotIndex, 0].CurrentItemPackage, slotMap[selectedSlotIndex, 0]);
    }

    //List<KeyValuePair<ItemDataContainer, InventorySlot>> inventoryItem = new List<KeyValuePair<ItemDataContainer, InventorySlot>>();

    public List<InventorySlot> CanCraft(RecipeData recipe)
    {
        int i = recipe.ingredients.Length;

        List<InventorySlot> slotsWithIngredients = new List<InventorySlot>();

    NextIngredient:

        i--;

        if (i < 0) { return slotsWithIngredients; }

        for (int x = 0; x < inventorySize.x; x++)
            for (int y = 0; y < inventorySize.y; y++)
            {
                if (slotMap[x, y].CurrentItem == recipe.ingredients[i].item && Extensions.CanAddItem(slotMap[x, y].CurrentItemPackage, recipe.ingredients[i], true) == 0)
                {
                    slotsWithIngredients.Add(slotMap[x, y]);
                    goto NextIngredient;
                }
            }

        return null;
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

                if (!seenEmpty && slot.CurrentItem == null)
                {
                    emptyIndex = new Vector2Int(x, y);
                    seenEmpty = true;
                    continue;
                }

                int overflow = slot.TryModifyItem(newItemPackage);

                if (overflow == -1)
                {
                    continue;
                }
                else if (overflow == 0)
                {
                    return;
                }
                else
                {
                    newItemPackage.count -= newItemPackage.count - overflow;
                    continue;
                }

            }
        }

        if (seenEmpty)
        {
            slotMap[emptyIndex.x, emptyIndex.y].TryModifyItem(newItemPackage);
        }
        else
        {
            //Drop rest of it
        }
    }

    public void ToggleExtendedInventory()
    {
        extendedInventoryHolder.SetActive(!extendedInventoryHolder.activeSelf);
        canEditInventory = extendedInventoryHolder.activeSelf;

        inventoryToggled.Invoke();

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

    public void ScrollSlot(int direction)
    {
        slotMap[selectedSlotIndex, 0].DeSelectSlot();

        selectedSlotIndex += direction;

        if (selectedSlotIndex >= inventorySize.x) { selectedSlotIndex = 0; }
        else if (selectedSlotIndex < 0) { selectedSlotIndex = inventorySize.x - 1; }

        var newSlot = slotMap[selectedSlotIndex, 0];

        newSlot.SelectSlot();

        if (newSlot.CurrentItem)
        {
            currentItemText.text = newSlot.CurrentItem.itemData.itemName;
        }
        else
        {
            currentItemText.text = "";
        }

        if (player)
        {
            player.SetItemAndSlot(slotMap[selectedSlotIndex, 0].CurrentItemPackage, slotMap[selectedSlotIndex, 0]);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleExtendedInventory();
        }

        if (!canEditInventory)
        {
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
}
