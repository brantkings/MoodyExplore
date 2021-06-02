﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LHH.Switchable
{

    [RequireComponent(typeof(Switchable))]
    public abstract class SwitchableEffect : MonoBehaviour {

        protected Switchable _switch;

        protected virtual void Awake()
        {
            _switch = GetComponent<Switchable>();
        }

        private void OnEnable()
        {
            _switch.OnAfterSwitch += Effect;
        }

        private void OnDisable()
        {
            _switch.OnAfterSwitch -= Effect;
        }

        abstract protected void Effect(bool on);
    }

}
