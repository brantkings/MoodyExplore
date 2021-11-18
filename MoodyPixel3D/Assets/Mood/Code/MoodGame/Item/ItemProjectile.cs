using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemProjectile : MonoBehaviour, IItemHolder
{
    public MoodItemInstance instance;
    public int itemQuanitityCost;
    private bool _destroyed;

    public MoodItemInstance GetItem()
    {
        return instance;
    }

    public void Hold(MoodItemInstance instance)
    {
        this.instance = instance;
        Debug.LogErrorFormat("{0} is holding {1}!", this, this.instance);
    }

    public void Release(MoodItemInstance instance)
    {
        if (this.instance == instance) this.instance = null;
    }

    private void OnCollisionEnter(Collision collision)
    {
        OnTriggerEnter(collision.collider);
    }


    private void OnTriggerEnter(Collider other)
    {
        if (_destroyed) return;

        /*Debug.LogErrorFormat(collision.collider, "{0} collided with {5}! Now gonna get pickup from {1} which is {2}! Yay! Going for pos{3} and rot{4}", this, this.instance, this.instance.itemData.GetPickupPrefab(), transform.position, transform.rotation,
            collision.collider);*/
        MoodItemInstance itemInstance = this.instance;
        if(itemInstance.IsValid())
        {
            Release(itemInstance);
            itemInstance.properties.quantity -= itemQuanitityCost;

            if(itemInstance.IsFunctional())
            {
                ItemInteractable item = Instantiate<ItemInteractable>(itemInstance.itemData.GetPickupPrefab(), transform.position, transform.rotation);
                item.Hold(this.instance);
            }
        }
        Destroy(gameObject);
        _destroyed = true;
    }
}
