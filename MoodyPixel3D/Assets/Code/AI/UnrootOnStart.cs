using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnrootOnStart : MonoBehaviour
{
    public bool disableOnAwake;

    private void Awake()
    {
        if (disableOnAwake) gameObject.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        transform.SetParent(null);   
    }
}
