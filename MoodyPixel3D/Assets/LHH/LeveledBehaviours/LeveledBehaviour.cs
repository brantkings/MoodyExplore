using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LHH.LeveledBehaviours
{
    public abstract class LeveledBehaviour : MonoBehaviour
    {

        bool _wasEverInitalized;
        float _currentLevel;

        protected virtual void Start()
        {
            if (!_wasEverInitalized)
                SetLevel(0);
        }

        protected abstract void ApplyLevel(float level);

        public void SetLevel(float level)
        {
            _wasEverInitalized = true;

            if (_currentLevel != level)
            {
                _currentLevel = level;
                ApplyLevel(_currentLevel);
            }
        }

        public float GetLevel()
        {
            return _currentLevel;
        }
    }

}
