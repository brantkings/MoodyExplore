using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CustomPhysicsBody))]
public class CustomPhysicsController : MonoBehaviour
{
    CustomPhysicsBody _body;
    protected CustomPhysicsBody Body
    {
        get
        {
            if (_body == null) _body = GetComponent<CustomPhysicsBody>();
            return _body;
        }
    }
}
