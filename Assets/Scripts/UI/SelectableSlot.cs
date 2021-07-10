using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectableSlot : SlotUI
{
    bool selected;

    public void SelectSlot()
    {
        backgroundSr.color = selectedBackgroundColor;
        transform.localScale = new Vector3(scaleOnSelect.x, scaleOnSelect.y, 0);
        selected = true;
    }

    public override void UpdateImage()
    {
        base.UpdateImage();

        if (!selected)
        {
            if (itemPackage.item != null)
            {
                backgroundSr.color = filledBackgroundColor;
            }
            else
            {
                backgroundSr.color = normalBackgroundColor;
            }
        }
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
