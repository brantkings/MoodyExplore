using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;



public class GlobalCameraBuffer : MonoBehaviour
{
    public bool shouldNotCopyToAllCameras = false;

    [System.Serializable]
    public class CameraBufferData
    {
        public string bufferName = "CustomBuffer";
        public RenderTextureDescriptor renderTextureDescriptor;
        public Shader lightChooseShader;
        public float sizeFactor = 1f;
    }

    public class BufferItselfBehaviour : MonoBehaviour
    {
        CameraBufferData data;

        Camera original;
        Camera drawer;

        public RenderTexture customBuffer;

        public int width;
        public int height;

        public bool HasName(string n)
        {
            return data.bufferName == n;
        }

        private void Awake()
        {
            original = GetComponent<Camera>();
            GameObject child = new GameObject(name + "_CameraBaby");
            child.transform.SetParent(original.transform, false);
            drawer = child.AddComponent<Camera>();
            drawer.CopyFrom(original);
            drawer.cullingMask -= LayerMask.GetMask("TransparentFX");
            drawer.enabled = false;

        }

        private void Start()
        {
            width = Mathf.FloorToInt(original.pixelWidth * data.sizeFactor);
            height = Mathf.FloorToInt(original.pixelHeight * data.sizeFactor);
            data.renderTextureDescriptor.width = width;
            data.renderTextureDescriptor.height = height;
            customBuffer = new RenderTexture(original.pixelWidth, original.pixelHeight, 16, RenderTextureFormat.ARGBFloat);
            customBuffer.name = name + "_" + data.bufferName + "_CameraBuffer";
            int instanceId = customBuffer.GetInstanceID();
            drawer.targetTexture = customBuffer;
        }

        private void OnDestroy()
        {
#if UNITY_EDITOR
            if (UnityEngine.Application.isPlaying) Destroy(customBuffer);
            else DestroyImmediate(customBuffer);
            return;
#endif
            Destroy(customBuffer);
        }

        public void SetCameraBufferData(CameraBufferData newData)
        {
            data = newData;
        }

        private void OnPreRender()
        {
            /*if(original.pixelWidth != customBuffer.width || original.pixelHeight != customBuffer.height)
            {
                customBuffer.width = original.pixelWidth;
                customBuffer.height = original.pixelHeight;
            }*/

            //drawer.targetTexture = customBuffer;
            drawer.RenderWithShader(data.lightChooseShader, null);
        }
    }

    public CameraBufferData dataToAdd;

    void Start()
    {
        if (!shouldNotCopyToAllCameras) CopyToAllCameras();
    }

    static public RenderTexture GetBuffer(Camera cam, string bufferName)
    {
        BufferItselfBehaviour b = cam?.GetComponent<BufferItselfBehaviour>();
        if (b != null && b.HasName(bufferName)) return b.customBuffer;
        else return null;
    }

    private void CopyToAllCameras()
    {
        foreach (var cam in AllCameras())
        {
            cam.gameObject.AddComponent<BufferItselfBehaviour>().SetCameraBufferData(dataToAdd);
        }

    }

    public static IEnumerable<Camera> AllCameras()
    {
        foreach (var cam in FindObjectsOfType<Camera>())
        {
            yield return cam;
        }

#if UNITY_EDITOR
        foreach (var cam in UnityEditor.SceneView.GetAllSceneCameras())
        {
            yield return cam;
        }
#endif
    }


}
