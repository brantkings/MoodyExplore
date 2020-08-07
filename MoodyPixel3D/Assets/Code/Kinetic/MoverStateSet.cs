using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Mover))]
public class MoverStateSet : StateSet<Mover>
{
    [System.Serializable]
    public class MoverStateSetup : StateSetup
    {
        [Space()]
        public float speed;

        public override void SetState(Mover type)
        {
            type.SetSpeed(speed);
        }
    }

    [SerializeField]
    public MoverStateSetup[] _moverStates;

    protected override StateSetup[] States
    {
        get
        {
            return _moverStates;
        }
    }

}
