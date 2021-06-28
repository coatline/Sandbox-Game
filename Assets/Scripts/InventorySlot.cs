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
    public int count;
    bool selected;

    private void Start()
    {
        if (!selected)
        {
            backgroundSr.color = normalBackgroundColor;
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

        if (this.count > 1)
        {
            countText.text = this.count.ToString();
        }

        this.item = item;
        itemImageSr.enabled = true;
        itemImageSr.sprite = item.itemSprite;

        if (!selected)
        {
            backgroundSr.color = filledBackgroundColor;
        }
    }

    public void RemoveItem(int count)
    {
        this.count -= count;

        if (this.count > 1)
        {
            countText.text = this.count.ToString();
        }
        else if (this.count == 1)
        {
            countText.text = "";
        }
        else if (this.count <= 0)
        {
            item = null;
            itemImageSr.enabled = false;
            backgroundSr.color = normalBackgroundColor;
        }
    }

    public void TakeItem()
    {
        itemImageSr.enabled = false;
        backgroundSr.color = normalBackgroundColor;
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
