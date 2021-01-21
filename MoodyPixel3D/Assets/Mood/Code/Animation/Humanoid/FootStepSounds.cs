using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootStepSounds : MonoBehaviour
{
    public SoundEffect sfx;

    public void StepLeft()
    {
        PlaySound();
    }

    public void StepRight()
    {
        PlaySound();
    }

    private void PlaySound()
    {
        sfx.InvokeReturn(transform);
    }
}
