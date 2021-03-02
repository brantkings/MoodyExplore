using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandModeDependent : MonoBehaviour
{
    public GameObject[] objectToChange;
    //public TweenOnEnable tweens;

    private void OnEnable()
    {
        OnCommandModeChange(false);
        MoodPlayerController.Instance.OnChangeCommandMode += OnCommandModeChange;
    }

    private void OnDisable()
    {
        if (MoodPlayerController.Instance != null)
            MoodPlayerController.Instance.OnChangeCommandMode -= OnCommandModeChange;
    }

    private void OnCommandModeChange(bool set)
    {
        foreach (GameObject g in objectToChange) g.SetActive(set);
    }


}
