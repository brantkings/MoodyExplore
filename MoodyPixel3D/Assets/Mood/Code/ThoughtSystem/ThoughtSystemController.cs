using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;
using DG.Tweening;

public class ThoughtSystemController : MonoBehaviour, IFocusPointController
{
    IThoughtBoardObject[] _children;
    private ThoughtFocusable _currentFocusable;

    [SerializeField]
    private ThoughtFocusPoint focusPointPrefab;

    private List<RaycastResult> _resultsCache = new List<RaycastResult>(8);


    private class FocusPointState
    {
        public ThoughtFocusPoint point;
        public ThoughtFocusable focusable;
    }
    private List<FocusPointState> _focusState = new List<FocusPointState>(9);

    [Header("Objects")]
    public Transform thoughtObjectsParent;

    [Header("Canvas")]
    Camera canvasCamera;
    public RectTransform board;
    public RectTransform focusablesParent;
    public RectTransform focusPointsParent;
    public RectTransform pointer;
    public RectTransform minRect;
    public RectTransform maxRect;

    private GraphicRaycaster raycaster;

    [Space]
    public float changeDuration = 0.2f;
    public Ease ease = Ease.InOutCirc;
    public float focusDuration = 0.2f;
    public Ease easeFocus = Ease.InOutCirc;


    [Space]
    public float pointerAnchoredVelocity = 1f;

    [Space()]
    public Thought[] startTestThoughts;

    [System.Serializable]
    private struct BoardData
    {
        float width;
        float height;
    }

    private void Awake()
    {
        raycaster = GetComponentInChildren<GraphicRaycaster>();
        canvasCamera = board.GetComponentInParent<Canvas>()?.worldCamera;
        if (canvasCamera == null) canvasCamera = Camera.main;
    }

    private void Start()
    {
        int i = 0, len = startTestThoughts.Length;
        foreach(var t in startTestThoughts)
        {
            CreateThought(t, i++, len);
        }

        StartFocusPoints();

        _children = GetComponentsInChildren<IThoughtBoardObject>(true);
    }

    #region Thought Creation

    protected ThoughtFocusable CreateThought(Thought t, int index, int length)
    {
        ThoughtFocusable focusable = Instantiate(t.GetThoughtFocusablePrefab(), focusablesParent).Create(t);
        RectTransform rect = focusable.GetComponent<RectTransform>();
        rect.anchorMax = rect.anchorMin = GetCircularPosition(index, length, -90f) * 0.35f + Vector3.one * 0.5f;
        return focusable;
    }

    protected ThoughtFocusPoint CreateFocusPoint()
    {
        ThoughtFocusPoint point = Instantiate(focusPointPrefab, focusPointsParent);
        _focusState.Add(new FocusPointState()
        {
            point = point,
            focusable = null
        });
        return point;
    }

    protected Vector3 GetCircularPosition(int index, int length, float offsetangle, float minAngle = 35f, float maxAngle = 180f)
    {
        float angle = Mathf.Max(maxAngle / length, minAngle);
        float angleTotal = angle * length;
        float angleMe = offsetangle + index * angle - angleTotal * 0.5f;
        Debug.LogFormat("[THOUGHT] {0}/{1}: Angle is {2}, total is {3}, my angle is {4}", index, length, angle, angleTotal, angleMe);
        Vector3 pos = new Vector3(Mathf.Cos(Mathf.Deg2Rad * angleMe), -Mathf.Sin(Mathf.Deg2Rad * angleMe));
        return pos;
    }

    #endregion

    #region Interface

    private bool? _activated;

    [SerializeField]
    int _maxFocusPoints;
    int _availableFocusPoints;
    int _selectedFocusableIndex;

    int _currentPain;

    public int MaxPoints => _maxFocusPoints;

    public int AvailablePoints
    {
        get
        {
            return GetAmountOfFreeFocusPoints();
        }
    }

    public int CurrentPain { get => _currentPain; }

    public int MaxMinusPainPoints { get => _maxFocusPoints - _currentPain; }

