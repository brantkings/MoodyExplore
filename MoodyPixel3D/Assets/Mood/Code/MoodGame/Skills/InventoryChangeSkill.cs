using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Skill_InventoryChange_", menuName = "Mood/Skill/Inventory/Inventory Change", order = 0)]
public class InventoryChangeSkill : CostStaminaDelaySkill
{
    public enum Action
    {
        EquipCurrentItem,
        UnequipCurrentItem,
        UnequipCurrentItemAndRemove,
        AddArbitraryItem,
        AddAndEquipArbitraryItem,
    }
    [Header("Inventory")]
    public Action action = Action.EquipCurrentItem;
    public MoodItem arbitraryItem;


    protected override (float, ExecutionResult) ExecuteEffect(MoodPawn pawn, Vector3 skillDirection)
    {
        MoodItemInstance item = pawn.GetCurrentItem();
        switch (action)
        {
            case Action.EquipCurrentItem:
                pawn.Equip(item);
                break;
            case Action.UnequipCurrentItem:
                pawn.Unequip(item);
                break;
            case Action.UnequipCurrentItemAndRemove:
                pawn.Unequip(item);
                pawn.RemoveItem(item);
                break;
            case Action.AddArbitraryItem:
                item = arbitraryItem.MakeNewInstance();
                pawn.AddItem(item);
                break;
            case Action.AddAndEquipArbitraryItem:
                item = arbitraryItem.MakeNewInstance();
                pawn.AddItem(item);
                pawn.Equip(item);
                break;
            default:
                break;
        }
        return base.ExecuteEffect(pawn, skillDirection);
    }
}
