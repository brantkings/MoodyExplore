using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MoodAttackFeedback : MonoBehaviour
{


    const int MAX_QUAD = 16;

    MoodPawn pawn;
    Mesh mesh;
    MeshFilter filter;
    MeshRenderer renderer;
    GameObject meshObj;

    List<Vector3> vertexData;
    List<int> triangleData;
    static Vector3[] _quadCache = new Vector3[2];

    private SlashTrailProperties[] properties;

    private Vector3 topPositionBefore;
    private Vector3 botPositionBefore;


    private void Awake()
    {
        properties = GetComponentsInChildren<SlashTrailProperties>();
        pawn = GetComponentInParent<MoodPawn>();
    }

    private void OnEnable()
    {
        pawn.OnBeforeSwinging += OnBeforeSwinging;
    }

    private void OnDisable()
    {
        pawn.OnBeforeSwinging -= OnBeforeSwinging;
    }

    private void Start()
    {
        meshObj = new GameObject(this.ToString());
        meshObj.transform.SetParent(pawn.transform);
        meshObj.SetActive(false);
        filter = meshObj.AddComponent<MeshFilter>();
        renderer = meshObj.AddComponent<MeshRenderer>();
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
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

    private void OnBeforeSwinging(MoodSwing swing, Vector3 direction)
    {
        SavePosition();
    }


    public void SavePosition()
    {
        SlashTrailProperties slash = GetBestProperties();
        topPositionBefore = slash.top.position;
        botPositionBefore = slash.bottom.position;
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


    public void DoFeedback(MoodSwing attack, Vector3 direction)
    {
        StartCoroutine(ShowFeedbackRoutine(attack, direction));
    }

    private IEnumerator ShowFeedbackRoutine(MoodSwing attack, Vector3 direction)
    {
        meshObj.SetActive(false);
        //yield return new WaitForSeconds(0.05f);
        CreateMesh(attack, direction);
        meshObj.SetActive(true);
        yield return new WaitForSeconds(0.2f);
        meshObj.SetActive(false);
    }

    private void ModifyHeight(ref Vector3 top, ref Vector3 bot, int index, int length)
    {
        top += Vector3.up * 2f;// Mathf.Lerp(iniYTop, endYTop, currentYLerp);
        bot += Vector3.up * 2f;// Mathf.Lerp(iniYBot, endYBot, currentYLerp);
        //currentYLerp += deltaLerp;
    }

    private void CreateMesh(MoodSwing attack, Vector3 direction)
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


        MoodSwing.MakeVertexTrailRightToLeft(attack, vertexData, triangleData, ModifyHeight);
        

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
        renderer.material = slash.GetMaterial();
    }

    

}
