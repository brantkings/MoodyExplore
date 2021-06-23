using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;
using DG.Tweening;

public class ThoughtSystemController : MonoBehaviour
{
    ThoughtFocusable[] _children;
    private ThoughtFocusable _currentFocusable;

    [SerializeField]
    private RectTransform focusPointPrefab;

    private List<RaycastResult> _resultsCache = new List<RaycastResult>(8);

    public RectTransform board;
    public RectTransform pointer;
    public RectTransform minRect;
    public RectTransform maxRect;

    private GraphicRaycaster raycaster;

    [Space]
    public float changeDuration = 0.2f;
    public Ease ease = Ease.InOutCirc;


    [Space]
    public float pointerAnchoredVelocity = 1f;

    [System.Serializable]
    private struct BoardData
    {
        float width;
        float height;
    }

    private void Awake()
    {
        _children = GetComponentsInChildren<ThoughtFocusable>(true);
        raycaster = GetComponentInChildren<GraphicRaycaster>();
    }

    #region Interface
    public void SetActivated(bool set)
    {
        foreach (var c in _children) c.SetFocus(set);

        TweenToRectTransform(set ? maxRect : minRect, changeDuration, ease);

        if(set)
        {
            CheckSelected();
        }
    }


    public void MovePointer(Vector2 movement, float timeDelta)
    {
        MovePointerRelativePosition(movement * timeDelta * pointerAnchoredVelocity);
    }

    public void ResetPointer()
    {
        SetPointerRelativePosition(Vector3.one * 0.5f);
    }

    public void SelectWithPointer()
    {

    }

    public void SetPain(int painNumber)
    {
        throw new System.NotImplementedException();
    }

    public void AddPain(int add)
    {
        SetPain(GetCurrentPain() + add);
    }

    public int GetCurrentPain()
    {
        throw new System.NotImplementedException();
    }
    #endregion

    private void SetPointerRelativePosition(Vector3 position)
    {
        Vector3 center = Vector2.one * 0.5f;
        Vector3 centralPosition = (position - center);
        if (centralPosition.magnitude > 0.5f) {
            position = center + centralPosition.normalized * 0.5f;
        }
        pointer.anchorMin = pointer.anchorMax = position;
        CheckSelected();
    }

    private Vector2 GetPointerRelativePosition()
    {
        return pointer.anchorMax;
    }

    private void MovePointerRelativePosition(Vector2 exactMovement)
    {
        SetPointerRelativePosition(GetPointerRelativePosition() + exactMovement);
        CheckSelected();
    }

    private PointerEventData GetEventDataForNow()
    {
        PointerEventData data = new PointerEventData(EventSystem.current);
        data.position = Camera.main.WorldToScreenPoint(pointer.position);
        Debug.LogFormat("Trying to get stuff at position {0}", data);
        return data;  
    }

    private void CheckSelected()
    {
        ThoughtFocusable newFocus = GetFocusablesUnderPointer().DefaultIfEmpty().Aggregate((x, y) => x.focusablePriority > y.focusablePriority ? x : y);
        Debug.LogFormat("New focus is {0}, old is {1}", newFocus, _currentFocusable);
        if(newFocus != _currentFocusable)
        {
            newFocus?.SetSelected(true);
            _currentFocusable?.SetSelected(false);
            _currentFocusable = newFocus;
        }
    }

    private IEnumerable<ThoughtFocusable> GetFocusablesUnderPointer()
    {
        _resultsCache.Clear();
        raycaster.Raycast(GetEventDataForNow(), _resultsCache);
        int i = 0;

        foreach (var result in _resultsCache)
        {
            Debug.LogFormat("{0} raycasted {1} which has {2} [{3}]", this, result.gameObject.name, result.gameObject.GetComponentInParent<ThoughtFocusable>(), i++);
            ThoughtFocusable focusable = result.gameObject.GetComponentInParent<ThoughtFocusable>();
            if (focusable != null) yield return focusable;
        }
    }

    private void TweenToRectTransform(RectTransform target, float duration, Ease ease)
    {
        board.DOPivot(target.pivot, duration).SetEase(ease).SetUpdate(true);
        board.DORotateQuaternion(target.rotation, duration).SetEase(ease).SetUpdate(true);
        board.DOScale(target.localScale, duration).SetEase(ease).SetUpdate(true);
        board.DOAnchorPos3D(target.anchoredPosition3D, duration).SetEase(ease).SetUpdate(true).OnComplete(()=> {
        });
        board.DOSizeDelta(target.sizeDelta, duration).SetEase(ease).SetUpdate(true);
        board.DOAnchorMax(target.anchorMax, duration).SetEase(ease).SetUpdate(true);
        board.DOAnchorMin(target.anchorMin, duration).SetEase(ease).SetUpdate(true);
    }
}
