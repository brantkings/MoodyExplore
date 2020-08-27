using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Code.Animation.Humanoid
{
    public interface ILookAtPosition
    {
        Vector3 GetTargetPosition();
        bool IsRelative { get; }
    }

    public interface ILookAtPositionWeighted : ILookAtPosition
    {
        float Weight { get; }
    }

    [System.Serializable]
    public class LookAtPositionTargetSource : ILookAtPositionWeighted
    {
        [SerializeField]
        private Transform _target;

        public float weight = 1f;

        
        

        public LookAtPositionTargetSource(Transform t, float weightTarget = 1f)
        {
            _target = t;
            weight = weightTarget;
        }
        public Vector3 GetTargetPosition()
        {
            return _target != null? _target.position : Vector3.zero;
        }

        public bool IsRelative => false;

        public float Weight => _target != null? weight : 0f;
    }
    
    public class LookAtIK : MonoBehaviour
    {
        public Transform headTransform;

        private List<ILookAtPosition> _positionSources;

        private Vector3 _plainDirection;
        private float _plainWeight;
        
        private List<ILookAtPosition> PositionSources
        {
            get
            {
                if(_positionSources == null) _positionSources = new List<ILookAtPosition>(8);
                return _positionSources;
            }
        }
        public Animator anim;

        public ILookAtPosition AddPositionSource(ILookAtPosition src)
        {
            PositionSources.Add(src);
            return src;
        }

        public void RemovePositionSource(ILookAtPosition src)
        {
            PositionSources.Remove(src);
        }

        public void LookAt(Vector3 direction, float weightWithSources = 1f)
        {
            _plainDirection = direction;
            _plainWeight = weightWithSources;
        }

        public void UnlookAt()
        {
            _plainDirection = Vector3.zero;
            _plainWeight = 0f;
        }

        private Vector3 GetPositionSource(out float weightTotal)
        {
            Vector3 position = GetPositionFromDirection(_plainDirection);
            weightTotal = _plainWeight;
            foreach (ILookAtPosition src in PositionSources)
            {
                ILookAtPositionWeighted posWeight = src as ILookAtPositionWeighted;
                float weight = 1f;
                if(posWeight != null) weight = posWeight.Weight;

                if (src.IsRelative)
                {
                    position += GetPositionFromDirection(src.GetTargetPosition()) * weight;
                }
                else
                {
                    position += src.GetTargetPosition() * weight;
                }
                weightTotal += weight;
            }

            if (weightTotal != 0f) 
                return position / weightTotal;
            else 
                return Vector3.zero;
        }

        private Vector3 GetPositionFromDirection(Vector3 direction)
        {
            return headTransform.position + direction;
        }
        
        private void OnAnimatorIK(int layerIndex)
        {
            Vector3 pos = GetPositionSource(out float weight);
            anim.SetLookAtPosition(pos);
            anim.SetLookAtWeight(Mathf.Clamp01(weight));
        }

    }
}
