using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class MoodGameManager : PersistentSingleton<MoodGameManager>
{
    [SerializeField]
    private LayerMask _pawnlayer;

    public LayerMask GetPawnBodyLayer()
    {
        return _pawnlayer;
    }
}
