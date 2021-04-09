using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MoodItem : ScriptableObject
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

    public abstract bool CanUse(MoodPawn pawn, MoodInventory inventory);

    public abstract void OnAdquire(MoodPawn pawn);

    public abstract void OnUse(MoodPawn pawn);
}
