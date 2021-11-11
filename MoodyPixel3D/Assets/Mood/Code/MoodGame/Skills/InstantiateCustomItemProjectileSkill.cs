using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Skill_Instantiate_Item", menuName = "Mood/Skill/Inventory/Throw Item Custom", order = 0)]
public class InstantiateCustomItemProjectileSkill : InstantiateSkill
{
    public ItemProjectile prefab;

    public override string GetName(MoodPawn pawn)
    {
        return base.GetName(pawn) + " " + pawn.GetCurrentItem().itemData.GetName();
    }

    protected override GameObject GetProjectile(MoodPawn from, Vector3 skillDirection, Vector3 pos, Quaternion rot)
    {
        MoodItemInstance item = from.GetCurrentItem();
        from.RemoveItem(item);
        ItemProjectile proj = Instantiate(prefab, pos, rot);
        proj.Hold(item);
        return proj.gameObject;
    }
}
