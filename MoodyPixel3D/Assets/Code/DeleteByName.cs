using UnityEngine;

namespace Code
{
    public class DeleteByName : MonoBehaviour
    {
        public GameObject toDelete;

        public string name;

        [ContextMenu("GetObject")]
        public void GetObject()
        {
            toDelete = GameObject.Find(name);
        }

        [ContextMenu("Delete object")]
        public void Delete()
        {
            DestroyImmediate(toDelete);
            toDelete = null;
        }
    }
}
