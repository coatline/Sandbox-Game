using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

interface IItemHolder
{
    ItemPackage CurrentItemPackage { get; set; }

    int CanModifyCount(int count);

    void UpdateCountText();

    void UpdateImage();

    int TryModifyItem(ItemPackage itemPackage, float addCount = .1f, bool invertCount = false);
}

public class SlotUI : MonoBehaviour, IItemHolder
{
    public TMP_Text countText;
    public Image backgroundSr;
    public Image itemImageSr;

    public Color normalBackgroundColor;
    public Color filledBackgroundColor;
    public Vector2 scaleOnSelect;

    ItemPackage itemPackage;

    public ItemPackage CurrentItemPackage
    {
        get
        {
            return itemPackage;
        }
        set
        {
            CurrentItem = value.item;
            CurrentCount = value.count;
        }
    }

    public ItemDataContainer CurrentItem
    {
        get
        {
            return itemPackage.item;
        }
        set
        {
            itemPackage.item = value;
            UpdateImage();
        }
    }

    public int CurrentCount
    {
        get
        {
            return itemPackage.count;
        }
        set
        {
            itemPackage.count = value;

            if (itemPackage.count <= 0)
            {
                CurrentItem = null;
                itemPackage.count = 0;
            }

            UpdateCountText();
        }
    }

    public virtual void Start()
    {
        UpdateImage();
    }

    public virtual void UpdateImage()
    {
        if (itemPackage.item != null)
        {
            itemImageSr.sprite = itemPackage.item.itemData.itemSprite;
            backgroundSr.color = filledBackgroundColor;
            itemImageSr.enabled = true;
        }
        else
        {
            backgroundSr.color = normalBackgroundColor;
            itemImageSr.enabled = false;
        }
    }

    public void ClearItem()
    {
        CurrentCount = 0;
        CurrentItem = null;
    }

    public int TryModifyItem(ItemPackage itemPackage, float addCount = 0.1f, bool invertCount = false)
    {
        if (invertCount) { if (addCount != .1f) { addCount = -addCount; } else { itemPackage.count = -itemPackage.count; } }

        int ov = CurrentItemPackage.TryModifyItem(itemPackage, out ItemPackage newP, addCount);
        CurrentItemPackage = newP;
        return ov;
    }

    public void UpdateCountText()
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

    public void SetItem(ItemPackage itemPackage)
    {
        this.CurrentItemPackage = itemPackage;
    }

    public int CanModifyCount(int count)
    {
        return Extensions.OverflowOnAdd(count, CurrentCount, CurrentItem.itemData.maxStack);
    }
}
