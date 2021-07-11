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


    protected class FocusPointState
    {
        public ThoughtFocusPoint point;
        public ThoughtFocusable focusable;
    }
    private List<FocusPointState> _focusState = new List<FocusPointState>(9);

    [Header("Objects")]
    private MoodExperiencer experiencer;
    public OutlineMaterialFeedback outlineFeedback;
    public Transform thoughtObjectsParent;
    public UnityEngine.UI.Text descriptor;

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
    private List<ThoughtFocusable> upThoughts;
    private List<ThoughtFocusable> downThoughts;

    public enum ThoughtPlacement
    {
        Up,
        Down
    }
    [System.Serializable]
    private struct BoardData
    {
        float width;
        float height;
    }

    private void Awake()
    {
        experiencer = GetComponentInParent<MoodPawn>()?.GetComponentInChildren<MoodExperiencer>();
        raycaster = GetComponentInChildren<GraphicRaycaster>();
        canvasCamera = board.GetComponentInParent<Canvas>()?.worldCamera;
        if (canvasCamera == null) canvasCamera = Camera.main;
    }

    private void OnEnable()
    {
        if (experiencer != null)
        {
            experiencer.OnExperienceChange += OnExperienceChange;
        }
    }

    private void OnDisable()
    {
        if (experiencer != null)
        {
            experiencer.OnExperienceChange -= OnExperienceChange;
        }
    }

    private void Start()
    {
        int i = 0, len = startTestThoughts.Length;
        foreach(var t in startTestThoughts)
        {
            GetList(ThoughtPlacement.Down).Add(CreateThought(t));
        }

        PositionThoughtList(GetList(ThoughtPlacement.Down), GetThoughtListAngle(ThoughtPlacement.Down));

        StartFocusPoints();

        RecaptureBoardObjects();
    }

    private void RecaptureBoardObjects()
    {
        _children = GetComponentsInChildren<IThoughtBoardObject>(true);
    }

    #region Thought Creation

    public void AddThought(Thought t, MoodPawn p, OutlineMaterialFeedback.Data? outlineFeedbackData = null, ThoughtPlacement where = ThoughtPlacement.Up)
    {
        GetList(where).Add(CreateThought(t));
        PositionThoughtList(GetList(where), GetThoughtListAngle(where));
        t.AddThoughtInMind(p, this);
        if(outlineFeedbackData.HasValue) outlineFeedback?.DoFeedback(outlineFeedbackData.Value);
        RecaptureBoardObjects();
    }

    public void RemoveThought(Thought t, MoodPawn p, OutlineMaterialFeedback.Data? outlineFeedbackData = null, ThoughtPlacement where = ThoughtPlacement.Up)
    {
        List<ThoughtFocusable> list = GetList(where);
        ThoughtFocusable toDestroy = list.Find((x) => x.GetThought() == t);
        if (outlineFeedbackData.HasValue) outlineFeedback?.DoFeedback(outlineFeedbackData.Value);
        RemoveThought(t, p, toDestroy, list, where);
    }

    private void FindRemoveThought(Thought t, MoodPawn p, ThoughtPlacement toStart)
    {
        ThoughtPlacement where = toStart;
        int enumLength = System.Enum.GetValues(typeof(ThoughtPlacement)).Length;
        int amountToGo = enumLength;
        while (amountToGo-- > 0)
        {
            List<ThoughtFocusable> list = GetList(where);
            if(list != null)
            {
                ThoughtFocusable toDestroy = list.Find((x) => x.GetThought() == t);
                if(toDestroy != null)
                {
                    RemoveThought(t, p, toDestroy, list, where);
                    return;
                }
            }
            where = (ThoughtPlacement)(((int)where + 1) % enumLength);
        }
    }

    private void RemoveThought(Thought t, MoodPawn p, ThoughtFocusable toDestroy, List<ThoughtFocusable> list, ThoughtPlacement where)
    {
        list.Remove(toDestroy);
        DeassignFocusPoint(toDestroy, p);
        Destroy(toDestroy.gameObject);
        PositionThoughtList(list, GetThoughtListAngle(where));
        t.RemoveThoughtFromMind(p, this);
        RecaptureBoardObjects();
    }

    protected void PositionThought(in RectTransform thoughtPosition, int index, int length, float angleOffset, float radiusOffset = 0.7f, float desiredAngle = 60f, float minAngle = 15f, float totalAngle = 180f)
    {
        thoughtPosition.anchorMax = thoughtPosition.anchorMin = GetCircularPosition(index, length, angleOffset, desiredAngle, minAngle, totalAngle) * radiusOffset * 0.5f + Vector3.one * 0.5f;
    }

    protected void PositionThoughtList(in List<ThoughtFocusable> list, float angleOffset, float radiusOffset = 0.7f, float desiredAngle = 60f, float minAngle = 15f, float totalAngle = 180f)
    {
        for(int i = 0, len = list.Count;i<len;i++)
        {
            PositionThought(list[i].GetComponent<RectTransform>(), i, len, angleOffset, radiusOffset, desiredAngle, minAngle, totalAngle);
        }
    }

    protected List<ThoughtFocusable> CheckList(ref List<ThoughtFocusable> list)
    {
        if (list == null) list = new List<ThoughtFocusable>(8);
        return list;
    }

    protected List<ThoughtFocusable> GetList(ThoughtPlacement placement)
    {
        switch (placement)
        {
            case ThoughtPlacement.Up:
                return CheckList(ref upThoughts);
            case ThoughtPlacement.Down:
                return CheckList(ref downThoughts);
            default:
                return CheckList(ref upThoughts);
        }
    }

    protected float GetThoughtListAngle(ThoughtPlacement placement)
    {
        switch (placement)
        {
            case ThoughtPlacement.Up:
                return -90f;
            case ThoughtPlacement.Down:
                return 90f;
            default:
                return -90f;
        }
    }

    protected ThoughtFocusable CreateThought(Thought t)
    {
        ThoughtFocusable focusable = Instantiate(t.GetThoughtFocusablePrefab(), focusablesParent).Create(t);
        RectTransform rect = focusable.GetComponent<RectTransform>();
        
        return focusable;
    }

    protected Vector3 GetCircularPosition(int index, int length, float offsetangle, float desiredAngle, float minAngle, float desiredTotalAngle)
    {
        int numSpaces = length - 1;
        float angle = numSpaces == 0 ? 0f : Mathf.Clamp(desiredAngle, minAngle, desiredTotalAngle / numSpaces);
        float angleTotal = angle * numSpaces;
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

    public int MaxPoints => GetAmountOfFocusPoints();

    public int AvailablePoints
    {
        get
        {
            return GetAmountOfFreeFocusPoints();
        }
    }

    public int CurrentPain { get => _currentPain; }

    public int MaxMinusPainPoints { get => _maxFocusPoints - _currentPain; }

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
            CheckSelected(true);
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
                    StopThought(pawn, focusable.GetThought(), DeassignFocusPoint(focusable, pawn));
                }
                else if (HasFreeFocusPoint()) //If not then assign
                {
                    focusable.SetFocused(true, pawn);
                    StartThought(pawn, focusable.GetThought(), AssignFocusPoint(focusable, pawn));
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

    public ThoughtFocusPoint CreateFocusPoint()
    {
        ThoughtFocusPoint point = Instantiate(focusPointPrefab, focusPointsParent);
        point.UnsetExperienceText();
        _focusState.Add(new FocusPointState()
        {
            point = point,
            focusable = null
        });
        return point;
    }

    public bool RemoveFocusPoint()
    {
        if(_focusState.Count > 0)
        {
            FocusPointState state = _focusState.Last();
            Destroy(state.point);
            _focusState.Remove(state);
            return true;
        }
        else return false;
    }


    private void StartFocusPoints()
    {
        for (int j = 0; j < _maxFocusPoints; j++)
        {
            CreateFocusPoint();
        }
    }

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

    private FocusPointState AssignFocusPoint(ThoughtFocusable f, MoodPawn p)
    {
        FocusPointState next = GetNextFreeFocusPoint();
        if (next != null)
        {
            next.focusable = f;
            RectTransform toMove = next.point.GetObjectToMove();
            toMove.DOLocalMove(f.transform.localPosition - toMove.parent.localPosition, focusDuration).SetEase(easeFocus).SetUpdate(true);
            return next;
        }
        else return null;
    }

    private FocusPointState DeassignFocusPoint(ThoughtFocusable f, MoodPawn p)
    {
        FocusPointState next = GetFocusPointFrom(f);
        if (next != null)
        {
            next.focusable = null;
            RectTransform rt = next.point.GetObjectToMove();
            if (rt.gameObject.activeInHierarchy)
                rt.DOLocalMove(Vector3.zero, focusDuration).SetEase(easeFocus).SetUpdate(true);
            else rt.localPosition = Vector3.zero;
            return next;
        }
        else return null;
    }

    private int GetAmountOfFreeFocusPoints()
    {
        return _focusState.Count((x)=> x.focusable == null);
    }

    private int GetAmountOfFocusPoints()
    {
        return _focusState.Count;
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

    #region Experience
    internal class FocusedThoughtState
    {
        internal Thought thought;
        internal int experienceCount;

        internal void AddCount(int amount)
        {
            experienceCount += amount;
        }

        internal void ClearCount()
        {
            experienceCount = 0;
        }
    }

    internal int _experienceChange = 0;
    private void OnExperienceChange(int amount)
    {
        _experienceChange += amount;
        foreach(FocusedThoughtState state in _activeThoughts)
        {
            state.AddCount(amount);
        }
    }
    #endregion

    #region Thoughts and effects
    List<FocusedThoughtState> _activeThoughts = new List<FocusedThoughtState>(8);
    
    protected virtual void StartThought(MoodPawn p, Thought t, FocusPointState f)
    {
        _activeThoughts.Add(new FocusedThoughtState() { thought = t, experienceCount = 0});
        StartCoroutine(EffectRoutine(p, t, f));
    }

    protected virtual void StopThought(MoodPawn p, Thought t, FocusPointState focusPoint)
    {
        focusPoint.point.UnsetExperienceText();
        _activeThoughts.Remove(GetActiveThought(t));
        StartCoroutine(RemoveEffectRoutine(p, t));
    }

    protected virtual bool HasActiveThought(Thought t)
    {
        return _activeThoughts.Any((x) => x.thought == t);
    }

    internal virtual FocusedThoughtState GetActiveThought(Thought t)
    {
        return _activeThoughts.FirstOrDefault((x) => x.thought == t);
    }

    protected int GetUnusedExperience(Thought t, bool clear)
    {
        FocusedThoughtState state = GetActiveThought(t);
        int experience = 0;
        if (state != null)
        {
            experience = state.experienceCount;
        }
        if(clear)
        {
            state.ClearCount();
        }
        return experience;
    }

    private class WaitForExperienceChange : CustomYieldInstruction
    {
        ThoughtSystemController system;
        public WaitForExperienceChange(ThoughtSystemController s)
        {
            system = s;
        }

        public override bool keepWaiting
        {
            get
            {
                return system._experienceChange == 0;
            }
        }
    }

    private IEnumerator EffectRoutine(MoodPawn p, Thought t, FocusPointState focusPoint)
    {
        int experience = 0;
        focusPoint.point.SetExperienceText(experience, t.experienceNeeded);
        yield return StartCoroutine(t.FocusEffect(p, this));
        while (!t.CheckExperienceCondition(p, this, experience) && HasActiveThought(t))
        {
            yield return null;
            experience += GetUnusedExperience(t, clear:true);
            focusPoint.point.SetExperienceText(experience, t.experienceNeeded);
        }
        focusPoint.point.UnsetExperienceText();
        if(HasActiveThought(t))
        {
            yield return StartCoroutine(t.ExperienceCompleteEffect(p, this));
            if(t.consumedWhenExperienced)
            {
                FindRemoveThought(t, p, ThoughtPlacement.Up);
            }
        }
    }

    private IEnumerator RemoveEffectRoutine(MoodPawn p, Thought t)
    {
        yield return StartCoroutine(t.RemoveFocusEffect(p, this));
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

    private void CheckSelected(bool forceFeedback = false)
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
            OnNewSelectedFocusable(newFocus);
        } else if (forceFeedback)
        {
            OnNewSelectedFocusable(newFocus);
        }
    }

    private void OnNewSelectedFocusable(ThoughtFocusable newFocus)
    {
        if(newFocus != null)
        {
            descriptor.text = newFocus.GetThought().GetDescription();
        }
        else
        {
            descriptor.text = "Select a thought to focus on.";
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
