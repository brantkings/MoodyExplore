using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoodItemInstance
{
    public MoodItem itemData;
    public int durability;
    public int quanitity;

    public bool IsSameType(MoodItemInstance other)
    {
        return this.itemData == other.itemData;
    }

    public bool CanMerge(MoodItemInstance other)
    {
        return IsSameType(other) && itemData.category == null;
    }

    public void MergeWithAndDestroy(ref MoodItemInstance instance)
    {
        this.quanitity += instance.quanitity;
        instance = null;
    }

    public string GetStatusText()
    {
        string status = "";
        AddStatusText(ref status, "Dur:", durability);
        AddStatusText(ref status, "x", quanitity);
        return status;
    }

    private void AddStatusText(ref string status, string prefix, object what)
    {
        if (!string.IsNullOrWhiteSpace(status)) status += '\n';
        status += $"{prefix}{what}";
    }
}
