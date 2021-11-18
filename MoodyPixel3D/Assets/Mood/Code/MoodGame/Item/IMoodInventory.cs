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

    IEnumerable<MoodItemInstance> GetEquippedItems();
    int GetAllItemsLength();
    IEnumerable<(MoodSkill, MoodItemInstance)> GetAllUsableSkills();

    /// <summary>
    /// Set item equipped or not. Returns if it worked successfully.
    /// </summary>
    /// <param name="item"></param>
    /// <param name="setEquipped"></param>
    /// <returns>If the operation worked</returns>
    bool SetItemEquipped(MoodItemInstance item, bool equipped);

    bool IsEquipped(MoodItemInstance item);

    bool IsEquipped(MoodItem item);

    bool CanEquip(MoodItemInstance item);

    bool IsCategoryEquipped(MoodItemCategory category);


    int AmountCategoryEquipped(MoodItemCategory category);

    bool HasItemInBag(MoodItemInstance item);

    bool HasItemInBag(MoodItem item);

    bool AddItem(MoodItemInstance item);

    bool RemoveItem(MoodItemInstance item);

    /// <summary>
    /// Get an item to swap with if cannot equip item. TODO: Not usable in the final game, lets use a slot system.
    /// </summary>
    /// <param name="category"></param>
    /// <returns></returns>
    MoodItemInstance GetObstacleItem(MoodItemCategory category);



}
