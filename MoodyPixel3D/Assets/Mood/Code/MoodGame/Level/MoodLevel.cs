using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoodLevel : MonoBehaviour
{
    public LayerMask groundMask;

    private MoodLevelRoom[] _roomsCache;
    

    private MoodLevelRoom[] Rooms
    {
        get
        {
            if (_roomsCache == null) _roomsCache = GetComponentsInChildren<MoodLevelRoom>();
            return _roomsCache;
        }
    }

    private void Start()
    {
        MoodLevelRoom playerRoom = GetPlayerRoom();

        if(playerRoom != null)
        {
            foreach(var room in Rooms)
            {
                Debug.LogWarningFormat("Got room '{0}' and going to activate? '{1}'", room, room == playerRoom);
                if (room == playerRoom) room.StartActivationRoutine(true, true);
                else room.StartActivationRoutine(false, true);
            }
        }

    }

    public MoodLevelRoom GetPlayerRoom()
    {
        if (MoodPlayerController.Instance != null)
            return GetRoom(MoodPlayerController.Instance.Pawn.Position);
        else return null;
    }

    public MoodLevelRoom GetRoom(Vector3 pos)
    {
        Ray ray = new Ray(pos + Vector3.up * 3f, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 10f, groundMask.value, QueryTriggerInteraction.Ignore))
        {
            Debug.LogFormat("Got {0}!", hit.collider.GetComponentInParent<MoodLevelRoom>());
            return hit.collider.GetComponentInParent<MoodLevelRoom>();
        }
        Debug.LogFormat("Got nothing!");
        return null;
    }


}
