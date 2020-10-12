using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEditor;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class MoodCommandController : MonoBehaviour
{
    [SerializeField]
    private MoodCommandOption _optionPrefab;
    [SerializeField]
    private GameObject _optionParent;

    [SerializeField]
    private List<MoodSkill> _equippedSkills;

    private List<OptionTuple> _options;

    
    private RangeSphere _sphereIndicator;
    private RangeArrow _arrowIndicator;
    private RangeTarget _targetIndicator;
    [SerializeField]
    private Canvas _canvas;
    [SerializeField]
    private CanvasGroup _canvasGroup;

    public float canvasGroupFadeDuration = 0.45f;

    private bool _activated;

    private int _currentOption;
    
    private struct OptionTuple
    {
        public MoodSkill skill;
        public MoodCommandOption command;
    }


    public SoundEffect[] onChangeOption;

    private void Awake()
    {
        _sphereIndicator = GetComponentInChildren<RangeSphere>();
        _arrowIndicator = GetComponentInChildren<RangeArrow>();
        _targetIndicator = GetComponentInChildren<RangeTarget>();

        Deactivate();
        FadeCanvasGroup(false, 0f);
    }

    private void Start()
    {
        MakeOptions();
        _currentOption = -1;
        MoveSelected(1);
    }

    private void MakeOptions()
    {
        foreach (Transform child in _optionParent.transform)
        {
            Destroy(child.gameObject);
        }
        _options = new List<OptionTuple>(12);
        foreach (MoodSkill skill in _equippedSkills)
        {
            MoodCommandOption child = Instantiate(_optionPrefab, _optionParent.transform);
            child.SetOption(skill);
            _options.Add(new OptionTuple()
            {
                skill = skill,
                command = child
            });
        }
    }
    
    private void PaintOptions(MoodPawn pawn, Vector3 direction)
    {
        if (_options != null)
        {
            foreach (OptionTuple opt in _options)
            {
                bool canBeShown = opt.skill.CanBeShown(pawn);
                if(!canBeShown)
                {
                    PaintOption(opt, canBeShown, false);
                }
                else
                {
                    bool canExecute = opt.skill.CanExecute(pawn, direction);
                    PaintOption(opt, canBeShown, canExecute);
                }
            }
        }
    }

    private void PaintOption(OptionTuple opt, bool canBeShown, bool canExecute)
    {
        opt.command.gameObject.SetActive(canBeShown);
        opt.command.SetPossible(canExecute);
    }

    public void Activate(Vector3 position, float radius)
    {
        transform.position = position;
        SetActiveObjects(true, GetCurrentSkill());
        
        TimeManager.Instance.ChangeTimeDelta(0.02f, "ControlSlow");
        _activated = true;
    }

    public void Deactivate()
    {
        SetActiveObjects(false, GetCurrentSkill());
        TimeManager.Instance.RemoveTimeDeltaChange("ControlSlow");
        _activated = false;
    }

    public MoodSkill GetCurrentSkill()
    {
        return _equippedSkills[_currentOption];
    }

    public IEnumerator ExecuteCurrent(MoodPawn pawn, Vector3 direction)
    {
        yield return GetCurrentSkill().ExecuteRoutine(pawn, direction);
    }

    private void AssureSelectedOptionIsVisible()
    {
        if(!IsVisible(_currentOption)) 
        {
            Debug.LogFormat("Current selected {0} is not visible", _currentOption);
            MoveSelected(1);
            Debug.LogFormat("Current selected {0} is visible?", _currentOption);
        }
    }

    private void MoveIndex(ref int current, int add)
    {
        current = Mathf.RoundToInt(Mathf.Repeat(current + add, _options.Count));
    }

    public void MoveSelected(int add)
    {
        int oldOption = _currentOption;
        if(_currentOption != -1) SetSelected(_currentOption, false);
        MoveIndex(ref _currentOption, add);
        while(!IsVisible(_currentOption) && _currentOption != oldOption)
        {
            MoveIndex(ref _currentOption, Mathf.RoundToInt(Mathf.Sign(add)));
            Debug.LogFormat("Hey trying {0}", _currentOption);
        }
        SetSelected(_currentOption, true );
        SetActiveObjects(IsActivated(), GetCurrentSkill());
        onChangeOption.Execute(transform);
    }

    private bool IsVisible(int index) 
    {
        return _options[index].command.gameObject.activeSelf;
    }

    private void SetSelected(int index, bool selected)
    {
        _options[index].command.SetSelected(selected);
    }

    private IEnumerable<IRangeShow> AllRangeShows()
    {
        yield return _sphereIndicator;
        yield return _arrowIndicator;
        yield return _targetIndicator;
    }


    private void SetActiveObjects(bool active, MoodSkill skillTo)
    {
        if (active)
        {
            //TODO this sucks. The skill should be able to both execute and draw itself. Not every drawer should be asking the current skill if the drawer can draw it. This sucks.
            foreach(IRangeShow show in AllRangeShows()) 
                show.ResolveSkill(skillTo);
        }
        else
        {
            foreach(IRangeShow show in AllRangeShows()) 
                show.Hide();
        }

        FadeCanvasGroup(active, canvasGroupFadeDuration);
    }

    private Tween FadeCanvasGroup(bool setOn, float duration)
    {
        _canvasGroup.DOKill();
        return _canvasGroup.DOFade(setOn ? 1f : 0f, duration).SetId(_canvasGroup).SetUpdate(true).SetEase(Ease.InOutSine);
    }

    public bool IsActivated()
    {
        return _activated;
    }

    public void UpdateCommandView(MoodPawn pawn, Vector3 direction)
    {
        
        _arrowIndicator.SetDirection(direction);
        PaintOptions(pawn, direction);
        AssureSelectedOptionIsVisible();
        GetCurrentSkill().SetShowDirection(pawn, direction);
    }

    public IEnumerable<MoodSkill> GetMoodSkills()
    {
        if (_options != null)
        {
            foreach (OptionTuple opt in _options)
            {
                yield return opt.skill;
            }
        }
    }
}
