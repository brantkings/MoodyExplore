using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LHH.Singleton;
using System.Linq;

namespace LHH.Caster
{
    public class CasterDebugger : CreateablePersistentSingleton<CasterDebugger>
    {
#if UNITY_EDITOR
        [System.Serializable]
        private struct FrameData
        {
            public int amountOfCasts;
            public float sumOfLengths;
        }
           
        public const int AmountFramesLooking = 256;

        [SerializeField]
        private FrameData _lastFrame;
        [SerializeField]
        private FrameData _maxes;
        [SerializeField]
        private FrameData _mean;
        private Queue<FrameData> _latestFrames;
        private FrameData _ongoingFrame;
        protected override void Awake()
        {
            base.Awake();
            _latestFrames = new Queue<FrameData>(AmountFramesLooking);
        }

        public void LateUpdate()
        {
            _lastFrame = _ongoingFrame;
            _latestFrames.Enqueue(_lastFrame);
            while (_latestFrames.Count > AmountFramesLooking) _latestFrames.Dequeue();
            _maxes.amountOfCasts = _latestFrames.Max((x) => x.amountOfCasts);
            _maxes.sumOfLengths = _latestFrames.Max((x) => x.sumOfLengths);
            _mean.amountOfCasts = Mathf.RoundToInt((float)_latestFrames.Average(x=> x.amountOfCasts));
            _mean.sumOfLengths = _latestFrames.Average(x=> x.sumOfLengths);
            _ongoingFrame = new FrameData();
        }

        public void JustDoneCast(Caster cast, float length)
        {
            _ongoingFrame.amountOfCasts++;
            _ongoingFrame.sumOfLengths += length;
        }
#endif
    }
}