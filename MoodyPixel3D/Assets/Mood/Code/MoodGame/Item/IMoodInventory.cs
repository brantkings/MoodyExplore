using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMoodInventory 
{

    public delegate void DelInventoryEvent();
    public delegate void DelInventoryEventWithItem(MoodItemInstance item);

    public event DelInventoryEvent OnInventoryChange;
    public event DelInventoryEventWithItem OnEquipped;
    public event DelInventoryEventWithItem OnUnequipped;


    IEnumerable<MoodItemInstance> GetAllItems();
    int GetAllItemsLength();
    IEnumerable<(MoodSkill, MoodItemInstance)> GetAllUsableSkills();

    void SetItemEquipped(MoodItemInstance item, bool equipped);

    bool IsEquipped(MoodItemInstance item);

    bool IsEquipped(MoodItem item);

    bool HasItemInBag(MoodItemInstance item);

    bool HasItemInBag(MoodItem item);

    bool AddItem(MoodItemInstance item);

    bool RemoveItem(MoodItemInstance item);


    
}
