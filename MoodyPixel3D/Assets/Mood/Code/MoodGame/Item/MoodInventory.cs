using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoodInventory : MonoBehaviour, IMoodInventory
{
    public MoodItem[] _initialItems;

    public Dictionary<MoodItemCategory, MoodItemInstance> equipped;
    public List<MoodItemInstance> bag;

    public event IMoodInventory.DelInventoryEvent OnInventoryChange;

    private void Awake()
    {
        equipped = new Dictionary<MoodItemCategory, MoodItemInstance>(12);
        bag = new List<MoodItemInstance>(12);
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

    public int GetAllItemsLength()
    {
        return bag.Count;
    }

    public IEnumerable<(MoodSkill, MoodItemInstance)> GetAllUsableSkills()
    {
        foreach(var item in equipped)
        {
            foreach (var skill in item.Value.itemData.GetSkills())
                yield return (skill, item.Value);
        }
    }
}
