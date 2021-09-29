using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace Code.MoodGame.BehaviourDesigner
{
    
    [TaskCategory("Unity/Vector3")]
    public class RotateVector3Euler : Action
    {
        [SerializeField] [RequiredField] private SharedVector3 inVector;
        [SerializeField] private SharedVector3 euler;
        [SerializeField] [RequiredField] private SharedVector3 outVector;

        public override TaskStatus OnUpdate()
        {
            outVector.Value = Quaternion.Euler(euler.Value) * inVector.Value;
            return TaskStatus.Success;
        }
    }

    [TaskCategory("Unity/Math")]
    public class InverseLerp : Action
    {
        [SerializeField] [RequiredField] private SharedFloat inMin;
        [SerializeField] [RequiredField] private SharedFloat inMax;
        [SerializeField] [RequiredField] private SharedFloat inValue;
        [SerializeField] [RequiredField] private SharedFloat outValue;

        public override TaskStatus OnUpdate()
        {
            outValue.Value = Mathf.InverseLerp(inMin.Value, inMax.Value, inValue.Value);
            return TaskStatus.Success;
        }
    }
}
