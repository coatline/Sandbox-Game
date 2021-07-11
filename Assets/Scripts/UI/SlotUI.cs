using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SlotUI : MonoBehaviour
{
    [SerializeField] Image itemImageSr;
    [SerializeField] TMP_Text countText;
    public Image backgroundSr;

    public Color normalBackgroundColor;
    public Color filledBackgroundColor;
    public Color selectedBackgroundColor;
    public Vector2 scaleOnSelect;

    public ItemPackage itemPackage;

    private void Start()
    {
        UpdateImage();
    }

    public virtual void UpdateImage()
    {
        if (itemPackage.item != null)
        {
            itemImageSr.sprite = itemPackage.item.itemData.itemSprite;
            itemImageSr.enabled = true;
        }
        else
        {
            itemImageSr.enabled = false;
        }

        UpdateCountText();
    }

    public void ClearItem()
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
            //print($"I am recieving a totally new item {newItem.itemName} with {count}");
            itemPackage = new ItemPackage(newItem, count);
        }
        else
        {
            this.itemPackage.count += count;

            if (this.itemPackage.count + count > itemPackage.item.itemData.maxStack)
            {
                //drop the rest of it
                this.itemPackage.count = itemPackage.item.itemData.maxStack;
                print($"Overflow! Dropping {(this.itemPackage.count + count) - itemPackage.item.itemData.maxStack}");
            }
        }

        UpdateImage();
    }

    public void RemoveCount(int count)
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

    public void SetItem(ItemPackage itemPackage)
    {
        this.itemPackage.count = itemPackage.count;
        this.itemPackage.item = itemPackage.item;

        UpdateImage();
    }

}
