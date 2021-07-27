using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MoodItem : ScriptableObject
{
    [SerializeField]
    private string _itemName;
    [SerializeField]
    [TextArea]
    private string _itemDescription;

    [SerializeField]
    private ItemInteractable pickupPrefab;
    public MoodItemCategory category;

    public string GetName()
    {
        return _itemName;
    }

    public string GetDescription()
    {
        return _itemDescription;
    }

    public ItemInteractable GetPickupPrefab()
    {
        return pickupPrefab;
    }

    public abstract bool CanUse(MoodPawn pawn, MoodInventory inventory);

    public abstract void OnAdquire(MoodPawn pawn);

    public abstract void OnUse(MoodPawn pawn);
}
