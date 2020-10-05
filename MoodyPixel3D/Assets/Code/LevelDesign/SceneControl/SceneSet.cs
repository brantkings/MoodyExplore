using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[CreateAssetMenu(fileName = "SCENES_", menuName = "Long Hat House/Scenes/Scene Set")]
public class SceneSet : ScriptableObject
{
#if UNITY_EDITOR
    public SceneAsset mainScene;

    public SceneAsset[] scenes;

    [ContextMenu("Add all to build settings")]
    public void PutAllInBuildSettings()
    {
        EditorBuildSettings.scenes = AllBuildScenesScenes(true).ToArray();
    }

    private IEnumerable<Scene> AllScenes()
    {
        yield return GetScene(mainScene);
        foreach (var scene in scenes) yield return GetScene(scene);
    }

    private IEnumerable<EditorBuildSettingsScene> AllBuildScenesScenes(bool enabled)
    {
        yield return GetBuildScene(mainScene, enabled);
        foreach (var scene in scenes) yield return GetBuildScene(scene, enabled);
    }
    
    public void LoadScenes()
    {
        LoadScene(mainScene.name);
        foreach (var scene in scenes) LoadScene(scene.name, LoadSceneMode.Additive);
    }

    private void LoadScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
    {
        Debug.LogFormat("Loading {0} as {1}", sceneName, mode);
        SceneManager.LoadScene(sceneName, mode);
    }

    private Scene GetScene(SceneAsset asset)
    {
        return EditorSceneManager.GetSceneByPath(AssetDatabase.GetAssetPath(mainScene));
    }

    private EditorBuildSettingsScene GetBuildScene(SceneAsset scene, bool enabled)
    {
        return new EditorBuildSettingsScene(AssetDatabase.GetAssetPath(scene), enabled);
    }

#if UNITY_EDITOR
    [ContextMenu("Load all scenes")]
    public void LoadScenesEditor()
    {
        LoadSceneEditor(mainScene);
        foreach (var scene in scenes) LoadSceneEditor(scene, OpenSceneMode.Additive);
    }
    private void LoadSceneEditor(SceneAsset scene, OpenSceneMode mode = OpenSceneMode.Single)
    {
        EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(scene), mode);
    }
#endif
#endif
}
