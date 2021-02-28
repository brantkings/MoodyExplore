using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace BehaviorDesigner.DetectorTasks
{
    [TaskCategory("Detector")]
    public class IsDetecting : Conditional
    {
        [SharedRequired]
        public SharedDetector detector;
        
        public override TaskStatus OnUpdate()
        {
            if (detector.Value != null) return detector.Value.IsDetecting ? TaskStatus.Success : TaskStatus.Failure;
            else
            {
                Debug.LogErrorFormat("No detector in variable '{0}' of '{1}'.", detector, transform.name);
                return TaskStatus.Failure;
            }
        }
    }
    
    [TaskCategory("Detector")]
    public class IsNotDetecting : Conditional
    {
        [SharedRequired]
        public SharedDetector detector;
        
        public override TaskStatus OnUpdate()
        {
            if (detector.Value != null) return detector.Value.IsDetecting ? TaskStatus.Failure : TaskStatus.Success;
            else
            {
                Debug.LogErrorFormat("No detector in variable '{0}' of '{1}'.", detector, transform.name);
                return TaskStatus.Failure;
            }
        }
    }
}