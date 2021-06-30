using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IThoughtBoardObject
{
    void SetMaximize(bool set);
}

public class ThoughtBoardObject : MonoBehaviour, IThoughtBoardObject
{

    public LHH.Switchable.Switchable maximizedSwitchable;
    // Start is called before the first frame update

    public void SetMaximize(bool set)
    {
        maximizedSwitchable?.Set(set);
    }
}
