using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SameDirectionAsPawn : MonoBehaviour
{
    MoodPawn pawn;

    private void Start()
    {
        pawn = GetComponentInParent<MoodPawn>();
    }

    private void Update()
    {
        transform.forward = pawn.Direction;
    }
}
