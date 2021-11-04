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
    private Sprite _itemIcon;

    [SerializeField]
    private ItemProjectile _projectilePrefab;
    [SerializeField]
    private ItemInteractable _pickupPrefab;
    public MoodItemCategory category;

    [SerializeField]
    private MoodItemInstance.Properties instancePrototype = MoodItemInstance.Properties.Default;



    public string GetName()
    {
        return _itemName;
    }

    public string GetDescription()
    {
        return _itemDescription;
    }
    
    public Sprite GetIcon()
    {
        return _itemIcon;
    }

    public ItemInteractable GetPickupPrefab()
    {
        return _pickupPrefab;
    }

    public ItemProjectile GetProjectilePrefab()
    {
        return _projectilePrefab;
    }

    public virtual MoodItemInstance MakeNewInstance()
    {
        return new MoodItemInstance()
        {
            itemData = this,
            properties = instancePrototype
        };
    }

    public abstract IEnumerable<MoodSkill> GetSkills();

    public abstract string WriteItemStatus(in MoodItemInstance.Properties properties, in bool equipped);

    /// <summary>
    /// For use on WriteItemStatus
    /// </summary>
    /// <param name="status"></param>
    /// <param name="prefix"></param>
    /// <param name="what"></param>
    protected void AddStatusText(ref string status, string prefix, object what)
    {
        if (!string.IsNullOrWhiteSpace(status)) status += '\n';
        status += $"{prefix}<color=white>{what}</color>";
    }

    public abstract bool CanUse(MoodPawn pawn, IMoodInventory inventory);

    public abstract void OnAdquire(MoodPawn pawn);

    public virtual bool IsFunctional(in MoodItemInstance.Properties properties)
    {
        return properties.quantity > 0;
    }

    public virtual void OnUse(MoodPawn pawn, MoodSkill skill, ref MoodItemInstance.Properties properties)
    {
        properties.quantity--;
    }

}
