using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CursorBehavior : MonoBehaviour, IItemHolder
{
    [SerializeField] Image displayPlacementImage;
    bool displayingPlacement;
    ItemPackage itemPackage;
    TMP_Text countText;
    Image image;
    Camera cam;

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

    public void UpdateImage()
    {
        if (itemPackage.item)
        {
            image.enabled = true;
            image.sprite = itemPackage.item.itemData.itemSprite;
        }
        else
        {
            image.enabled = false;
        }
    }

    void Start()
    {
        image = GetComponent<Image>();
        countText = GetComponentInChildren<TMP_Text>();
        cam = Camera.main;
    }

    private void Update()
    {
        //var mousePosition = mainCam.ScreenToWorldPoint(Input.mousePosition) + new Vector3(0,0,10);
        transform.position = Input.mousePosition;

        var blockPos = cam.ScreenToWorldPoint(Input.mousePosition);

        displayPlacementImage.transform.position = new Vector3((int)blockPos.x, (int)blockPos.y);

        if (displayingPlacement)
        {
            if (image.enabled)
            {
                displayPlacementImage.enabled = false;
            }
            else
            {
                displayPlacementImage.enabled = true;
            }
        }
    }

    public void UpdateCountText()
    {
        if (itemPackage.count > 1)
        {
            countText.text = itemPackage.count.ToString();
        }
        else
        {
            countText.text = "";
        }
    }

    public void DisplayPlacement(Sprite sprite)
    {
        displayPlacementImage.enabled = true;
        displayPlacementImage.sprite = sprite;
        displayPlacementImage.preserveAspect = true;
        displayingPlacement = true;
    }

    public void EndDisplayPlacement()
    {
        displayPlacementImage.enabled = false;
        displayPlacementImage.sprite = null;
        displayingPlacement = false;
    }

    public int CanModifyCount(int count)
    {
        return Extensions.OverflowOnAdd(count, CurrentCount, CurrentItem.itemData.maxStack);
    }

    public void ChangeItem(ItemPackage newItem)
    {
        CurrentItemPackage = newItem;
    }

    public void ClearItem()
    {
        CurrentItem = null;
        CurrentCount = 0;
    }

    public int TryModifyItem(ItemPackage itemPackage, float addCount = 0.1f, bool invertCount = false)
    {
        if (invertCount) { if (addCount != .1f) { addCount = -addCount; } else { itemPackage.count = -itemPackage.count; } }

        int ov = CurrentItemPackage.TryModifyItem(itemPackage, out ItemPackage newP, addCount);
        CurrentItemPackage = newP;

        return ov;
    }
}

///// <summary>
///// Returns the absolute value of the remainder count after reaching 0 or max stack.
///// If 0, you may add it.
///// Otherwise if -1, they are different items or count is less than 0
///// </summary>
///// <param name="count"></param>
///// <returns></returns>
//public int TryAddItem(ItemPackage item)
//{
//    if (item.count <= 0) { print($"Why am i trying to add {item.count} {item.item}s to the cursor?"); return -1; }

//    if (CurrentItem == null)
//    {
//        CurrentItemPackage = item;
//        return 0;
//    }
//    else
//    {
//        if (CurrentItem == item.item)
//        {
//            var overFlow = CanModifyCount(CurrentCount);

//            if (overFlow == 0)
//            {
//                // No overflow so you can add
//                CurrentCount += item.count;
//                return 0;
//            }
//            else if (overFlow > 0)
//            {
//                // Overflow! Set count to max stack and return leftovers
//                CurrentCount = itemPackage.item.itemData.maxStack;
//                return overFlow;
//            }
//            else
//            {
//                print("why is overflow less than 0?>?????");
//            }
//        }
//        else
//        {
//            // Different items can not add them
//            return -1;
//        }
//    }

//    print($"Why did I get here? {item.count} {item.item}");
//    return -1;
//}