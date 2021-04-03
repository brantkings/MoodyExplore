using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoodItem : ScriptableObject
{
    private string _itemName;
    private string _itemDescription;
    public MoodItemCategory category;

    public string GetName()
    {
        return _itemName;
    }

    public string GetDescription()
    {
        return _itemDescription;
    }
}
