using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MoodAttackFeedback : MonoBehaviour
{


    const int MAX_QUAD = 16;

    public float attackDuration = 0.16f;

    MoodPawn pawn;
    Mesh mesh;
    MeshFilter filter;
    MeshRenderer meshRend;
    GameObject meshObj;

    List<Vector3> vertexData;
    List<int> triangleData;
    static Vector3[] _quadCache = new Vector3[2];

    private SlashTrailProperties[] properties;

    private Vector3 topPositionBefore;
    private Vector3 botPositionBefore;

    private float proportion = 0f;


    private void Awake()
    {
        GetProperties();
        pawn = GetComponentInParent<MoodPawn>();
    }

    private void GetProperties()
    {
        properties = GetComponentsInChildren<SlashTrailProperties>();

        if(properties == null || properties.Length <= 0)
        {
            Debug.LogErrorFormat(this, "{0} has no slashTrailProperties!", this);
        }
    }

    private void OnEnable()
    {
        pawn.OnBeforeSwinging += OnBeforeSwinging;
        if(pawn.Inventory != null) pawn.Inventory.OnInventoryChange += OnInventoryChange;
    }

    private void OnDisable()
    {
        pawn.OnBeforeSwinging -= OnBeforeSwinging;
        if (pawn.Inventory != null) pawn.Inventory.OnInventoryChange -= OnInventoryChange;
    }

    private void Start()
    {
        meshObj = new GameObject(this.ToString());
        meshObj.transform.SetParent(pawn.transform);
        meshObj.SetActive(false);
        filter = meshObj.AddComponent<MeshFilter>();
        meshRend = meshObj.AddComponent<MeshRenderer>();
        meshRend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mesh = new Mesh();
        mesh.name = "Created_SwingMesh";
        filter.mesh = mesh;

        vertexData = new List<Vector3>(MAX_QUAD * 4);
        triangleData = new List<int>(MAX_QUAD * 6);
        mesh.SetVertices(vertexData);
        mesh.SetTriangles(triangleData, 0);
    }

    private SlashTrailProperties GetBestProperties()
    {
        if (properties != null && properties.Length > 0)
            return properties.Aggregate((x, y) => x.priority > y.priority ? x : y);
        else return null;
    }

    private void OnBeforeSwinging(MoodSwing.MoodSwingBuildData swing, Vector3 direction)
    {
        SavePosition();
    }

    private void OnInventoryChange(MoodInventory inventory)
    {
        GetProperties();
    }

    public void SavePosition()
    {
        SlashTrailProperties slash = GetBestProperties();
        if(slash != null)
        {
            topPositionBefore = slash.top.position;
            botPositionBefore = slash.bottom.position;
        }
    }

    private void OnDrawGizmos()
    {
        SlashTrailProperties slash = GetBestProperties();
        if(slash != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(slash.top.position, 0.125f);
            Gizmos.DrawSphere(topPositionBefore, 0.125f);
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(slash.bottom.position, 0.125f);
            Gizmos.DrawSphere(botPositionBefore, 0.125f);
        }
    }


    public void DoFeedback(MoodSwing.MoodSwingBuildData attack, Vector3 direction)
    {
        StartCoroutine(ShowFeedbackRoutine(attack, direction));
    }

    private IEnumerator ShowFeedbackRoutine(MoodSwing.MoodSwingBuildData attack, Vector3 direction)
    {
        meshObj.SetActive(false);
        //yield return new WaitForSeconds(0.05f);
        CreateMesh(attack, direction);
        meshObj.SetActive(true);
        yield return new WaitForSeconds(attackDuration);
        meshObj.SetActive(false);
    }

    private void ModifyHeight(ref Vector3 top, ref Vector3 bot, int index, int length)
    {
        top += Vector3.up * 2f;// Mathf.Lerp(iniYTop, endYTop, currentYLerp);
        bot += Vector3.up * 2f;// Mathf.Lerp(iniYBot, endYBot, currentYLerp);
        //currentYLerp += deltaLerp;
    }

    private void CreateMesh(MoodSwing.MoodSwingBuildData attack, Vector3 direction)
    {
        vertexData.Clear();
        triangleData.Clear();


        SlashTrailProperties slash = GetBestProperties();
        Quaternion directionRotation = Quaternion.LookRotation(direction, pawn.Up);
        Quaternion invDirectionRot = Quaternion.Inverse(directionRotation);
        Vector3 topPositionRelativeNow = invDirectionRot * (slash.top.position - pawn.Position);
        Vector3 botPositionRelativeNow = invDirectionRot * (slash.bottom.position - pawn.Position);
        Vector3 topPositionRelativeBefore = invDirectionRot * (topPositionBefore - pawn.Position);
        Vector3 botPositionRelativeBefore = invDirectionRot * (botPositionBefore - pawn.Position);
        /*
         * Use this block when animating the mesh then
         * float iniYTop = topPositionRelativeBefore.y;
        float endYTop = topPositionRelativeNow.y;
        float iniYBot = botPositionRelativeBefore.y;
        float endYBot = botPositionRelativeNow.y;
        float deltaLerp = 1f / (float)length;*/

        //_quadCache[0] = topPositionRelativeBefore;
        //_quadCache[1] = botPositionRelativeBefore;

        MoodSwing.DelUpdateVectors updateFunc = default(MoodSwing.DelUpdateVectors);
        updateFunc += ModifyHeight;

        MoodSwing.MakeVertexTrailRightToLeft(attack, vertexData, triangleData, updateFunc);
        

        /*_quadCache[2] = _quadCache[0];
        _quadCache[3] = _quadCache[1];
        _quadCache[0] = topPositionRelativeNow;
        _quadCache[1] = botPositionRelativeNow;
        MakeVertexFromQuad(ref index, _quadCache, vertexData, triangleData);*/

        mesh.Clear();
        mesh.SetVertices(vertexData);
        mesh.SetTriangles(triangleData, 0);
        mesh.RecalculateBounds();
        meshObj.transform.position = pawn.Position;
        meshObj.transform.rotation = directionRotation;
        meshRend.material = slash.GetMaterial();
    }

    private void Update()
    {
        if(vertexData.Count > 0)
        {
            for (int i = 0, len = vertexData.Count; i < len; i++)
            {
                if (i % 2 == 1)
                {
                    vertexData[i] = Vector3.Lerp(vertexData[i], vertexData[i - 1], 0.25f);
                }
                else continue;
            }
            mesh.SetVertices(vertexData);
        }
    }



}
