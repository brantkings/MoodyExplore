using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReusableManager : MonoBehaviour
{
    private static ReusableManager _instance;
    public static ReusableManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject obj = new GameObject("[REUSABLE MANAGER]");
                _instance = obj.AddComponent<ReusableManager>();
            }
            return _instance;
        }
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public GameObject Instantiate(GameObject prefab)
    {
        return Instantiate(prefab, out IReusable unused);
    }

    public GameObject Instantiate(GameObject prefab, out IReusable newInstance)
    {
        IReusable reusable = prefab.GetComponent<IReusable>();
        if (reusable != null)
        {
            newInstance = Instantiate(reusable);
            return newInstance.gameObject;
        }
        else
        {
            newInstance = null;
            return Instantiate<GameObject>(prefab);
        }
    }

    public IReusable Instantiate(IReusable prefab)
    {
        Debug.LogFormat("Instantiating {0}", prefab);
        IReusable newInstance = Instantiate(prefab.gameObject, null).GetComponent<IReusable>();
        newInstance.Begin();
        return newInstance;
    }
}
