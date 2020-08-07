using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralMesh
{
    internal struct TriangleData
    {
        internal int v1;
        internal int v2;
        internal int v3;

        public IEnumerable<int> GetTriangles()
        {
            yield return v1;
            yield return v2;
            yield return v3;
        }
    }

    public class ProceduralSphere : MonoBehaviour
    {
        private MeshFilter _mesh;
        
        void Start()
        {
            _mesh = GetComponent<MeshFilter>();
        }

        // Update is called once per frame
        void Update()
        {
            MakeSphere(1f, 2, out Vector3[] vertex, out Vector3[] normals);
        }

        private void MakeSphere(float radius, int recursions, out Vector3[] vertex, out Vector3[] normals)
        {
            List<Vector3> vertexList = new List<Vector3>(3 ^ recursions);
            List<Vector3> normalList = new List<Vector3>(3 ^ recursions);

            vertex = vertexList.ToArray();
            normals = normalList.ToArray();
        }
    }
}

