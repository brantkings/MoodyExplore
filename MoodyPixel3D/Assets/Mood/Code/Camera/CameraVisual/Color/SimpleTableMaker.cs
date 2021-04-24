using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ColorTableMaker<P> : ScriptableObject
{
    [Range(0, 1024)]
    public int size = 32;
    public TextureFormat format = TextureFormat.RGBA32;
    public TextureWrapMode wrapMode = TextureWrapMode.Clamp;
    public FilterMode filterMode = FilterMode.Point;
    public bool mipMaps;
    public Palette<P> paletteToUse;
    public PaletteComparerMethod<P> paletteComparer;
    public Texture3D textureToCreate;

#if UNITY_EDITOR
    [ContextMenu("CreateTexture")]
    public void CreateTexture()
    {
        if(textureToCreate != null)
        {
            if(textureToCreate.width != size || textureToCreate.height != size || textureToCreate.depth != size)
            {
                Debug.LogWarningFormat("Changed size through creation needs deleting the asset and re-applying it to everyone using it.");
            }
        }

        if(textureToCreate == null)
        {
            // Create the texture and apply the configuration
            Texture3D texture = new Texture3D(size, size, size, format, mipMaps);
            texture.wrapMode = wrapMode;
            texture.filterMode = filterMode;

            SetColors(out Color[] colors);

            // Copy the color values to the texture
            texture.SetPixels(colors);

            // Apply the changes to the texture and upload the updated texture to the GPU
            texture.Apply();

            // Save the texture to your Unity Project
            string path = UnityEditor.AssetDatabase.GetAssetPath(this);
            path = path.Substring(0, path.LastIndexOf("/"));
            UnityEditor.AssetDatabase.CreateAsset(texture, path + "/" + name + "_3DColor.asset");

            textureToCreate = texture;
        }
        else
        {
            SetColors(out Color[] colors);

            // Copy the color values to the texture
            textureToCreate.SetPixels(colors);

            // Apply the changes to the texture and upload the updated texture to the GPU
            textureToCreate.Apply();
        }
        
    }
#endif

    private void SetColors(out Color[] colors)
    {
        // Create a 3-dimensional array to store color data
        colors = new Color[size * size * size];

        // Populate the array so that the x, y, and z values of the texture will map to red, blue, and green colors
        float inverseResolution = 1.0f / (size - 1.0f);
        float semiResolution = inverseResolution / 2f;
        for (int z = 0; z < size; z++)
        {
            int zOffset = z * size * size;
            for (int y = 0; y < size; y++)
            {
                int yOffset = y * size; 
                for (int x = 0; x < size; x++)
                {
                    Color toCompare = new Color(x * inverseResolution, y * inverseResolution, z * inverseResolution, 1.0f);
                    Color newColor;

                    if (paletteComparer != null && paletteToUse != null)
                    {

                        //newColor = paletteComparer.GetBestColor(toCompare, paletteToUse);
                        newColor = GetBestColor(paletteComparer, paletteToUse, AllAroundColors(toCompare, inverseResolution));
                    }
                    else newColor = toCompare;

                    Debug.LogFormat("{2} -> Best color to {0} is {1}", GetColorDebugString(toCompare), GetColorDebugString(newColor), name);
                    colors[x + yOffset + zOffset] = newColor;
                }
            }
        }
    }

    [Range(-1f,1f)]
    public float[] possibilities = new float[] { -0.5f, 0f, 0.5f };

    private IEnumerable<Vector3> AllAroundPlaces()
    {
        foreach(float r in possibilities)
        {
            foreach(float g in possibilities)
            {
                foreach (float b in possibilities)
                {
                    yield return new Vector3(r, g, b);
                }
            }
        }
    }

    private IEnumerable<Color> AllAroundColors(Color ground, float inverseResolution)
    {
        yield return ground;
        //This wasnt working at all, creating all sorts of artifacts, lets leave it out
        /*foreach(var place in AllAroundPlaces())
        {
            yield return ground + new Color(place.x, place.y, place.z) * inverseResolution;
        }*/
    }

    private Color GetBestColor<T>(PaletteComparerMethod<T> paletteComparer, IColorPalette<T> palette, IEnumerable<Color> colors)
    {
        Dictionary<Color, int> raffle = new Dictionary<Color, int>(Mathf.FloorToInt(Mathf.Pow(possibilities.Length, 3)));
        foreach(Color c in colors)
        {
            Color result = paletteComparer.GetBestColor(c, palette);
            if(raffle.ContainsKey(result))
            {
                raffle[result]++;
            }
            else
            {
                raffle.Add(result, 1);
            }
        }
        return raffle.OrderBy(x => x.Value).First().Key;
    }

    private string GetColorDebugString(Color c)
    {
        return string.Format("<color=#{0}>{1}</color>", c.ToHexStringRGB(), c);
    }
}



[CreateAssetMenu(menuName = "Long Hat House/Colors/Color Table Maker", fileName = "COLOR_Table_")]
public class SimpleTableMaker : ColorTableMaker<Color>
{

#if UNITY_EDITOR
    [ContextMenu("CreateTexture")]
    public void TestCreate()
    {
        CreateTexture();
    }
#endif
}
