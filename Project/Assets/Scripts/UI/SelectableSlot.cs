using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectableSlot : SlotUI
{
    public Color selectedBackgroundColor;
    public bool selected;

    public override void Start()
    {
        UpdateImage();
    }

    public void SelectSlot()
    {
        backgroundSr.color = selectedBackgroundColor;
        transform.localScale = new Vector3(scaleOnSelect.x, scaleOnSelect.y, 0);
        selected = true;
    }

    public override void UpdateImage()
    {
        base.UpdateImage();

        if (selected)
        {
            backgroundSr.color = selectedBackgroundColor;
        }
    }

    public void DeSelectSlot()
    {
        if (CurrentItem!= null)
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
