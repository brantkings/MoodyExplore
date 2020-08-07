using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
using UnityEditor;
#endif
public class AssetPath
{

    string path;

    public AssetPath(string p)
    {
        int dotIndex = p.LastIndexOf('.');
        int slashIndex = p.LastIndexOf('/');
        if(slashIndex < dotIndex) //If this is a file, turn it into a folder
        {
            p = p.Substring(0, slashIndex);
        }

        if (p.Contains("Assets/"))
        {
            p = p.Substring(p.IndexOf("Assets/") + 7);
        }
        path = p;
    }

    public static implicit operator string(AssetPath p)
    {
        return p.path;
    }

    public static implicit operator AssetPath(string p)
    {
        return new AssetPath(p);
    }

    public override string ToString()
    {
        return string.Format("'{0}'", path);
    }

    public AssetPath GetParent()
    {
        int lastIndex = path.LastIndexOf("/");
        if (lastIndex != -1)
            return path.Substring(0, path.LastIndexOf("/"));

        else return null;
    }

    public abstract class Query
    {
        public abstract bool IsAssetCorrect(Object obj);
    }

    public class ContainsNameQuery : Query
    {
        public string contains;

        public ContainsNameQuery(string query)
        {
            contains = query;
        }

        public static implicit operator string(ContainsNameQuery p)
        {
            return p.contains;
        }

        public static implicit operator ContainsNameQuery(string p)
        {
            return new ContainsNameQuery(p);
        }

        public override bool IsAssetCorrect(Object obj)
        {
            return obj.name.Contains(contains);
        }
    }

    public class IsTypeQuery<T> : Query
    {
        public override bool IsAssetCorrect(Object obj)
        {
            return obj is T;
        }
    }


#if UNITY_EDITOR
    public IEnumerable<Object> GetAllAssets()
    {
        //Debug.LogFormat("Getting all objects at {0}", path);
        string[] fileEntries = Directory.GetFiles(Application.dataPath + "/" + path);
        foreach (string fileName in fileEntries)
        {
            string relativeFileName = fileName.Replace(Application.dataPath, "Assets/");
            Object t = AssetDatabase.LoadAssetAtPath(relativeFileName, typeof(Object));


            //Debug.LogFormat("Loading {0} and it is {1}", path, t);
            if (t != null)
                yield return t;
        }
    }

    public IEnumerable<AssetPath> GetAllChildrenFolders()
    {
        foreach (var p in AssetDatabase.GetSubFolders(path)) yield return p;
    }

    public IEnumerable<Object> SearchAssets(params Query[] query)
    {
        foreach (Object obj in GetAllAssets())
        {
            if(IsAssetCorrect(obj, query)) yield return obj;
        }
    }

    public IEnumerable<Object> SearchAssetsDown(params Query[] query)
    {
        foreach (var o in SearchAssets(query)) yield return o;
        foreach(AssetPath folder in GetAllChildrenFolders())
        {
            foreach (Object obj in folder.SearchAssetsDown(query)) yield return obj;
        }
    }

    public IEnumerable<Object> SearchAssetsUp(params Query[] query)
    {
        foreach (var o in SearchAssets(query)) yield return o;
        AssetPath parent = GetParent(); 
        if(parent != null)
        {
            foreach (Object obj in parent.SearchAssetsUp(query)) yield return obj;
        }
    }

    private bool IsAssetCorrect(Object obj, params Query[] query)
    {
        Debug.LogFormat("Trying to see if {0} is correct!", obj);

        for (int i = 0, len = query.Length; i < len; i++)
            if (!query[i].IsAssetCorrect(obj))
                return false;

        return true;
    }
    
#endif
}
