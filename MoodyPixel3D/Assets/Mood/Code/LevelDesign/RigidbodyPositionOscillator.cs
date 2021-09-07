using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidbodyPositionOscillator : AddonBehaviour<Rigidbody>
{

    public Vector3 relativeEndPoint;
    public float velocity;

    private Vector3 localPos;
    private float t;
    // Start is called before the first frame update
    void Start()
    {
        localPos = transform.position;
        t = Random.Range(0f, Mathf.PI * 2f);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        t += Time.deltaTime * velocity * Mathf.PI * 2f;
        Addon.MovePosition(localPos + relativeEndPoint * (1f + Mathf.Sin(t)) * 0.5f);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        if(localPos != Vector3.zero)
            Gizmos.DrawWireSphere(localPos + relativeEndPoint, 0.25f);
        else
            Gizmos.DrawWireSphere(transform.position + relativeEndPoint, 0.25f);

    }
}
