using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : SelectableSlot
{
    //TODO Animate Select and Deselect
    CursorBehavior cursor;
    InventoryManager im;

    private void Start()
    {
        im = FindObjectOfType<InventoryManager>();
        cursor = FindObjectOfType<CursorBehavior>();
    }

    public void InteractWithCursor()
    {
        if (!im.canEditInventory) { return; }

        if (cursor.itemPackage.item)
        {
            if (itemPackage.item != null)
            {
                if (cursor.itemPackage.item == itemPackage.item)
                {
                    // Combine items

                    AddItem(cursor.itemPackage.count);
                    cursor.RemoveItem();
                }
                else
                {
                    // Swap items
                    //print($"Swapping items with cursor. I give {itemPackage.count} {itemPackage.item.itemName}, it give me {cursor.itemPackage.count} {cursor.itemPackage.item.itemName}");

                    ItemPackage oldItemPackage = new ItemPackage(itemPackage.item, itemPackage.count);

                    SetItem(cursor.itemPackage);

                    cursor.RemoveItem();
                    cursor.TakeItem(oldItemPackage);
                }
            }
            else
            {
                // Take item

                SetItem(cursor.itemPackage);
                //print($"I just took {itemPackage.count} of {itemPackage.item} from the cursor");
                cursor.RemoveItem();
            }
        }
        else
        {
            if (itemPackage.item != null)
            {
                // Give item
                //print($"Giving my items to cursor");
                cursor.TakeItem(itemPackage);
                ClearItem();
            }
            else
            {
                // Do nothing
            }
        }
    }
}
