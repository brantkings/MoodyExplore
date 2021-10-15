using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMoodInventory 
{
    IEnumerable<MoodItemInstance> GetAllItems();
    int GetAllItemsLength();
    
}
