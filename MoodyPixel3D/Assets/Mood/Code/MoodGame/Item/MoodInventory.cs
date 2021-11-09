using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MoodInventory : MonoBehaviour, IMoodInventory
{
    public MoodItem[] _initialItems;

    public Dictionary<MoodItemCategory, MoodItemInstance> equipped;
    public List<MoodItemInstance> bag;
    public int maxItemCount = 12;
    [SerializeField]
    private int maxItemCountEver = 12;

    public event IMoodInventory.DelInventoryEvent OnInventoryChange;
    public event IMoodInventory.DelInventoryEventWithItem OnEquipped;
    public event IMoodInventory.DelInventoryEventWithItem OnUnequipped;

    private void Awake()
    {
        maxItemCountEver = Mathf.Max(maxItemCount, maxItemCountEver);
        equipped = new Dictionary<MoodItemCategory, MoodItemInstance>(maxItemCountEver);
        bag = new List<MoodItemInstance>(maxItemCountEver);
    }

    private void Start()
    {
        foreach(MoodItem item in _initialItems)
        {
            GetItem(item.MakeNewInstance(), false);
        }
        OnInventoryChange?.Invoke();
    }

    public void GetItem(MoodItemInstance item, bool feedback = true)
    {
        bag.Add(item);
        if (feedback) OnInventoryChange?.Invoke();
    }

    public IEnumerable<MoodItemInstance> GetAllItems()
    {
        return bag;
    }

    public IEnumerable<MoodItemInstance> GetEquippedItems()
    {
        return equipped.Values;
    }

    public int GetAllItemsLength()
    {
        return bag.Count;
    }

    public IEnumerable<(MoodSkill, MoodItemInstance)> GetAllUsableSkills()
    {
        foreach(var item in equipped)
        {
            if(item.Value != null)
            {
                foreach (var skill in item.Value.itemData.GetSkills())
                    yield return (skill, item.Value);
            }
        }
    }

    public void SetItemEquipped(MoodItemInstance item, bool setEquipped)
    {
        if (setEquipped) EquipItem(item);
        else UnequipItem(item);
    }

    private void EquipItem(MoodItemInstance item)
    {
        if (equipped.ContainsKey(item.itemData.category))
        {
            if(equipped[item.itemData.category] != item)
            {
                if(equipped[item.itemData.category] != null)
                {
                    UnequipItem(equipped[item.itemData.category]);
                }
                equipped[item.itemData.category] = item;
                OnEquipped?.Invoke(item);
            }
        }
        else
        {
            equipped.Add(item.itemData.category, item);
            OnEquipped?.Invoke(item);
        }
        OnInventoryChange?.Invoke();
    }

    private void UnequipItem(MoodItemInstance item)
    {
        if (IsEquipped(item))
        {
            equipped[item.itemData.category] = null;
            OnUnequipped?.Invoke(item);
        }
        Debug.LogFormat("Unequipped {0}", item);
        OnInventoryChange?.Invoke();
    }

    public bool IsEquipped(MoodItemInstance item)
    {
        Debug.LogFormat("Contains value {0}? {1}", item, equipped.ContainsValue(item));
        return equipped.ContainsValue(item);
    }

    public bool IsEquipped(MoodItem item)
    {
        return equipped.Select((x) => x.Value.itemData).Any((x) => x == item);
    }

    public bool HasItemInBag(MoodItemInstance item)
    {
        return bag.Any((x) => x == item);
    }

    public bool HasItemInBag(MoodItem item)
    {
        return bag.Select((x) => x.itemData).Any((x) => x == item);
    }

    public bool AddItem(MoodItemInstance item)
    {
        if(bag.Count >= maxItemCount)
        {
            return false;
        }
        bag.Add(item);
        OnInventoryChange?.Invoke();
        return true;
    }

    public bool RemoveItem(MoodItemInstance item)
    {
        if (IsEquipped(item)) UnequipItem(item);
        bool removed = bag.Remove(item);
        if (removed) OnInventoryChange?.Invoke();
        return removed;
    }

    public bool IsCategoryEquipped(MoodItemCategory category)
    {
        return equipped.ContainsKey(category) && equipped[category] != null;
    }

}
