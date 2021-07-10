using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CraftingManager : MonoBehaviour
{
    [SerializeField] CraftingSlot slotPrefab;
    [SerializeField] InventoryManager im;
    [SerializeField] List<RecipeData> recipes;
    [SerializeField] GridLayoutGroup glg;
    [SerializeField] RectTransform rt;
    [SerializeField] CursorBehavior c;
    List<CraftingSlot> craftableSlots;
    int selectedSlotIndex;

    void Start()
    {
        craftableSlots = new List<CraftingSlot>();

        for (int i = 0; i < recipes.Count; i++)
        {
            var craftingSlot = Instantiate(slotPrefab, transform);
            craftingSlot.recipe = recipes[i];
            craftableSlots.Add(craftingSlot);
            craftableSlots[i].AddItem(recipes[i].product.count, recipes[i].product.item);
        }

        //Invoke("u", .1f);
    }

    public void TryCraft(RecipeData recipe)
    {
        var slots = im.CanCraft(recipe);

        if (slots != null)
        {
            for (int i = 0; i < recipe.ingredients.Length; i++)
            {
                slots[i].RemoveCount(recipe.ingredients[i].count);
            }

            if (c.itemPackage.item == null)
            {
                c.TakeItem(new ItemPackage(recipe.product.item, recipe.product.count));
            }
            else if (c.itemPackage.item == recipe.product.item)
            {
                c.TakeItem(new ItemPackage(recipe.product.item, recipe.product.count + c.itemPackage.count));
            }
        }
    }

    void u()
    {
        for (int k = 0; k < recipes.Count; k++)
        {
            craftableSlots[k].transform.SetParent(transform);
        }
    }

    void Update()
    {
        if (im.canEditInventory)
        {
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

        //if (newSlot.itemPackage.item)
        //{
        //    currentItemText.text = newSlot.itemPackage.item.itemName;
        //}
        //else
        //{
        //    currentItemText.text = "";
        //}
    }
}
