using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Mood/Item/Equipment", fileName = "Item_E_")]
public class EquippableMoodItem : MoodItem
{
    public MoodSkill[] skillsToGrant;

    public AttachItself equippedItem;
    public Vector3 equippedItemOffset;

    private List<AttachItself> _instances = new List<AttachItself>(8);

    public override bool CanUse(MoodPawn pawn, MoodInventory inventory)
    {
        return true;
    }

    public void SetEquipped(MoodPawn pawn, bool set)
    {
        if (set)
        {
            AttachItself newInstance = Instantiate(equippedItem);
            pawn.GetComponentInChildren<AttacheableArmature>().Attach(newInstance.transform, newInstance.part, equippedItemOffset);
            Debug.LogFormat(newInstance, "Equipped {0} and instantiated {1}", this, newInstance);
            _instances.Add(newInstance);
        }
        else
        {
            Instantiate(GetPickupPrefab(), pawn.ObjectTransform.position + Vector3.up * 5f, pawn.ObjectTransform.rotation, null);
            foreach (AttachItself a in pawn.animator.GetComponentsInChildren<AttachItself>())
            {
                if (_instances.Contains(a))
                {
                    _instances.Remove(a);
                    Destroy(a.gameObject);
                }
            }
        }
    }

    public override void OnAdquire(MoodPawn pawn)
    {
    }

    public override void OnUse(MoodPawn pawn)
    {
       
    }
}
