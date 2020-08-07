using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Castle3D
{
    public class GeneratorPrefabs : MonoBehaviour, IGenerator
    {
        public GameObject[] prefabs;
        public Area area;

        public void Generate()
        {
            Instantiate(GetRandomPrefab(), area.GetRandomPosition(), transform.rotation);     
        }

        private GameObject GetRandomPrefab()
        {
            return prefabs[Random.Range(0, prefabs.Length)];
        }
    }
}
