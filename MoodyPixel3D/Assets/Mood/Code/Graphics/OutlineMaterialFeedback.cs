using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;

public class OutlineMaterialFeedback : MonoBehaviour
{
    public GameObject outlineParent;
    public List<Material> materials;
    public List<Color> originalColorsCache;
    public string colorProperty = "_BorderColor";
    private int _propHash;

    [System.Serializable]
    public struct Data
    {
        [System.Serializable]
        public struct Animation
        {
            public float duration;
            public float beginValue;
            public float endValue;
            public Ease ease;
        }

        [Header("Alpha 0 means old color")]
        public Gradient outlineColor;
        public Animation inAnim;
        public Animation outAnim;
        
    }

    private void Awake()
    {
        materials = GetMaterials().ToList();
        originalColorsCache = new List<Color>(materials.Count);
        _propHash = Shader.PropertyToID(colorProperty);
    }

    private IEnumerable<Material> GetMaterials()
    {
        foreach(var rend in GetComponentsInChildren<Renderer>())
        {
            foreach(Material mat in rend.materials)
            {
                if (IsPossibleMaterial(mat)) yield return mat;
            }
        }
    }

    private bool IsPossibleMaterial(Material mat)
    {
        return mat.HasProperty(colorProperty);
    }

    public void DoFeedback(Data data)
    {
        StopAllCoroutines();
        DOTween.Complete(this);
        StartCoroutine(AnimRoutine(data));
    }

    private IEnumerator AnimRoutine(Data data)
    {
        originalColorsCache.Clear();
        for (int i = 0, len = materials.Count; i < len; i++)
        {
            originalColorsCache.Add(materials[i].GetColor(_propHash));
        }
        yield return DoOutline(materials, originalColorsCache, _propHash, data, data.inAnim);
        yield return DoOutline(materials, originalColorsCache, _propHash, data, data.outAnim);
    }

    private float _tweenX = 0;

    private Tween DoOutline(IList<Material> materials, IList<Color> originalColors, int colorHash, Data data, Data.Animation anim)
    {
        Gradient grad = data.outlineColor;
        //Debug.LogFormat("[OUTLINE] {0} tweening for {1} with duration {2}", this, anim.endValue, anim.duration);
        if (anim.duration > 0f)
        {
            SetColors(materials, originalColors, colorHash, grad, anim.beginValue);
            return DOTween.To(() =>
            {
                return _tweenX;
            }, (x) =>
            {
                SetColors(materials, originalColors, colorHash, grad, x);
            }, anim.endValue, anim.duration).SetId(this).SetEase(anim.ease);
        }
        else
        {
            SetColors(materials, originalColors, colorHash, grad, anim.endValue);
            return null;
        }

    }

    private void SetColors(IList<Material> materials, IList<Color> originalColors, int propHash, Gradient grad, float x)
    {
        _tweenX = x;
        for (int i = 0, len = materials.Count; i < len; i++)
        {
            materials[i].SetColor(propHash, CalculateColor(originalColors[i], grad, x));
        }
    }

    private Color CalculateColor(Color main, Gradient grad, float gradX)
    {
        Color gradColor = grad.Evaluate(gradX);
        return Color.Lerp(main, gradColor, gradColor.a);
    }


}
