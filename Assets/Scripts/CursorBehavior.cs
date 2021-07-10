using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CursorBehavior : MonoBehaviour
{
    Image image;
    TMP_Text countText;

    public ItemPackage itemPackage;

    void Start()
    {
        image = GetComponent<Image>();
        countText = GetComponentInChildren<TMP_Text>();
    }

    private void Update()
    {
        //var mousePosition = mainCam.ScreenToWorldPoint(Input.mousePosition) + new Vector3(0,0,10);
        transform.position = Input.mousePosition;
    }

    void UpdateText()
    {
        if(itemPackage.count > 1)
        {
            countText.text = itemPackage.count.ToString();
        }
        else
        {
            countText.text = "";
        }
    }

    public void UseItem(int count)
    {
        this.itemPackage.count -= count;

        if(this.itemPackage.count <= 0)
        {
            RemoveItem();
            return;
        }

        UpdateText();
    }

    public void TakeItem(ItemPackage newItem)
    {
        this.itemPackage.count = newItem.count;
        image.enabled = true;
        itemPackage.item = newItem.item;
        image.sprite = itemPackage.item.itemData.itemSprite;
        UpdateText();
    }

    public void RemoveItem()
    {
        image.enabled = false;
        itemPackage.count = 0;
        itemPackage.item = null;
        UpdateText();
    }
}
