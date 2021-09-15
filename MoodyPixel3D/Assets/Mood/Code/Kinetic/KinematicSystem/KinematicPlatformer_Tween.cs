 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;

public partial class KinematicPlatformer
{

    private DG.Tweening.Core.DOSetter<Vector3> SetPawnLerpSpecificPriorityDiff(int priority)
    {
        return (Vector3 diff) =>
        {
            AddExactNextFrameMove(diff, priority);
#if UNITY_EDITOR
            if (diff.IsNaN())
                Debug.LogErrorFormat("{0} setting lerp position NaN! {0} {1} + {2}", name, Position, diff);
#endif
        };
    }

    // Maybe this is useful if the tween is dando pau
    private class TweenedValue<T>
    {
        public T value;
        public bool used;
    }

    private void CalculatePosition<T>(ref List<TweenedValue<T>> list, out int positionInList)
    {
        if (list == null) list = new List<TweenedValue<T>>(16);

        positionInList = -1;
        if (list.Count > 0)
        {
            positionInList = list.FindIndex((x) => x.used == false);
        }
        if (positionInList == -1)
        {
            list.Add(new TweenedValue<T>());
            positionInList = list.Count - 1;
        }
    }

    private List<TweenedValue<Vector3>> _localPositions;
    private int _latestLocalPosition;

    private int commentIndex;


    public class PriorityVelocityOperation
    {
        public Vector3 comparer;
        public int priorityOrLess;
        public Origin origin = Origin.GetNextFrameFullVelocity;
        public Modification modification = Modification.ProjectOnPositiveVector;
        public Operation operation = Operation.Sum;

        private PriorityVelocityOperation(Vector3 compareTo, int priorityOrLess, Origin origin, Modification modification, Operation operation)
        {
            this.comparer = compareTo;
            this.priorityOrLess = priorityOrLess;
            this.origin = origin;
            this.modification = modification;
            this.operation = operation;
        }

        public static PriorityVelocityOperation Get(Vector3 compareTo, int priorityOrLess, Origin origin, Modification modification, Operation operation)
        {
            return new PriorityVelocityOperation(compareTo, priorityOrLess, origin, modification, operation);
        }

        public enum Origin
        {
            GetNextFrameFullVelocity,
            AbsoluteComparer,
            Zero,
        }

        public enum Modification
        {
            Nothing,
            ProjectOnVector,
            ProjectOnPositiveVector,
            ProjectOnPlane
        }

        public enum Operation
        {
            Sum,
            Subtract
        }

        public void Operate(ref Vector3 originalVel, KinematicPlatformer plat)
        {
            Vector3 value;

            switch (origin)
            {
                case Origin.GetNextFrameFullVelocity:
                    value = plat.GetNextFrameFullVelocity(priorityOrLess);
                    break;
                case Origin.AbsoluteComparer:
                    value = comparer;
                    break;
                default:
                    value = Vector3.zero;
                    break;
            }

            if(comparer != Vector3.zero)
            {
                switch (modification)
                {
                    case Modification.ProjectOnVector:
                        value = Vector3.Project(value, comparer);
                        break;
                    case Modification.ProjectOnPositiveVector:
                        float cos = Vector3.Dot(value, comparer);
                        value = Vector3.Project(value, comparer);
                        value *= Mathf.Max(Mathf.Sign(cos), 0f);
                        break;
                    case Modification.ProjectOnPlane:
                        value = Vector3.ProjectOnPlane(value, comparer);
                        break;
                }
            }

            switch (operation)
            {
                case Operation.Sum:
                    originalVel += value;
                    break;
                case Operation.Subtract:
                    originalVel -= value;
                    break;
            }

        }


    }

    public Tween TweenMoverPosition(Vector3 movement, float duration, int priority = 0, PriorityVelocityOperation op = null, string comment = "")
    {
        var setAndMove = SetPawnLerpSpecificPriorityDiff(priority);

        if (duration == 0f)
        {
            setAndMove(movement);
            return null;
        }
        else
        {
            CalculatePosition(ref _localPositions, out int index);

            _localPositions[index].used = true;
            Vector3 p = Position;
            Vector3 initP = Position;
            _localPositions[index].value = Position;
            comment = comment + "_" + commentIndex++;
            int commentCount = 0;

            DG.Tweening.Core.DOSetter<Vector3> setter;
            if (op != null)
            {
                KinematicPlatformer plat = this;
                setter = (x) =>
                {
                    Vector3 diff = x - p;
                    op.Operate(ref diff, plat);
                    setAndMove(diff);
                    Debug.LogWarningFormat("{0} is going begin: {1} now is {2} going to be {3} -> diff is {4} (operated: {5}) ({6})", this, initP.ToString("F3"), p.ToString("F3"), x.ToString("F3"), (x - p).ToString("F3"), diff.ToString("F3"), comment + "_" + commentCount++);
                    p = x;
                };
            }
            else
            {
                setter = (x) =>
                {
                    Vector3 diff = x - p;
                    setAndMove(diff);
                    Debug.LogWarningFormat("{0} is going begin: {1} now is {2} going to be {3} -> diff is {4} ({5})", this, initP.ToString("F3"), p.ToString("F3"), x.ToString("F3"), (x - p).ToString("F3"), comment + "_" + commentCount++);
                    p = x;
                };
            }

            DG.Tweening.Core.DOGetter<Vector3> getter = () =>
            {
                return p;
            };

            Tween t = DOTween.To(getter, setter, movement, duration).SetId(this).SetRelative(true).SetUpdate(UpdateType.Fixed);
            return t;
        }

    }


    /*
    public Tween TweenMoverDirection(float angleAdd, float duration)
    {
        Vector3 directionAdd = Quaternion.Euler(0f, angleAdd, 0f) * Direction;
        return TweenMoverDirection(directionAdd, duration);
    }

    //This should be called directly on MoodPawn, or else it won't work because MoodPawn needs to control the direction
    public Tween TweenMoverDirection(Vector3 directionTo, float duration)
    {
        Debug.LogFormat("{0} is rotating to direction {1}, {2} [{3}]", this, directionTo, duration, Time.time);
        if (duration <= 0f)
        {
            SetPawnLerpDirection(directionTo);
            return null;
        }
        else return DOTween.To(GetPawnLerpDirection, SetPawnLerpDirection, directionTo, duration).SetId(this).OnKill(()=> Debug.LogFormat("Killed me {0} {1} [{2}]!", this, directionTo, Time.time));//.OnKill(CallEndMove).OnStart(CallBeginMove);
    }
    */
}
