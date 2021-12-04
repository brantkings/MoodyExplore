using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MoodLevelRoom : MonoBehaviour
{
    public string faderTag = "Fader";
    public string fadeProperty = "_FadeValue";
    private int _fadePropertyID;

    public float fadeTime = 0.45f;
    public Ease fadeOutEase = Ease.OutSine;
    public Ease fadeInEase = Ease.OutSine;

    public GameObject[] toActivateWithRoom;

    public enum Status
    {
        Deactivated,
        Activating,
        Activated,
        Deactivating
    }

    private Status? _status;

    public Status GetStatus()
    {
        return _status.HasValue? _status.Value : Status.Activated;
    }

    public bool IsActivated()
    {
        switch (GetStatus())
        {
            case Status.Deactivated:
                return false;
            case Status.Activating:
                return true;
            case Status.Activated:
                return true;
            case Status.Deactivating:
                return false;
            default:
                return false;
        }
    }

    private void Awake()
    {
        _fadePropertyID = Shader.PropertyToID(fadeProperty);

        StartCoroutine(SetRoomActivateRoutine(false, true));
    }

    public void SetRoomActive(bool active, bool immediate = false)
    {
        StartCoroutine(SetRoomActivateRoutine(active, immediate));
    }

    public IEnumerator SetRoomActivateRoutine(bool active, bool immediate)
    {
        Debug.LogFormat("Setting room '{0}' {1}. Status now is {2}", this.name, active, _status);

        if (active)
        {
            if (_status.HasValue && (_status.Value == Status.Activated || _status.Value == Status.Activating))
                yield break;

            _status = Status.Activating;
            SetImmediatelyActivated(true);
            yield return ActivatingRoutine(true, immediate);
            _status = Status.Activated;
        }
        else
        {
            if (_status.HasValue && (_status.Value == Status.Deactivating || _status.Value == Status.Deactivated))
                yield break;

            _status = Status.Deactivating;
            yield return ActivatingRoutine(false, immediate);
            SetImmediatelyActivated(false);
            _status = Status.Deactivated;
        }
    }

    private bool IsFadeable(Renderer rend)
    {
        return !string.IsNullOrEmpty(rend.material.GetTag(faderTag, false));
    }

    protected virtual IEnumerable<Renderer> AllFadeableWalls()
    {
        foreach (Renderer rend in GetComponentsInChildren<Renderer>())
        {
            if (IsFadeable(rend))
            {
                yield return rend;
            }
        }
    }

    protected virtual IEnumerator ActivatingRoutine(bool active, bool immediate)
    {
        bool foundOne = false;
        foreach(Renderer rend in AllFadeableWalls())
        {
            Debug.LogFormat("{0} is activating? {1}", rend, active);
            foundOne = true;
            if(immediate)
            {
                rend.material.SetFloat(_fadePropertyID, active ? 1f : 0f);
            }
            else
            {
                rend.material.SetFloat(_fadePropertyID, active ? 0f : 1f);
                rend.material.DOFloat(active ? 1f : 0f, _fadePropertyID, fadeTime).SetEase(active? fadeInEase : fadeOutEase);
            }
        }
        if (foundOne && !immediate) yield return new WaitForSeconds(fadeTime);
    }


    protected virtual void SetImmediatelyActivated(bool active)
    {
        foreach(GameObject obj in toActivateWithRoom)
        {
            if(obj != null) obj.SetActive(active);
        }

        foreach (Renderer rend in AllFadeableWalls())
        {
            rend.enabled = active;
        }
        foreach(Light light in GetComponentsInChildren<Light>(true))
        {
            light.gameObject.SetActive(active);
        }
        foreach (MoodPawn p in GetComponentsInChildren<MoodPawn>(true))
        {
            if(p != null)
            {
                p.gameObject.SetActive(active);
            }
        }
    }
    
    [LHH.Unity.Button]
    public void Deactivate()
    {
        StopAllCoroutines();
        StartCoroutine(SetRoomActivateRoutine(false, false));
    }

    [LHH.Unity.Button]
    public void Activate()
    {
        StopAllCoroutines();
        StartCoroutine(SetRoomActivateRoutine(true, false));
    }
    public void StartActivationRoutine(bool active, bool immediate)
    {
        StopAllCoroutines();
        StartCoroutine(SetRoomActivateRoutine(active, immediate));
    }

}
