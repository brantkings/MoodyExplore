using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MoodInventory : MonoBehaviour
{
    public const int EQUIPPED_ITEMS_NUMBER = 8;
    public const int CATEGORY_LESS_ITEMS_NUMBER = 8;

    private MoodPawn _pawn;
    public MoodPawn Pawn
    {
        get
        {
            if (_pawn == null) _pawn = GetComponent<MoodPawn>();
            return _pawn;
        }
    }


    public delegate void DelInventoryChange(MoodInventory inventory);
    public event DelInventoryChange OnInventoryChange;

    //public MoodItemCategory[] categoriesToEquipInto;

    public MoodItem[] initialItems;

    private Dictionary<ConsumableMoodItem, int> categoryLessItems = new Dictionary<ConsumableMoodItem, int>(EQUIPPED_ITEMS_NUMBER);
    private Dictionary<MoodItemCategory, EquippableMoodItem> equippedItems = new Dictionary<MoodItemCategory, EquippableMoodItem>(CATEGORY_LESS_ITEMS_NUMBER);

    [Space()]
    public MoodItem testAdd;

    [ContextMenu("Test add an item")]
    void TestAdd()
    {
        AddUntypedItem(testAdd);
    }

    private void Start()
    {
        foreach (MoodItem item in initialItems) AddUntypedItem(item, false);
        DispatchInventoryChanged();
    }


    public void AddUntypedItem(MoodItem item, bool feedbacks = true)
    {
        Debug.LogFormat("[INVENTORY] Adding item {0}", item);
        if (item is EquippableMoodItem) Equip(item as EquippableMoodItem, feedbacks);
        else AddItem(item as ConsumableMoodItem, feedbacks);
    }

    public bool UseItem(MoodItem item)
    {
        if (item is ConsumableMoodItem) return ConsumeItem(item as ConsumableMoodItem);
        else return true;
    }

    public bool HasItem(ConsumableMoodItem item)
    {
        return categoryLessItems.Any((x) =>
        {
            return x.Key == item && x.Value > 0;
        });
    }

    public IEnumerable<Tuple<MoodSkill, MoodItem>> GetAllUsableSkills()
    {
        foreach (EquippableMoodItem item in GetEquippedItems())
            foreach (MoodSkill skill in item.skillsToGrant)
                yield return new Tuple<MoodSkill, MoodItem>(skill, item);
        foreach (ConsumableMoodItem item in GetConsumableItems())
        {
            Debug.LogFormat("[INVENTORY] Checking out {0} in {1}", item, this);
            foreach (MoodSkill skill in item.consumableSkills)
            {
                Debug.LogFormat("[INVENTORY] Returning out {0} in {1} in {2}", skill, item, this);
                yield return new Tuple<MoodSkill, MoodItem>(skill, item);
            }
        }
    }

    private IEnumerable<EquippableMoodItem> GetEquippedItems()
    {
        foreach (EquippableMoodItem item in equippedItems.Values)
        {
            yield return item;
        }
    }


    private IEnumerable<ConsumableMoodItem> GetConsumableItems()
    {
        if (categoryLessItems == null) yield break;
        foreach (ConsumableMoodItem item in categoryLessItems.Keys)
        {
            Debug.LogFormat("Checking out consumable item {0} for {1}", item, this);
            if (categoryLessItems[item] > 0) yield return item;
        }
    }

    public EquippableMoodItem GetEquippedItem(MoodItemCategory category)
    {
        if (equippedItems.ContainsKey(category))
        {
            return equippedItems[category];
        }
        else return null;
    }


    public bool Equip(EquippableMoodItem item, bool feedbacks = true)
    {
        if (item.category == null)
        {
            Debug.LogError("[INVENTORY] {0} has no category!", item);
            return false;
        }

        if(equippedItems.ContainsKey(item.category))
        {
            Unequip(item.category);
            equippedItems[item.category] = item;
        }
        else
        {
            equippedItems.Add(item.category, item);
        }

        item.SetEquipped(Pawn, true);
        if(feedbacks) DispatchInventoryChanged();
        return true;
    }

    public bool Unequip(EquippableMoodItem item, bool feedbacks = true)
    {
        return Unequip(item.category);
    }

    public bool Unequip(MoodItemCategory category, bool feedbacks = true)
    {
        if (equippedItems != null)
        {
            equippedItems[category]?.SetEquipped(Pawn, false);

            if (equippedItems.Remove(category))
            {
                if(feedbacks) DispatchInventoryChanged();
                return true;
            }

        }
        return false;
    }

    private bool AddItem(ConsumableMoodItem item, bool feedbacks = true)
    {
        if (categoryLessItems == null) categoryLessItems = new Dictionary<ConsumableMoodItem, int>(CATEGORY_LESS_ITEMS_NUMBER);

        if(categoryLessItems.ContainsKey(item))
        {
            categoryLessItems[item] = categoryLessItems[item] + 1;
            return true;
        }
        else
        {
            categoryLessItems.Add(item, 1);
            if(feedbacks) DispatchInventoryChanged();
            return true;
        }
    }

    public bool ConsumeItem(ConsumableMoodItem item, bool feedbacks = true)
    {
        if (categoryLessItems == null) return false;
        if(categoryLessItems.ContainsKey(item))
        {
            if(categoryLessItems[item] > 0)
            {
                categoryLessItems[item] = categoryLessItems[item] - 1;
                if(feedbacks) DispatchInventoryChanged();
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

    public IEnumerable<string> WriteInventory()
    {
        yield return $"Inventory of '{name}':";
        foreach(EquippableMoodItem item in equippedItems.Values)
            yield return $"Equipped {item.name} in {item.category.name}.";
        foreach (ConsumableMoodItem item in categoryLessItems.Keys)
            yield return $"Holding {categoryLessItems[item]} units of {item}.";
    }

    [ContextMenu("Test inventory")]
    public void TestInventory()
    {
        foreach(var s in WriteInventory())
        {
            Debug.Log(s);
        }
    }

    private void DispatchInventoryChanged()
    {
        OnInventoryChange?.Invoke(this);
    }


}
