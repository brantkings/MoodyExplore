using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MoodItemInstance
{
    public MoodItem itemData;
    public Properties properties;
    public MoodPawn equippedTo;

    public delegate void DelMoodItemInstanceEvent(MoodPawn pawn, MoodItemInstance instance);
    public delegate void DelMoodItemInstanceEventPawnLess(MoodItemInstance instance);
    public event DelMoodItemInstanceEvent OnEquipChange;
    public event DelMoodItemInstanceEvent OnUse;
    public event DelMoodItemInstanceEventPawnLess OnDestroy;

    [System.Serializable]
    public struct Properties
    {
        public int quantity;


        private static Properties _default = new Properties()
        {
            quantity = 1
        };
        public static Properties Default
        {
            get => _default; 
        }

        public override string ToString()
        {
            return $"({quantity})";
        }
    }

    public static void Destroy(ref MoodItemInstance other)
    {
        other.Destroy();
        other = null;
    }

    private void Destroy()
    {
        OnDestroy?.Invoke(this);
    }

    public bool IsSameType(MoodItemInstance other)
    {
        return this.itemData == other.itemData;
    }

    public bool CanMerge(MoodItemInstance other)
    {
        return IsSameType(other) && itemData.category == null;
    }

    public void MergeWithAndDestroy(ref MoodItemInstance other)
    {
        this.properties.quantity += other.properties.quantity;
        Destroy(ref other);
    }


    public bool IsFunctional()
    {
        return itemData.IsFunctional(properties);
    }

    public void Use(MoodPawn pawn, MoodSkill skill, DelMoodItemInstanceEventPawnLess onDestroy = null)
    {
        itemData.OnUse(pawn, skill, ref properties);
        OnUse?.Invoke(pawn, this);
        if(!itemData.IsFunctional(properties))
        {
            onDestroy?.Invoke(this);
            Destroy();
        }
    }

    public void SetEquipped(MoodPawn pawn, bool set)
    {
        if(set)
            equippedTo = pawn;
        else if(equippedTo == pawn)
        {
            equippedTo = null;
        }

        //Todo every item can be equipped in the final game?
        if(itemData is EquippableMoodItem)
        {
            (itemData as EquippableMoodItem).SetEquipped(pawn, set);
        }

        OnEquipChange?.Invoke(pawn, this);

    }

    public override string ToString()
    {
        return $"{itemData} -> {properties}";
    }
}
