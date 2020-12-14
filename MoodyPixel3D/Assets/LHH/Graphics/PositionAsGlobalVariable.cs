using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace LHH.Graphics
{
    [ExecuteInEditMode]
    public class PositionAsGlobalVariable : MonoBehaviour
    {
        public string variableName;

        private int id;

        private void Start()
        {
            id = Shader.PropertyToID(variableName);
        }

        void LateUpdate()
        {
            if(id != 0)
                Shader.SetGlobalVector(id, transform.position);
        }
    }
}
