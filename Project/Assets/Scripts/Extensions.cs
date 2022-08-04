using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
    /// <summary>
    /// Returns the absolute value of the remainder count after reaching 0 or max stack.
    /// </summary>
    /// <param name="count"></param>
    /// <returns></returns>
    public static int OverflowOnAdd(int add, int current, int max)
    {
        if (add > 0)
        {
            return Mathf.Clamp((current + add) - max, 0, 10000000);
        }
        else if (add < 0)
        {
            return Mathf.Clamp((current + add) - max, 0, 10000000);
        }

        return 0;
    }

    /// <summary>
    /// Returns abs of the remainder after reaching zero or max stack.
    /// If zero it worked.
    /// Otherwise if -1, they are different items
    /// </summary>
    /// <param name="count"></param>
    /// <returns></returns>
    public static int TryModifyItem(this ItemPackage currentPackage, ItemPackage addPackage, out ItemPackage newItemPackage, float addCount = .1f)
    {
        if (Mathf.Abs(addCount) != .1f) { addPackage.count = (int)addCount; }

        newItemPackage = currentPackage;
        //if (invertCount) { addPackage.count = -addPackage.count; }

        if (currentPackage.item == null)
        {
            newItemPackage = addPackage;
            return 0;
        }
        else
        {
            if (currentPackage.item == addPackage.item)
            {
                var overFlow = OverflowOnAdd(addPackage.count, currentPackage.count, currentPackage.item.itemData.maxStack);

                if (overFlow == 0)
                {
                    // No overflow so you can add
                    newItemPackage.count += addPackage.count;
                    return 0;
                }
                else if (overFlow > 0)
                {
                    // Overflow! Set count to max stack and return leftovers
                    newItemPackage.count = currentPackage.item.itemData.maxStack;
                    return overFlow;
                }
            }
            else
            {
                // Different items can not add them
                return -1;
            }
        }

        return -1;
    }
    public static int CanAddItem(ItemPackage currentPackage, ItemPackage addPackage, bool invertCount = false)
    {
        if (invertCount) { addPackage.count = -addPackage.count; }

        if (currentPackage.item == null)
        {
            currentPackage = addPackage;
            return 0;
        }
        else
        {
            if (currentPackage.item.Equals(currentPackage.item))
            {
                var overFlow = OverflowOnAdd(addPackage.count, currentPackage.count, currentPackage.item.itemData.maxStack);

                if (overFlow == 0)
                {
                    // No overflow so you can add
                    currentPackage.count += addPackage.count;
                    return 0;
                }
                else if (overFlow > 0)
                {
                    // Overflow! Set count to max stack and return leftovers
                    currentPackage.count = currentPackage.item.itemData.maxStack;
                    return overFlow;
                }
            }
            else
            {
                // Different items can not add them
                return -1;
            }
        }

        return -1;
    }
}
