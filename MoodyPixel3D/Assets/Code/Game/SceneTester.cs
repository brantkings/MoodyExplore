using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

public class SceneTester : MonoBehaviour
{
    public string spawnPointTag = "Spawn";
    public string playerTag = "Player";

    static bool doneIt;

#if UNITY_EDITOR
    [ContextMenu("Do it")]
    private IEnumerator Start()
    {
        if (doneIt) yield break;
        doneIt = true;
        Vector3 spawnPos = Vector3.zero;
        Quaternion spawnRot = Quaternion.identity;
        GameObject spawnPoint = GameObject.FindGameObjectWithTag(spawnPointTag);
        if(spawnPoint != null)
        {
            spawnPos = spawnPoint.transform.position;
            spawnRot = spawnPoint.transform.rotation;
        }
        spawnPoint = null;
        Debug.LogFormat("GotSpawnpoint {0} at {1}", spawnPoint, spawnPos);

        Scene currentScene = EditorSceneManager.GetActiveScene();

        Debug.LogFormat("Scenepath is {0}", currentScene.path);
        AssetPath scenePath = currentScene.path.Substring(0, currentScene.path.LastIndexOfAny(new char[]{'\\','/'}));

        string mainLevelName = currentScene.name.Substring(0, currentScene.name.IndexOfAny(new char[] { '-', '.', '_' }));

        foreach(Object o in scenePath.SearchAssetsUp((AssetPath.ContainsNameQuery) mainLevelName, new AssetPath.IsTypeQuery<SceneSet>()))
        {
            SceneSet set = o as SceneSet;
            if (Application.isPlaying)
            {
                set.LoadScenes();
            }
            else
            {
                set.LoadScenesEditor();
            }
            break;
        }
        
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if(player != null)
        {
            player.transform.position = spawnPos;
            player.transform.rotation = spawnRot;
        }
    }
#endif
}
