using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointsAsObjectsUI : MonoBehaviour
{
    public GameObject prefabObject;

    List<GameObject> _objects = new List<GameObject>(3);

    public void SetNPoints(int n)
    {
        if (_objects.Count >= n)
        {
            foreach (var obj in _objects)
            {
                if(n > 0)
                {
                    obj.SetActive(true);
                    n -= 1;
                }
                else
                {
                    obj.SetActive(false);
                }
            }
        }
        else
        {
            foreach (var obj in _objects)
            {
                obj.SetActive(true);
            }

            while (_objects.Count < n)
            {
                var obj = Instantiate(prefabObject, transform);
                obj.SetActive(true);

                _objects.Add(obj);
            }
        }
    }
}
