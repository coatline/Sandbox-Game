using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : SelectableSlot, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    //TODO Animate Select and Deselect
    CursorBehavior cursor;
    InventoryManager im;
    bool rightMouseDown;

    public override void Start()
    {
        base.Start();

        im = FindObjectOfType<InventoryManager>();
        cursor = FindObjectOfType<CursorBehavior>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            LeftClickOnSlot();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            rightMouseDown = true;
            StartCoroutine(OnRightMouse());
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        rightMouseDown = false;
    }

    IEnumerator OnRightMouse()
    {
        RightClickOnSlot();
        yield return new WaitForSeconds(.65f);

        while (rightMouseDown)
        {
            if (rightMouseDown)
            {
                RightClickOnSlot();
            }

            yield return new WaitForSeconds(.04f);
        }
    }

    void SwapItemsWithCursor()
    {
        ItemPackage oldItemPackage = CurrentItemPackage;

        SetItem(cursor.CurrentItemPackage);

        cursor.ClearItem();
        cursor.ChangeItem(oldItemPackage);
    }

    void CombineItemsIntoSlot()
    {
        int overflow = TryModifyItem(cursor.CurrentItemPackage);
        cursor.TryModifyItem(CurrentItemPackage, cursor.CurrentCount - overflow, true);
    }

    //void GiveCountToCursor(int count)
    //{
    //    var overFlow = cursor.CanModifyCount(count);
    //    if () { return; }
    //    if (cursor.CanModify)
    //        cursor.AddItem(new ItemPackage());
    //    cursor.AddItem(1, cursor.itemPackage.item);
    //    RemoveCount(1);
    //}

    void TakeItemFromCursor(float count = .1f)
    {
        // Add to slot
        TryModifyItem(cursor.CurrentItemPackage, count);

        // Remove from cursor
        if (count == .1f)
        {
            cursor.ClearItem();
        }
        else
        {
            cursor.TryModifyItem(CurrentItemPackage, count, true);
        }
    }

    void GiveItemToCursor(float count = .1f)
    {
        // Add to cursor
        cursor.TryModifyItem(CurrentItemPackage, count);

        // Remove from slot
        if (count == .1f)
        {
            ClearItem();
        }
        else
        {
            TryModifyItem(cursor.CurrentItemPackage, count, true);
        }
    }

    void RightClickOnSlot()
    {
        im.ScrollSlot(0);

        if (cursor.CurrentItem)
        {
            //if (CurrentItem)
            {
                if (cursor.CurrentItem == CurrentItem || CurrentItem == null)
                {
                    // They are the same
                    // Take one from the cursor
                    TakeItemFromCursor(1);
                }
            }
            //else
            //{
            //    // Cursor has something I have nothing
            //    // Take one from the cursor
            //    TryTakeCountFromCursor(1);
            //}
        }
        else
        {
            if (CurrentItem)
            {
                // I have something Cursor has nothing
                // Give one to the cursor
                GiveItemToCursor((int)(CurrentCount / 2));
            }
        }
    }

    void LeftClickOnSlot()
    {
        im.ScrollSlot(0);

        if (cursor.CurrentItem)
        {
            if (CurrentItem)
            {
                if (cursor.CurrentItem == CurrentItem)
                {
                    // They are the same 
                    // Try to combine them
                    // Give cursor leftovers
                    CombineItemsIntoSlot();
                }
                else
                {
                    SwapItemsWithCursor();
                }
            }
            else
            {
                // I have nothing and cursor has something
                // Take item from cursor
                TakeItemFromCursor();
            }
        }
        else
        {
            if (CurrentItem)
            {
                // Cursor has nothing I have something
                // Give cursor my item
                GiveItemToCursor();
            }
            else
            {
                // Neither of us have anything to give
                // Do nothing at all
            }
        }
    }

}
