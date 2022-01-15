﻿/******************************************************************************/
/*
  Project - Unity Ray Marching
            https://github.com/TheAllenChou/unity-ray-marching
  
  Author  - Ming-Lun "Allen" Chou
  Web     - http://AllenChou.net
  Twitter - @TheAllenChou
*/
/******************************************************************************/

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
public class PostProcessingBase : MonoBehaviour
{
  public bool ApplyToSceneCamera = true;

  #if UNITY_EDITOR
  private Camera m_sceneCamera;
  private PostProcessingBase m_postProcessing;
  private bool m_appliedToSceneCamera = false;
  private bool m_attachedToSceneCamera = false;
  #endif

  private void DisposeSceneCamera()
  {
    if (m_postProcessing != null)
    {
      DestroyImmediate(m_postProcessing);
    }

    m_sceneCamera = null;
    m_postProcessing = null;
    m_appliedToSceneCamera = false;
  }

  private void Update()
  {
    #if UNITY_EDITOR
    UpdateSceneCamera();
    #endif
  }

  protected virtual void OnValidate()
  {
    #if UNITY_EDITOR
    // reboot component copy to scene camera if changed in editor
    m_appliedToSceneCamera = false;
    #endif
  }

  #if UNITY_EDITOR
  private void UpdateSceneCamera()
  {
    if (m_attachedToSceneCamera)
      return;

    if (!ApplyToSceneCamera)
    {
      DisposeSceneCamera();
      return;
    }

    if (m_appliedToSceneCamera)
      return;

    var window = EditorWindow.GetWindow<SceneView>(false, "", false);
    var camera = window ? window.camera : null;
    if (m_sceneCamera && m_sceneCamera != camera)
    {
      DisposeSceneCamera();
    }
    m_sceneCamera = camera;

    if (camera == null)
      return;

    var pp = (PostProcessingBase) camera.GetComponent(GetType());
    if (pp && pp != m_postProcessing)
    {
      DestroyImmediate(pp);
      pp = null;
    }

    if (pp == null)
    {
      pp = (PostProcessingBase) camera.gameObject.AddComponent(GetType());
      pp.m_attachedToSceneCamera = true;
    }

    EditorUtility.CopySerialized(this, pp);
    m_postProcessing = pp;

    m_appliedToSceneCamera = true;
  }
  #endif
}
