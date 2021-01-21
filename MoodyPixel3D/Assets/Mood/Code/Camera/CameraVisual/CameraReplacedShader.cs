using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CameraReplacedShader : AddonBehaviour<Camera>
{
    public Shader shader;
    public string replacementTag = "Mask";


    public void OnEnable()
    {
        Addon.SetReplacementShader(shader, replacementTag);
    }

    private void OnDisable()
    {
        Addon.ResetReplacementShader();
    }

    private void Update()
    {
        Addon.SetReplacementShader(shader, replacementTag);
    }
}
