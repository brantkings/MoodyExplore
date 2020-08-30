using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothFixedStepWalk : MonoBehaviour
{

    private abstract class Interpolator<T>
    {
        protected const int AMOUNT = 2;
        protected Queue<T> values;

        public Interpolator()
        {
            StartValues();
        }

        private void StartValues()
        {
            if(values == null)
            {
                values = new Queue<T>(AMOUNT + 1);
            }
        }

        public void AddValue(T val)
        {
            values.Enqueue(val);
            if (values.Count > AMOUNT) values.Dequeue();
        }

        public void ClearValues()
        {
            values.Clear();
        }

        public T GetValue(float time)
        {
            T v1 = default, v2 = default;
            int i = 0, len = values.Count;
            if (len == 0) return default;
            else if (len == 1) return values.Peek();
            else
            {
                foreach(T v in values)
                {
                    if (i == 0) v1 = v;
                    else if (i == 1) v2 = v;
                    else if (i == AMOUNT) break;
                    i++;
                }
            }
            //Debug.LogWarningFormat("Lerping between {0} and {1} on time {3} is {2}", debug(v1), debug(v2), debug(Lerp(v1, v2, time)), time);
            return Lerp(v1, v2, time);
        }

        protected abstract string debug(T v);

        public abstract T Lerp(T a, T b, float t);
    }

    private class PositionInterpolator : Interpolator<Vector3>
    {
        protected override string debug(Vector3 v)
        {
            return v.ToString("F3");
        }

        public override Vector3 Lerp(Vector3 a, Vector3 b, float time)
        {
            return Vector3.Lerp(a, b, time);
        }
    }

    private class RotationInterpolator : Interpolator<Quaternion>
    {
        protected override string debug(Quaternion v)
        {
            return v.ToString("F3");
        }

        public override Quaternion Lerp(Quaternion a, Quaternion b, float time)
        {
            return Quaternion.Lerp(a, b, time);
        }
    }
    [SerializeField]
    Transform followee;

    public bool doPosition = true;
    public bool doRotation = true;

    PositionInterpolator pos;
    RotationInterpolator rot;


    private void Awake()
    {
        Application.targetFrameRate = 60;
        pos = new PositionInterpolator();
        rot = new RotationInterpolator();
    }
    private void OnEnable()
    {
        transform.SetPositionAndRotation(followee.position, followee.rotation);
        pos.ClearValues();
        rot.ClearValues();
        pos.AddValue(followee.position);
        rot.AddValue(followee.rotation);
    }

    private void FixedUpdate()
    {
        if (doPosition) pos.AddValue(followee.position);
        if (doRotation) rot.AddValue(followee.rotation);
    }

    void LateUpdate()
    {
        float t = (Time.time - Time.fixedTime) / Time.fixedDeltaTime;
        transform.SetPositionAndRotation(pos.GetValue(t), rot.GetValue(t));
    }
}
