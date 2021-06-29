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
    public ItemDataContainer item;
    CursorBehavior cursor;
    InventoryManager im;
    public int count;
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

        if (cursor.currentItem)
        {
            if (item)
            {
                if (cursor.currentItem == item)
                {
                    // Combine items

                    AddItem(cursor.currentItem, cursor.count);
                    cursor.RemoveItem();
                }
                else
                {
                    // Swap items

                    ItemDataContainer oldItem = item;
                    int oldCount = count;

                    ChangeItem(cursor.currentItem, cursor.count);

                    cursor.RemoveItem();
                    cursor.TakeItem(oldItem, oldCount);
                }
            }
            else
            {
                // Take item

                ChangeItem(cursor.currentItem, cursor.count);

                cursor.RemoveItem();
            }
        }
        else
        {
            if (item)
            {
                // Give item

                cursor.TakeItem(item, count);
                ClearItem();
            }
            else
            {
                // Do nothing
            }
        }
    }

    void ChangeItem(ItemDataContainer newItem, int count)
    {
        this.item = newItem;
        this.count = count;

        UpdateImage();
    }

    void UpdateImage()
    {
        if (item)
        {
            itemImageSr.sprite = item.itemSprite;
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
        item = null;
        count = 0;

        UpdateImage();
    }

    void UpdateCountText()
    {
        if (this.count > 1)
        {
            countText.text = this.count.ToString();
        }
        else
        {
            countText.text = "";
        }
    }

    public void AddItem(ItemDataContainer item, int count)
    {
        if (this.count + count > item.maxStack)
        {
            //drop the rest of it
            this.count = item.maxStack;
        }

        this.count += count;
        this.item = item;

        UpdateImage();
    }

    public void RemoveItem(int count)
    {
        this.count -= count;

        if (this.count <= 0)
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
        if (item != null)
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
