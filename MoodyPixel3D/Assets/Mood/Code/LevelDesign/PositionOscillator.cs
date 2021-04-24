using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionOscillator : MonoBehaviour
{

    public Vector3 relativeEndPoint;
    public float velocity;

    private Vector3 localPos;
    private float t;
    // Start is called before the first frame update
    void Start()
    {
        localPos = transform.localPosition;
        t = Random.Range(0f, Mathf.PI * 2f);
    }

    // Update is called once per frame
    void Update()
    {
        t += Time.deltaTime * velocity * Mathf.PI * 2f;
        transform.localPosition = localPos + relativeEndPoint * (1f + Mathf.Sin(t)) * 0.5f;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.localPosition + relativeEndPoint, 0.25f);
    }
}