    private void StartFocusPoints()
    {
        for (int j = 0; j < _maxFocusPoints; j++)
        {
            CreateFocusPoint();
        }
    }

    public bool IsActivated()
    {
        return _activated.HasValue? _activated.Value : false;
    }

    public void SetActivated(bool set)
    {
        if (set == _activated) return;
        foreach (var c in _children) c.SetMaximize(set);

        TweenToRectTransform(set ? maxRect : minRect, changeDuration, ease);

        if(set)
        {
            CheckSelected();
        }

        _activated = set;
    }


    public void MovePointer(Vector2 movement, float timeDelta)
    {
        MovePointerRelativePosition(movement * timeDelta * pointerAnchoredVelocity);
    }

    public void ResetPointer()
    {
        SetPointerRelativePosition(Vector3.one * 0.5f);
    }

    public void SelectWithPointer(MoodPawn pawn)
    {
        ThoughtFocusable focusable = GetSelectedFocusable();
        if(focusable != null)
        {
            if (focusable.CanSetFocus(pawn))
            {
                if(focusable.IsFocused()) //if is focused then remove
                {
                    focusable.SetFocused(false, pawn);
                    DeassignFocusPoint(focusable, pawn);
                    RemoveThought(pawn, focusable.GetThought());
                }
                else if (HasFreeFocusPoint())
                {
                    focusable.SetFocused(true, pawn);
                    AssignFocusPoint(focusable, pawn);
                    StartThought(pawn, focusable.GetThought());
                }
            }
        }
    }

    public void SetPain(int painNumber)
    {
        _currentPain = painNumber;
    }

    public void AddPain(int add)
    {
        SetPain(CurrentPain + add);
    }

    #endregion

    #region Focus Point

    private Vector3 GetAnchorRelativePosition(RectTransform childToMove, RectTransform placeToMoveTo, RectTransform parent)
    {
        float width = parent.rect.width;
        float height = parent.rect.height;
        Vector2 rect = new Vector2(width, height);
        Vector2 pos = (Vector2)placeToMoveTo.position - (parent.pivot * rect) - (Vector2)(childToMove.position - parent.position);
        return pos / rect;

    }

    private bool HasFreeFocusPoint()
    {
        return GetNextFreeFocusPoint() != null;
    }

    private bool AssignFocusPoint(ThoughtFocusable f, MoodPawn p)
    {
        FocusPointState next = GetNextFreeFocusPoint();
        if (next != null)
        {
            next.focusable = f;
            RectTransform toMove = next.point.GetObjectToMove();
            toMove.DOLocalMove(f.transform.localPosition - toMove.parent.localPosition, focusDuration).SetEase(easeFocus).SetUpdate(true);
            return true;
        }
        else return false;
    }

    private bool DeassignFocusPoint(ThoughtFocusable f, MoodPawn p)
    {
        FocusPointState next = GetFocusPointFrom(f);
        if (next != null)
        {
            next.focusable = null;
            next.point.GetObjectToMove().DOLocalMove(Vector3.zero, focusDuration).SetEase(easeFocus).SetUpdate(true);
            return true;
        }
        else return false;
    }

    private int GetAmountOfFreeFocusPoints()
    {
        return _focusState.Count((x)=> x.focusable == null);
    }

    private FocusPointState GetNextFreeFocusPoint()
    {
        return _focusState.FirstOrDefault((x) => x.focusable == null);
    }

    private FocusPointState GetFocusPointFrom(ThoughtFocusable f)
    { 
        return _focusState.FirstOrDefault((x) => x.focusable == f);
    }
    #endregion

    #region Thoughts and effects

    HashSet<Thought> _activeThoughts = new HashSet<Thought>();
    
    protected virtual void StartThought(MoodPawn p, Thought t)
    {
        _activeThoughts.Add(t);
        StartCoroutine(EffectRoutine(p, t));
    }

    protected virtual void RemoveThought(MoodPawn p, Thought t)
    {
        _activeThoughts.Remove(t);
        StartCoroutine(RemoveEffectRoutine(p, t));
    }

