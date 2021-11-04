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

    public bool createDefaultOnAwake;
    public MoodItemInstance instance;

    public int consumableQuantity = 1;
    public Consumes consumableStyle = Consumes.AllAtOnce;
    public GameObject objectToConsume;

    private void Awake()
    {
        if (instance.itemData == null)
        {
            Debug.LogErrorFormat(this, "Error! No item data on {0}!", this);
        }
        if (createDefaultOnAwake) instance = instance.itemData?.MakeNewInstance();
    }

    public void CreateFrom(MoodItemInstance instance)
    {
        this.instance = instance;
        createDefaultOnAwake = false;
    }

    private GameObject GetConsumed()
    {
        if (objectToConsume != null) return objectToConsume; else return gameObject;
    }

    public override void Interact(MoodInteractor interactor)
    {
        IMoodInventory inventory = interactor.GetComponentInParent<IMoodInventory>();
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

    private void AddItem(IMoodInventory inventory)
    {
        inventory.AddItem(instance);
    }

    public override bool IsBeingInteracted()
    {
        return false;
    }
}
