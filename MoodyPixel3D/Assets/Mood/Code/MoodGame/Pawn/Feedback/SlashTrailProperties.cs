using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlashTrailProperties : MonoBehaviour
{
    public int priority = 0;
    [SerializeField]
    private Material material;
    public Transform top;
    public Transform bottom;

    public Material GetMaterial()
    {
        return material;
    }
}