    private class WaitForExperienceChange : CustomYieldInstruction
    {
        public WaitForExperienceChange(ThoughtSystemController s)
        {

        }

        public override bool keepWaiting
        {
            get
            {
                return false;
            }
        }
    }

    private IEnumerator EffectRoutine(MoodPawn p, Thought t)
    {
        int experience = 0;
        yield return StartCoroutine(t.FocusEffect(p, this));
        while (!t.CheckExperienceCondition(p, this, experience) && _activeThoughts.Contains(t))
        {
            yield return null;
        }
        yield return StartCoroutine(t.ExperienceCompleteEffect(p, this));
    }

    private IEnumerator RemoveEffectRoutine(MoodPawn p, Thought t)
    {
        yield return StartCoroutine(t.RemoveFocusEffect(p, this));
    }

    public void AddEXP(int amount)
    {

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

    private PointerEventData GetEventDataForNow(Vector3 pointerPosition)
    {
        PointerEventData data = new PointerEventData(EventSystem.current);
        data.position = canvasCamera.WorldToScreenPoint(pointerPosition);
        //Debug.LogFormat("Mouse: {0} while Pointer position is {1} -> {2}", Input.mousePosition, pointerPosition, data.position);
        LHH.Utils.DebugUtils.DrawNormalStar(pointer.position, 0.1f, Quaternion.identity, Color.yellow, 0.2f);
        return data;  
    }

    private ThoughtFocusable GetSelectedFocusable()
    {
        return _currentFocusable;
    }

    private void CheckSelected()
    {
        Vector3 pointerPosition = pointer.position;
        ThoughtFocusable newFocus = GetFocusablesUnderPointer(pointerPosition).DefaultIfEmpty().Aggregate((x, y) =>
        {
            if (x.focusablePriority > y.focusablePriority) return x;
            else if (x.focusablePriority < y.focusablePriority) return y;
            else
            {
                float distX = (pointerPosition - x.transform.position).sqrMagnitude, distY = (pointerPosition - y.transform.position).sqrMagnitude;
                if (distX <= distY) return x;
                else return y;
            }
        });
        //Debug.LogFormat("New focus is {0}, old is {1}", newFocus, _currentFocusable);
        if(newFocus != _currentFocusable)
        {
            newFocus?.SetHovered(true);
            _currentFocusable?.SetHovered(false);
            _currentFocusable = newFocus;
        }
    }

    private IEnumerable<ThoughtFocusable> GetFocusablesUnderPointer(Vector3 pointerPosition)
    {
        _resultsCache.Clear();
        raycaster.Raycast(GetEventDataForNow(pointerPosition), _resultsCache);
        //int i = 0;

        foreach (var result in _resultsCache)
        {
            //Debug.LogFormat("[THOUGHT] {0} raycasted {1} which has {2} [{3}]", this, result.gameObject.name, result.gameObject.GetComponentInParent<ThoughtFocusable>(), i++);
            ThoughtFocusable focusable = result.gameObject.GetComponentInParent<ThoughtFocusable>();
            if (focusable != null) yield return focusable;
        }
        //if(i==0) Debug.LogFormat("[THOUGHT] {0} raycasted nothing", this);
    }

    private void TweenToRectTransform(RectTransform target, float duration, Ease ease)
    {
        board.DOPivot(target.pivot, duration).SetUpdate(true).SetEase(ease);
        board.DOLocalRotateQuaternion(target.localRotation, duration).SetUpdate(true).SetEase(ease);
        board.DOScale(target.localScale, duration).SetUpdate(true).SetEase(ease);
        board.DOAnchorPos3D(target.anchoredPosition3D, duration).SetUpdate(true).SetEase(ease).OnComplete(()=> {
        });
        board.DOSizeDelta(target.sizeDelta, duration).SetUpdate(true).SetEase(ease);
        board.DOAnchorMax(target.anchorMax, duration).SetUpdate(true).SetEase(ease);
        board.DOAnchorMin(target.anchorMin, duration).SetUpdate(true).SetEase(ease);
    }
}
