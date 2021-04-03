using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoodInventory : MonoBehaviour
{
    public int EQUIPPED_ITEMS_NUMBER = 8;
    public int CATEGORY_LESS_ITEMS = 8;

    //public MoodItemCategory[] categoriesToEquipInto;

    private Dictionary<ConsumableMoodItem, int> categoryLessItems;
    private Dictionary<MoodItemCategory, EquippableMoodItem> equippedItems;

    [Space()]
    public MoodItem testAdd;

    [ContextMenu("Test add an item")]
    void TestAdd()
    {
        AddUntypedItem(testAdd);
    }

    public void AddUntypedItem(MoodItem item)
    {
        if (item is EquippableMoodItem) Equip(item as EquippableMoodItem);
        else AddItem(item as ConsumableMoodItem);
    }

    public bool UseItem(MoodItem item)
    {
        if (item is ConsumableMoodItem) return ConsumeItem(item as ConsumableMoodItem);
        else return true;
    }

    private IEnumerable<EquippableMoodItem> GetEquippedItems()
    {
        foreach (EquippableMoodItem item in equippedItems.Values) yield return item;
    }

    public IEnumerable<MoodSkill> GetEquippedSkills()
    {
        foreach (EquippableMoodItem item in GetEquippedItems())
            foreach (MoodSkill skill in item.skillsToGrant) yield return skill;
    }

    public IEnumerable<ConsumableMoodItem> GetConsumableItems()
    {
        if (equippedItems == null) yield break;
        foreach (ConsumableMoodItem item in categoryLessItems.Keys)
        {
            if (categoryLessItems[item] > 0) yield return item;
        }
    }


    public bool Equip(EquippableMoodItem item)
    {
        if (equippedItems == null) equippedItems = new Dictionary<MoodItemCategory, EquippableMoodItem>(EQUIPPED_ITEMS_NUMBER);

        if(equippedItems.ContainsKey(item.category))
        {
            equippedItems[item.category] = item;
            return true;
        }
        else
        {
            equippedItems.Add(item.category, item);
            return false;
        }

    }

    public bool Unequip(MoodItemCategory category)
    {
        if(equippedItems != null)
        {
            return equippedItems.Remove(category);
        }
        return false;
    }

    public bool Unequip(EquippableMoodItem item)
    {
        return Unequip(item.category);
    }

    public bool AddItem(ConsumableMoodItem item)
    {
        if (categoryLessItems == null) categoryLessItems = new Dictionary<ConsumableMoodItem, int>(CATEGORY_LESS_ITEMS);

        if(categoryLessItems.ContainsKey(item))
        {
            categoryLessItems[item] = categoryLessItems[item] + 1;
            return true;
        }
        else
        {
            categoryLessItems.Add(item, 1);
            return true;
        }
    }

    public bool ConsumeItem(ConsumableMoodItem item)
    {
        if (categoryLessItems == null) return false;
        if(categoryLessItems.ContainsKey(item))
        {
            if(categoryLessItems[item] > 0)
            {
                categoryLessItems[item] = categoryLessItems[item] - 1;
                return true;
            }
        }
        return false;
    }

    public int GetItemAmount(ConsumableMoodItem item)
    {
        if (categoryLessItems == null) return 0;
        else return categoryLessItems[item];
    }

    public IEnumerator<string> WriteInventory()
    {
        yield return $"Inventory of '{name}':";
        foreach(EquippableMoodItem item in equippedItems.Values)
            yield return $"Equipped {item.name} in {item.category.name}.";
        foreach (ConsumableMoodItem item in categoryLessItems.Keys)
            yield return $"Holding {categoryLessItems[item]} units of {item}.";
    }


}
