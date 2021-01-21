using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace Code.MoodGame.BehaviourDesigner
{
    
    [TaskCategory("Unity/Vector3")]
    public class RotateVector3Euler : Action
    {
        [SerializeField] private SharedVector3 inVector;
        [SerializeField] private SharedVector3 euler;
        [SerializeField] private SharedVector3 outVector;

        public override TaskStatus OnUpdate()
        {
            outVector.Value = Quaternion.Euler(euler.Value) * inVector.Value;
            return TaskStatus.Success;
        }
    }
}
