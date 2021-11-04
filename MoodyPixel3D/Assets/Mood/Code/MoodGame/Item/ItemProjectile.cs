using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemProjectile : MonoBehaviour
{
    public MoodItemInstance instance;

    private void OnCollisionEnter(Collision collision)
    {
        Instantiate(instance.itemData.GetPickupPrefab(), transform.position, transform.rotation);
        Destroy(gameObject);
    }
}
