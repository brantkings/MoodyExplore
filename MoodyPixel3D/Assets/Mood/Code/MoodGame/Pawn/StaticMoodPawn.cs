using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticMoodPawn : MonoBehaviour
{
    [SerializeField] private Animator _animator;

    public Animator Animator
    {
        get
        {
            return _animator;
        }
    }

    public interface IPose
    {
        void SetPose(Animator anim);
    }

    public abstract class Pose<T> : IPose
    {
        public LHH.AnimationParameterID param;
        public T value;

        protected int GetParam()
        {
            return param.GetId();
        }

        public abstract void SetPose(Animator anim);
    }

    [System.Serializable]
    public class PoseInt : Pose<int>
    {
        public override void SetPose(Animator anim)
        {
            anim.SetInteger(GetParam(), value);
        }
    }

    public PoseInt basePose;

    private void Awake()
    {
        if (_animator == null) _animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        basePose.SetPose(Animator);
    }
}
