using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MoodInventory : MonoBehaviour, IMoodInventory
{
    [System.Serializable]
    private struct InitialItemState
    {
        public enum InstanceType
        {
            DefaultFromItem,
            Arbirtrary,
        }

        public MoodItem type;
        public InstanceType instanceToUse;
        public bool equipped;
        public MoodItemInstance arbitraryInstance;

        public MoodItemInstance GetInstance()
        {
            switch (instanceToUse)
            {
                case InstanceType.Arbirtrary:
                    return arbitraryInstance;
                default:
                    return type.MakeNewInstance();
            }
        }
    }

    [System.Serializable]
    private class SlotData
    {
        public MoodItemCategory slotType;
        public int maxAmountOfItems = 1;
        public bool IsSwappable()
        {
            return maxAmountOfItems == 1;
        }
    }

    [SerializeField] private InitialItemState[] _initialItems;
    [SerializeField] private SlotData[] _initialSlotData;
    [SerializeField] private SlotData _defaultSlotData;

    private Dictionary<MoodItemCategory, SlotData> _slotData;
    [SerializeField] private Dictionary<MoodItemCategory, HashSet<MoodItemInstance>> _equipped;
    [SerializeField] [ReadOnly] private List<MoodItemInstance> _bag;
    public int maxItemCount = 12;
    [SerializeField]
    private int maxItemCountEver = 12;

    public event IMoodInventory.DelInventoryEvent OnInventoryChange;
    public event IMoodInventory.DelInventoryEventWithItem OnEquipped;
    public event IMoodInventory.DelInventoryEventWithItem OnUnequipped;

    private void Awake()
    {
        maxItemCountEver = Mathf.Max(maxItemCount, maxItemCountEver);
        MakeSlotData();
        _equipped = new Dictionary<MoodItemCategory, HashSet<MoodItemInstance>>(maxItemCountEver);
        _bag = new List<MoodItemInstance>(maxItemCountEver);

    }

    private void MakeSlotData()
    {
        _slotData = new Dictionary<MoodItemCategory, SlotData>(_initialSlotData.Length);
        foreach (var data in _initialSlotData) _slotData.Add(data.slotType, data);
    }

    private void Start()
    {
        foreach (var item in _initialItems)
        {
            MoodItemInstance instance = item.GetInstance();
            GetItem(instance, false);
            if (item.equipped) EquipItem(instance);
        }
        OnInventoryChange?.Invoke();
    }

    public void GetItem(MoodItemInstance item, bool feedback = true)
    {
        _bag.Add(item);
        if (feedback) OnInventoryChange?.Invoke();
    }

    public IEnumerable<MoodItemInstance> GetAllItems()
    {
        return _bag;
    }

    public IEnumerable<MoodItemInstance> GetEquippedItems()
    {
        return _equipped.Values?.SelectMany((x)=> x);
    }

    public IEnumerable<MoodItemInstance> GetEquippedItems(MoodItemCategory category)
    {
        if (_equipped.ContainsKey(category))
            return _equipped[category];
        else
            return Enumerable.Empty<MoodItemInstance>();
    }


    public int GetAllItemsLength()
    {
        return _bag.Count;
    }

    public IEnumerable<(MoodSkill, MoodItemInstance)> GetAllUsableSkills()
    {
        foreach(MoodItemInstance inst in GetEquippedItems())
        {
            foreach(MoodSkill skill in inst.itemData.GetSkills())
            {
                yield return (skill, inst);
            }
        }
    }

    /// <summary>
    /// Set item equipped or not. Return if it worked successfully
    /// </summary>
    /// <param name="item"></param>
    /// <param name="setEquipped"></param>
    /// <returns>If the operation worked</returns>
    public bool SetItemEquipped(MoodItemInstance item, bool setEquipped)
    {
        if (setEquipped) return EquipItem(item);
        else return UnequipItem(item);
    }

    private bool EquipItem(MoodItemInstance item)
    {
        if (AddItemToEquippedSet(item))
        {
            OnEquipped?.Invoke(item);
            OnInventoryChange?.Invoke();
            return true;
        }
        return false;
    }

    private bool UnequipItem(MoodItemInstance item)
    {
        if(RemoveItemFromEquippedSet(item))
        {
            OnUnequipped?.Invoke(item);
            OnInventoryChange?.Invoke();
            return true;
        }
        return false;
    }

    public bool IsEquipped(MoodItem item)
    {
        return GetEquippedItems(item.category).Any((x) => x.itemData == item);
    }

    public bool CanEquip(MoodItemInstance item)
    {
        return !IsEquipped(item.itemData) && GetSlotData(item.itemData.category).maxAmountOfItems > AmountCategoryEquipped(item.itemData.category);
    }

    public bool HasItemInBag(MoodItemInstance item)
    {
        return _bag.Any((x) => x == item);
    }

    public MoodItemInstance GetObstacleItem(MoodItemCategory category)
    {
        if(GetSlotData(category).IsSwappable())
        {
            return GetEquippedItems(category).First();
        }
        return null;
    }

    public bool HasItemInBag(MoodItem item)
    {
        return _bag.Select((x) => x.itemData).Any((x) => x == item);
    }

    public bool IsEquipped(MoodItemInstance item)
    {
        return GetEquippedItems(item.itemData.category).Any((x) => x == item);
    }

    public bool AddItem(MoodItemInstance item)
    {
        if(_bag.Count >= maxItemCount)
        {
            return false;
        }
        _bag.Add(item);
        OnInventoryChange?.Invoke();
        return true;
    }

    public bool RemoveItem(MoodItemInstance item)
    {
        if (IsEquipped(item)) UnequipItem(item);
        bool removed = _bag.Remove(item);
        if (removed) OnInventoryChange?.Invoke();
        return removed;
    }

    public bool IsCategoryEquipped(MoodItemCategory category)
    {
        return AmountCategoryEquipped(category) > 0;
    }

    public int AmountCategoryEquipped(MoodItemCategory category)
    {
        if (_equipped.ContainsKey(category) && _equipped[category] != null)
        {
            return _equipped[category].Count;
        }
        else return 0;
    }


    #region Manipulate sets

    private SlotData GetSlotData(MoodItemCategory category)
    {
        if (category == null) return _defaultSlotData;
        else if (_slotData.ContainsKey(category)) return _slotData[category];
        else return _defaultSlotData;
    }

    private bool AddItemToEquippedSet(MoodItemInstance item)
    {
        MoodItemCategory cat = item.itemData.category;
        if (!_equipped.ContainsKey(cat))
        {
            _equipped.Add(item.itemData.category, new HashSet<MoodItemInstance>());
        }

        Debug.LogFormat("[INVENTORY] Trying to equip item {0}. Has {1} items in this {2}. Can equip {3}.", item, _equipped[cat].Count, cat, GetSlotData(cat).maxAmountOfItems);
        if (_equipped[cat].Count >= GetSlotData(cat).maxAmountOfItems) return false;
        else return _equipped[cat].Add(item);
    }

    private bool RemoveItemFromEquippedSet(MoodItemInstance item)
    {
        MoodItemCategory cat = item.itemData.category;
        if (!_equipped.ContainsKey(cat)) return false;
        return _equipped[cat].Remove(item);
    }


    #endregion

}
