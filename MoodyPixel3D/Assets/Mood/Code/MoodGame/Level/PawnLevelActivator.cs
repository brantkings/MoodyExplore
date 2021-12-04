using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PawnLevelActivator : AddonBehaviour<MoodPawn>
{
    MoodLevelRoom _currentLevel;

    private void OnEnable()
    {
        Addon.mover.OnChangePlatform += OnChangePlatform;
    }

    private void OnDisable()
    {
        Addon.mover.OnChangePlatform -= OnChangePlatform;
    }

    private void OnChangePlatform(Collider oldPlat, Collider newPlat)
    {
        MoodLevelRoom newLevel = newPlat.GetComponentInParent<MoodLevelRoom>();

        if (newLevel != null)
        {
            if(_currentLevel != null)
            {
                _currentLevel.SetRoomActive(false, false);
            }

            if(!newLevel.IsActivated())
            {
                newLevel.SetRoomActive(true, false);
            }

            _currentLevel = newLevel;
        }
    }
}
