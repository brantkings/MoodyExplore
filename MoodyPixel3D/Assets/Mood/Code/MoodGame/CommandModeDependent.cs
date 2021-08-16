using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CommandModeDependent : MonoBehaviour
{
    public GameObject[] objectToChange;
    public LHH.Switchable.Switchable[] switchables;
    public MoodPlayerController.Mode[] modesToAppear = { MoodPlayerController.Mode.Command_Skill, MoodPlayerController.Mode.Command_Focus};
    //public TweenOnEnable tweens;

    private void OnEnable()
    {
        OnCommandModeChange(MoodPlayerController.Mode.None);
        if (MoodPlayerController.Instance != null)
            MoodPlayerController.Instance.OnChangeCommandMode += OnCommandModeChange;
    }

    private void OnDisable()
    {
        if (MoodPlayerController.Instance != null)
            MoodPlayerController.Instance.OnChangeCommandMode -= OnCommandModeChange;
    }

    private bool Corresponds(MoodPlayerController.Mode mode)
    {
        return modesToAppear.Contains(mode);
    }

    private void OnCommandModeChange(MoodPlayerController.Mode set)
    {
        bool active = Corresponds(set);
        foreach (GameObject g in objectToChange) g.SetActive(active);
        foreach (LHH.Switchable.Switchable s in switchables) s.Set(active);
    }


}
