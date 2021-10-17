using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMoodInventory 
{

    public delegate void DelInventoryEvent();

    public event DelInventoryEvent OnInventoryChange;
    IEnumerable<MoodItemInstance> GetAllItems();
    int GetAllItemsLength();
    IEnumerable<(MoodSkill, MoodItemInstance)> GetAllUsableSkills();
    
}
