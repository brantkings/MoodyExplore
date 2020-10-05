using UnityEngine;

namespace Code
{
    public class DeleteByName : MonoBehaviour
    {
        public GameObject toDelete;

        public string searchName;

        [ContextMenu("GetObject")]
        public void GetObject()
        {
            toDelete = GameObject.Find(searchName);
        }

        [ContextMenu("Delete object")]
        public void Delete()
        {
            DestroyImmediate(toDelete);
            toDelete = null;
        }
    }
}
