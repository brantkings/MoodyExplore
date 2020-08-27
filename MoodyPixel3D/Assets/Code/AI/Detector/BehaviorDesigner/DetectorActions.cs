using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace BehaviorDesigner.DetectorTasks
{
    [TaskCategory("Detector")]
    public class IsDetecting : Conditional
    {
        public SharedDetector detector;
        
        public override TaskStatus OnUpdate()
        {
            if (detector.Value != null) return detector.Value.IsDetecting ? TaskStatus.Success : TaskStatus.Failure;
            else return TaskStatus.Failure;
        }
    }
    
    [TaskCategory("Detector")]
    public class IsNotDetecting : Conditional
    {
        public SharedDetector detector;
        
        public override TaskStatus OnUpdate()
        {
            if (detector.Value != null) return detector.Value.IsDetecting ? TaskStatus.Failure : TaskStatus.Success;
            else return TaskStatus.Success;
        }
    }
}