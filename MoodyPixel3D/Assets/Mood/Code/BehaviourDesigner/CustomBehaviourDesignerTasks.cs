using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks
{

    [TaskCategory("Unity/Vector2")]
    public class ReflectVector2 : Action
    {
        public SharedVector2 inDirection;
        public SharedVector2 inNormal;
        public SharedVector2 outReflected;

        public override TaskStatus OnUpdate()
        {
            outReflected.Value = Vector3.Reflect(inDirection.Value, inNormal.Value);
            return base.OnUpdate();
        }
    }

    [TaskCategory("Unity/Vector3")]
    public class ReflectVector3 : Action
    {
        public SharedVector3 inDirection;
        public SharedVector3 inNormal;
        public SharedVector3 outReflected;

        public override TaskStatus OnUpdate()
        {
            outReflected.Value = Vector3.Reflect(inDirection.Value, inNormal.Value);
            Debug.LogFormat("Reflected {0} with {1} result is {2}", inDirection.Value, inNormal.Value, outReflected.Value);
            return TaskStatus.Success;
        }
    }

    [TaskCategory("Unity/Transform")]
    public class TransformDirection : Action
    {
        public SharedVector3 inVector;
        public SharedTransform transformer;
        public SharedVector3 outVector3;

        public override TaskStatus OnUpdate()
        {
            outVector3.Value = transformer.Value.TransformDirection(inVector.Value);
            return TaskStatus.Success;
        }
    }

    [TaskCategory("Unity/Transform")]
    public class TransformDirectionToThisTransform : Action
    {
        public SharedVector3 inVector;
        public SharedVector3 outVector3;

        public override TaskStatus OnUpdate()
        {
            outVector3.Value = transform.TransformDirection(inVector.Value);
            return TaskStatus.Success;
        }
    }

}
