using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CursorBehavior : MonoBehaviour
{
    Image image;
    TMP_Text countText;

    public ItemDataContainer currentItem;
    public int count;

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
        if(count > 1)
        {
            countText.text = count.ToString();
        }
        else
        {
            countText.text = "";
        }
    }

    public void TakeItem(ItemDataContainer newItem, int count)
    {
        this.count = count;
        image.enabled = true;
        currentItem = newItem;
        image.sprite = currentItem.itemSprite;
        UpdateText();
    }

    public void RemoveItem()
    {
        currentItem = null;
        image.enabled = false;
        count = 0;
        UpdateText();
    }

    //Class itempackage
    //holds itemdata and count for that data
    //
}
