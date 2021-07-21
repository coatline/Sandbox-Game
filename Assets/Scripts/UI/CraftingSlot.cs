using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CraftingSlot : SelectableSlot, IPointerUpHandler, IPointerDownHandler
{
    [SerializeField] Image outlineSr;
    public RecipeData recipe;
    CraftingManager cm;
    bool rightMouse;
    bool leftMouse;

    public override void Start()
    {
        cm = FindObjectOfType<CraftingManager>();
    }

    public void Craft()
    {
        cm.TryCraft(recipe);
    }

    IEnumerator OnRightMouse()
    {
        Craft();
        yield return new WaitForSeconds(.3f);

        while (rightMouse)
        {
            if (rightMouse)
            {
                Craft();
                yield return new WaitForSeconds(.03f);
            }
        }
    }

    IEnumerator OnLeftMouse()
    {
        Craft();
        yield return new WaitForSeconds(.75f);

        while (leftMouse)
        {
            if (leftMouse)
            {
                Craft();
                yield return new WaitForSeconds(.1f);
            }
        }
    }

    public void SetAlpha(float a)
    {
        backgroundSr.color = new Color(backgroundSr.color.r, backgroundSr.color.g, backgroundSr.color.b, a);
        outlineSr.color = new Color(outlineSr.color.r, outlineSr.color.g, outlineSr.color.b, a);
        itemImageSr.color = new Color(itemImageSr.color.r, itemImageSr.color.g, itemImageSr.color.b, a);
        countText.color = new Color(countText.color.r, countText.color.g, countText.color.b, a);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            rightMouse = false;
        }
        else
        {
            leftMouse = false;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (!leftMouse)
            {
                leftMouse = true;
                StartCoroutine(OnLeftMouse());
            }

        }
        else
        {
            if (!rightMouse)
            {
                rightMouse = true;
                StartCoroutine(OnRightMouse());
            }

        }
    }
}
