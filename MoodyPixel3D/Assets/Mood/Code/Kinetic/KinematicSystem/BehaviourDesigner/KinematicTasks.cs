using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace BehaviorDesigner.Runtime.Tasks.Kinematic
{
    public abstract class KinematicPlatformerAction : Action
    {
        [SerializeField] private KinematicSharedBehaviourTypes.SharedKinematicPlatformer kinematicPlatformer;

        public override TaskStatus OnUpdate()
        {
            KinematicPlatformer plat = kinematicPlatformer.Value;
            if (plat == null) return TaskStatus.Failure;
            return Update(plat);
        }

        protected KinematicPlatformer Platformer
        {
            get
            {
                return kinematicPlatformer.Value;
            }
        }

        public abstract TaskStatus Update(KinematicPlatformer platformer);
    }

    public abstract class KinematicPlatformerConditional : Conditional
    {
        [SerializeField] private KinematicSharedBehaviourTypes.SharedKinematicPlatformer kinematicPlatformer;

        public override TaskStatus OnUpdate()
        {
            KinematicPlatformer plat = kinematicPlatformer.Value;
            if (plat == null) return TaskStatus.Failure;
            return Update(plat);
        }
        
        protected KinematicPlatformer Platformer
        {
            get
            {
                return kinematicPlatformer.Value;
            }
        }

        public abstract TaskStatus Update(KinematicPlatformer platformer);
    }

    [TaskCategory("Kinematic")]
    public class MoveDirection : KinematicPlatformerAction
    {
        [SerializeField] private SharedVector3 movementPerSecond;
        [SerializeField] private SharedBool infinite;
        [SerializeField] private SharedFloat amount;

        private float amountWalked;

        public override void OnStart()
        {
            amountWalked = amount.Value;
        }

        public override TaskStatus Update(KinematicPlatformer platformer)
        {
            Platformer.SetVelocity(movementPerSecond.Value);
            amountWalked -= Time.deltaTime * movementPerSecond.Value.magnitude;
            if (infinite.Value) return TaskStatus.Running;
            else
            {
                if (amountWalked <= 0f) return TaskStatus.Success;
                return TaskStatus.Running;
            }
        }
    }

    [TaskCategory("Kinematic")]
    public class SetVelocity : KinematicPlatformerAction
    {
        [SerializeField] private SharedVector3 velocity;
        public override TaskStatus Update(KinematicPlatformer platformer)
        {
            platformer.SetVelocity(velocity.Value);
            return TaskStatus.Success;
        }
    }

    [TaskCategory("Kinematic")]
    public class GetWhereIAmTryingToGoNormal : KinematicPlatformerAction
    {
        [SerializeField] private SharedVector3 outNormal;
        [SerializeField] private SharedBool notFindingAnythingIsNotFailure;
        public KinematicPlatformer.CasterClass caster = KinematicPlatformer.CasterClass.Side;


        public override TaskStatus Update(KinematicPlatformer platformer)
        {
            if(platformer.WhatIsWhereIAmTryingToGo(caster, out RaycastHit hit))
            {
                outNormal.Value = hit.normal;
                return TaskStatus.Success;
            }
            else
            {
                outNormal.Value = Vector3.zero;
                if (notFindingAnythingIsNotFailure.Value) return TaskStatus.Success;
                else return TaskStatus.Failure;
            }

        }
    }

    [TaskCategory("Kinematic")]
    public class IsTouchingWall : KinematicPlatformerConditional
    {
        public override TaskStatus Update(KinematicPlatformer platformer)
        {
            if (platformer.Walled)
            {
                return TaskStatus.Success;
            }
            else return TaskStatus.Failure;
        }
    }

    [TaskCategory("Kinematic")]
    public class IsGrounded : KinematicPlatformerConditional
    {
        public override TaskStatus Update(KinematicPlatformer platformer)
        {
            if (platformer.Grounded)
            {
                return TaskStatus.Success;
            }
            else return TaskStatus.Failure;
        }
    }

    [TaskCategory("Kinematic")]
    public class IsMovingHorizontally : KinematicPlatformerConditional
    {
        public override TaskStatus Update(KinematicPlatformer platformer)
        {
            Vector3 v = platformer.Velocity;
            v.y = 0f;
            if (v.sqrMagnitude > 0f) return TaskStatus.Success;
            else return TaskStatus.Failure;
        }
    }

    [TaskCategory("Kinematic")]
    public class IsMovingVertically : KinematicPlatformerConditional
    {
        public override TaskStatus Update(KinematicPlatformer platformer)
        {
            Vector3 v = platformer.Velocity;
            if (v.y != 0f) return TaskStatus.Success;
            else return TaskStatus.Failure;
        }
    }

    [TaskCategory("Kinematic")]
    public class IsMoving : KinematicPlatformerConditional
    {
        public override TaskStatus Update(KinematicPlatformer platformer)
        {
            Vector3 v = platformer.Velocity;
            if (v.sqrMagnitude > 0f) return TaskStatus.Success;
            else return TaskStatus.Failure;
        }
    }
}
