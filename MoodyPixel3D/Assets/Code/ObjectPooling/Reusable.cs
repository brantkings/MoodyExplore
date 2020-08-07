using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IReusable
{
    void Begin();
    void Stop();

    bool IsRunning();

    GameObject gameObject { get; }
    Transform transform { get; }
}


public class Reusable : MonoBehaviour, IReusable
{
    public void Begin()
    {
        gameObject.SetActive(true);
    }

    public void Stop()
    {
        gameObject.SetActive(false);
    }

    public bool IsRunning()
    {
        return gameObject.activeInHierarchy;
    }
}
