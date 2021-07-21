using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CraftingManager : MonoBehaviour
{
    [SerializeField] AnimationCurve alphaDropoff;
    [SerializeField] CraftingSlot slotPrefab;
    [SerializeField] InventoryManager im;
    [SerializeField] List<RecipeData> recipes;
    [SerializeField] GridLayoutGroup glg;
    [SerializeField] RectTransform rt;
    [SerializeField] CursorBehavior c;
    List<CraftingSlot> craftableSlots;
    [Range(2, 8)]
    [SerializeField] int viewDist;
    int selectedSlotIndex;

    void Start()
    {
        craftableSlots = new List<CraftingSlot>();

        for (int i = 0; i < recipes.Count; i++)
        {
            var craftingSlot = Instantiate(slotPrefab, glg.transform);
            craftingSlot.recipe = recipes[i];
            craftableSlots.Add(craftingSlot);
            craftableSlots[i].TryModifyItem(recipes[i].product);
            craftingSlot.SetAlpha(0);
        }

        ScrollSlot(0);
    }

    public void TryCraft(RecipeData recipe)
    {
        List<InventorySlot> slots = im.CanCraft(recipe);

        if (slots != null)
        {
            for (int i = 0; i < recipe.ingredients.Length; i++)
            {
                slots[i].TryModifyItem(recipe.ingredients[i], .1f, true);
            }

            if (c.CurrentItem == null)
            {
                c.TryModifyItem(recipe.product);
            }
            else if (c.CurrentItem == recipe.product.item)
            {
                c.TryModifyItem(recipe.product);
            }
        }
    }

    void Update()
    {
        if (im.canEditInventory)
        {
            rt.gameObject.SetActive(true);

            var scrollInput = Input.mouseScrollDelta.y;

            if (scrollInput < 0)
            {
                // Scroll Up

                ScrollSlot(1);
            }
            else if (scrollInput > 0)
            {
                // Scroll Down

                ScrollSlot(-1);
            }
        }
        else
        {
            rt.gameObject.SetActive(false);
        }
    }

    int RelativeSlotIndex(int dir)
    {
        int newindex = selectedSlotIndex + dir;

        if (newindex >= craftableSlots.Count) { return newindex - craftableSlots.Count; }
        else if (newindex < 0) { return craftableSlots.Count + newindex; }
        else { return newindex; }
    }

    CraftingSlot SlotFromIndex(int index)
    {
        return craftableSlots[index];
    }

    void ScrollSlot(int direction)
    {
        craftableSlots[selectedSlotIndex].DeSelectSlot();

        selectedSlotIndex += direction;
        rt.localPosition += new Vector3(0, (direction * glg.cellSize.y) + (glg.spacing.y * direction));

        if (selectedSlotIndex >= craftableSlots.Count) { rt.localPosition -= new Vector3(0, craftableSlots.Count * (glg.cellSize.y + glg.spacing.y)); selectedSlotIndex = 0; }
        else if (selectedSlotIndex < 0) { rt.localPosition += new Vector3(0, craftableSlots.Count * (glg.cellSize.y + glg.spacing.y)); selectedSlotIndex = craftableSlots.Count - 1; }

        var newSlot = craftableSlots[selectedSlotIndex];

        newSlot.SelectSlot();

        SetSurroundingSlotAlphas();

        //if (newSlot.itemPackage.item)
        //{
        //    currentItemText.text = newSlot.itemPackage.item.itemName;
        //}
        //else
        //{
        //    currentItemText.text = "";
        //}
    }

    void SetSurroundingSlotAlphas()
    {
        for (int i = -viewDist; i <= viewDist; i++)
        {
            var slot = SlotFromIndex(RelativeSlotIndex(i));

            if (Mathf.Abs(RelativeSlotIndex(i) - selectedSlotIndex) > viewDist)
            {
                slot.SetAlpha(0);
                continue;
            }

            float a = 0;
            a = alphaDropoff.Evaluate(1 - ((float)Mathf.Abs(i) / (float)viewDist));
            slot.SetAlpha(a);
        }
    }
}
