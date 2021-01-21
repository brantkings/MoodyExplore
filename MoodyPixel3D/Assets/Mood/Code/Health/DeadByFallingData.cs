using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FALLDATA_", menuName = "Long Hat House/Health/Falling Data", order = 0)]
public class DeadByFallingData : ScriptableObject
{
    public float deadYPosition = -5;


    public bool IsDead(Vector3 position)
    {
        return position.y <= deadYPosition;
    }
}
