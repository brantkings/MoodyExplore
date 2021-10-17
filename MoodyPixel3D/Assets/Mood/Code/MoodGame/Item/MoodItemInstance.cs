using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MoodItemInstance
{
    public MoodItem itemData;
    public Properties properties;

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
        other = null;
    }

    public void Use(MoodPawn pawn)
    {
        itemData.OnUse(pawn);
    }

    public override string ToString()
    {
        return $"{itemData} -> {properties}";
    }
}
