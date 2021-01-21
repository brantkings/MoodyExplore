using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InstantiateUtility
{
    public GameObject prefab;

    public bool IsValid()
    {
        Debug.LogFormat("{0} is valid!", prefab);
        return prefab != null;
    }

    public GameObject Instantiate(Transform where)
    {
        return Instantiate(where, Vector3.zero, Quaternion.identity);
    }

    public GameObject Instantiate(Transform where, Vector3 offsetPosition, Quaternion offsetRotation)
    {
        Debug.LogFormat("Instiating {0} at {1}", prefab, where);
        GameObject inst = ReusableManager.Instance.Instantiate(prefab);
        inst.transform.position = where.position + offsetPosition;
        inst.transform.rotation = offsetRotation * where.rotation;
        return inst;
    }

    public GameObject Instantiate(Vector3 wherePosition, Quaternion whereRotation)
    {
        GameObject inst = ReusableManager.Instance.Instantiate(prefab);
        inst.transform.position = wherePosition;
        inst.transform.rotation = whereRotation;
        return inst;
    }
}
