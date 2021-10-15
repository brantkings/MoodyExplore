using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemInteractable : MoodInteractable
{
    public enum Consumes
    {
        OncePerInteract,
        AllAtOnce,
        OnceAndNeverDepletes,
        AllQuantityAndNeverDepletes
    }

    public MoodItem item;

    public int consumableQuantity = 1;
    public Consumes consumableStyle = Consumes.AllAtOnce;
    public GameObject objectToConsume;

    private GameObject GetConsumed()
    {
        if (objectToConsume != null) return objectToConsume; else return gameObject;
    }

    public override void Interact(MoodInteractor interactor)
    {
        MoodInventoryOld inventory = interactor.GetComponentInParent<MoodInventoryOld>();
        switch (consumableStyle)
        {
            case Consumes.OncePerInteract:
                AddItem(inventory);
                if (--consumableQuantity <= 0)
                    Destroy(GetConsumed());
                break;
            case Consumes.AllAtOnce:
                while(consumableQuantity-- > 0)
                {
                    AddItem(inventory);
                }
                Destroy(GetConsumed());
                break;
            case Consumes.OnceAndNeverDepletes:
                AddItem(inventory);
                break;
            case Consumes.AllQuantityAndNeverDepletes:
                int q = consumableQuantity;
                while(q-- > 0)
                {
                    AddItem(inventory);
                }
                AddItem(inventory);
                break;
            default:
                Debug.LogErrorFormat("Cant find {0} in {1}", consumableStyle, this);
                break;
        }
    }

    private void AddItem(MoodInventoryOld inventory)
    {
        inventory.AddUntypedItem(item);
    }

    public override bool IsBeingInteracted()
    {
        return false;
    }
}
