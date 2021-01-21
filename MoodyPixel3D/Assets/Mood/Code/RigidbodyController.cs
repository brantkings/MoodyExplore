using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class RigidbodyController : MonoBehaviour {

    [SerializeField]
    private Rigidbody _body;

    protected Rigidbody Body
    {
        get
        {
            return _body;
        }
    }

    protected virtual void Reset()
    {
        _body = GetComponentInParent<Rigidbody>();
    }
}
