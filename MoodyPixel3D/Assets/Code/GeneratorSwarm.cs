using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Castle3D
{
    public interface IGenerator
    {
        void Generate();
    }

    public class GeneratorSwarm : MonoBehaviour, IGenerator
    {
        TransformGetter parent;

        public Transform[] toGenerate;
        public Vector3 randomPositionGroup;
        public Vector3 randomPositionIndividual;

        [Space()]
        public Vector3 gizmosSizeOffset = Vector3.up * 2f;

        void Awake()
        {
            //toGenerate = ((IEnumerator<Transform>)parent.Get(transform)).ToArray();

            foreach (Transform child in parent.Get(transform)) child.gameObject.SetActive(false);
            
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.4f, 0.4f, 0f, 0.25f);
            foreach (Transform child in parent.Get(transform))
            {
                Gizmos.DrawCube(child.position, randomPositionGroup * 2f + randomPositionIndividual * 2f + gizmosSizeOffset);
            }
            Gizmos.color = new Color(0.8f, 0.8f, 0f, 0.5f);
            Gizmos.DrawCube(transform.position, randomPositionGroup * 2f + gizmosSizeOffset);
        }

        [ContextMenu("Generate!")]
        public void Generate()
        {
            Vector3 randomRange = transform.position + randomPositionGroup.RandomRange();
            foreach (Transform t in parent.Get(transform))
            {
                Transform clone = Instantiate(t, null, true);
                clone.position += randomRange + randomPositionIndividual.RandomRange();
                clone.rotation *= transform.rotation;
                clone.gameObject.SetActive(true);
            }
        }
    }
}


