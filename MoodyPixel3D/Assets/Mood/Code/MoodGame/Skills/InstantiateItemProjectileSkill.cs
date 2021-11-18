using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Skill_Instantiate_Item", menuName = "Mood/Skill/Inventory/Throw Item", order = 0)]
public class InstantiateItemProjectileSkill : InstantiateSkill
{
    [Header("Instantiate Item Projectile")]
    public bool addItemNameToSkillName = true;

    public override string GetName(MoodPawn pawn)
    {
        MoodItemInstance item = pawn.GetCurrentItem();
        if(item != null && addItemNameToSkillName)
        {
            return base.GetName(pawn) + " " + item.itemData.GetName();
        }
        else
        {
            return base.GetName(pawn) + " " + "item";
        }
    }


    protected override GameObject GetProjectile(MoodPawn from, Vector3 skillDirection, Vector3 pos, Quaternion rot)
    {
        MoodItemInstance item = from.GetCurrentItem();
        from.RemoveItem(item);
        ItemProjectile proj = Instantiate(item.itemData.GetProjectilePrefab(), pos, rot);
        proj.Hold(item);
        return proj.gameObject;
    }
}
