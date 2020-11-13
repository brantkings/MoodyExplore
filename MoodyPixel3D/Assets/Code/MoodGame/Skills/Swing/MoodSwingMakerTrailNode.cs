using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoodSwingMakerTrailNode : MonoBehaviour
{
    public IEnumerator<Transform> TrailPoints
    {
        get
        {
            VerifyTwoNodes();
            yield return transform.GetChild(0);
            yield return transform.GetChild(1);
        }
    }

    private void VerifyTwoNodes()
    {
        while (transform.childCount < 2)
        {
            GameObject tp = new GameObject("TrailPart");
            tp.transform.SetParent(transform);
        }
    }

    public Transform TopNode
    {
        get
        {
            VerifyTwoNodes();
            return transform.GetChild(0);
        }
    }

    public Transform BottomNode
    {
        get
        {
            VerifyTwoNodes();
            return transform.GetChild(1);
        }
    }
}
