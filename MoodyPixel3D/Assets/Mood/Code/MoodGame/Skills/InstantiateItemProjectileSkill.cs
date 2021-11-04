using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Skill_Instantiate_Item", menuName = "Mood/Skill/Instantiate", order = 0)]
public class InstantiateItemProjectileSkill : InstantiateSkill
{
    public override string GetName(MoodPawn pawn)
    {
        return base.GetName(pawn) + " " + pawn.GetCurrentItem().itemData.GetName();
    }


    protected override GameObject GetProjectile(MoodPawn from, Vector3 skillDirection, Vector3 pos, Quaternion rot)
    {
        MoodItemInstance item = from.GetCurrentItem();
        from.RemoveItem(item);
        ItemProjectile proj = Instantiate(item.itemData.GetProjectilePrefab(), pos, rot);
        proj.instance = item;
        return proj.gameObject;
    }
}
