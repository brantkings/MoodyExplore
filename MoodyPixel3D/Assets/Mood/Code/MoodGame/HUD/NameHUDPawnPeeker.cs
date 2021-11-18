using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NameHUDPawnPeeker : MonoBehaviour, IMoodPawnPeeker
{
    public Text text;

    public void SetTarget(MoodPawn pawn)
    {
        text.text = pawn.GetName();
    }

    public void UnsetTarget(MoodPawn pawn)
    {
        text.text = string.Empty;
    }
}
