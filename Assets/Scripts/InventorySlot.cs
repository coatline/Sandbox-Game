using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    //TODO Animate Select and Deselect

    [SerializeField] Image backgroundSr;
    [SerializeField] Image itemImageSr;
    [SerializeField] TMP_Text countText;

    [SerializeField] Color normalBackgroundColor;
    [SerializeField] Color filledBackgroundColor;
    [SerializeField] Color selectedBackgroundColor;
    [SerializeField] Vector2 scaleOnSelect;
    public ItemPackage itemPackage;
    CursorBehavior cursor;
    InventoryManager im;
    bool selected;

    private void Start()
    {
        im = FindObjectOfType<InventoryManager>();
        cursor = FindObjectOfType<CursorBehavior>();

        if (!selected)
        {
            backgroundSr.color = normalBackgroundColor;
        }
    }

    public void InteractWithCursor()
    {
        if (!im.canEditInventory) { return; }

        if (cursor.itemPackage.item)
        {
            if (itemPackage.item != null)
            {
                if (cursor.itemPackage == itemPackage)
                {
                    // Combine items

                    AddItem(cursor.itemPackage.count);
                    cursor.RemoveItem();
                }
                else
                {
                    // Swap items

                    ItemPackage oldItemPackage = itemPackage;

                    ChangeItem(cursor.itemPackage);

                    cursor.RemoveItem();
                    cursor.TakeItem(oldItemPackage);
                }
            }
            else
            {
                // Take item

                ChangeItem(cursor.itemPackage);

                cursor.RemoveItem();
            }
        }
        else
        {
            if (itemPackage.item != null)
            {
                // Give item

                cursor.TakeItem(itemPackage);
                ClearItem();
            }
            else
            {
                // Do nothing
            }
        }
    }

    void ChangeItem(ItemPackage itemPackage)
    {
        this.itemPackage = itemPackage;

        UpdateImage();
    }

    void UpdateImage()
    {
        if (itemPackage.item != null)
        {
            itemImageSr.sprite = itemPackage.item.itemSprite;
            itemImageSr.enabled = true;

            if (!selected)
            {
                backgroundSr.color = filledBackgroundColor;
            }
        }
        else
        {
            itemImageSr.enabled = false;

            if (!selected)
            {
                backgroundSr.color = normalBackgroundColor;
            }
        }

        UpdateCountText();
    }

    void ClearItem()
    {
        itemPackage.item = null;
        itemPackage.count = 0;

        UpdateImage();
    }

    void UpdateCountText()
    {
        if (this.itemPackage.count > 1)
        {
            countText.text = this.itemPackage.count.ToString();
        }
        else
        {
            countText.text = "";
        }
    }

    public void AddItem(int count, ItemDataContainer newItem = null)
    {
        if (itemPackage.item == null)
        {
            itemPackage = new ItemPackage(newItem, count);
        }
        else
        {
            this.itemPackage.count += count;

            if (this.itemPackage.count + count > itemPackage.item.maxStack)
            {
                //drop the rest of it
                this.itemPackage.count = itemPackage.item.maxStack;
            }
        }

        UpdateImage();
    }

    public void RemoveItem(int count)
    {
        this.itemPackage.count -= count;

        if (this.itemPackage.count <= 0)
        {
            ClearItem();
        }
        else
        {
            UpdateImage();
        }
    }

    public void SelectSlot()
    {
        backgroundSr.color = selectedBackgroundColor;
        transform.localScale = new Vector3(scaleOnSelect.x, scaleOnSelect.y, 0);
        selected = true;
    }

    public void DeSelectSlot()
    {
        if (itemPackage.item != null)
        {
            backgroundSr.color = filledBackgroundColor;
        }
        else
        {
            backgroundSr.color = normalBackgroundColor;
        }

        selected = false;
        transform.localScale = Vector3.one;
    }
}
