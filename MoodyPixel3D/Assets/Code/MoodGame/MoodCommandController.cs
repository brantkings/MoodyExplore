using System;
using System.Collections;
using System.Collections.Generic;
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

    private List<MoodCommandOption> _options;
    
    private RangeSphere _sphereIndicator;
    private RangeArrow _arrowIndicator;
    [SerializeField]
    private Canvas _canvas;

    private int _currentOption;

    private void Awake()
    {
        _sphereIndicator = GetComponentInChildren<RangeSphere>();
        _arrowIndicator = GetComponentInChildren<RangeArrow>();

        Deactivate();
    }

    private void Start()
    {
        MakeOptions();
        SetSelected(0, true);
    }

    private void MakeOptions()
    {
        foreach (Transform child in _optionParent.transform)
        {
            Destroy(child.gameObject);
        }
        _options = new List<MoodCommandOption>(12);
        foreach (MoodSkill skill in _equippedSkills)
        {
            MoodCommandOption child = Instantiate(_optionPrefab, _optionParent.transform);
            child.SetOption(skill);
            _options.Add(child);
        }
    }

    public void Activate(Vector3 position, float radius)
    {
        transform.position = position;
        SetActiveObjects(true, GetCurrentSkill());
    }

    public void Deactivate()
    {
        SetActiveObjects(false, GetCurrentSkill());
    }

    private MoodSkill GetCurrentSkill()
    {
        return _equippedSkills[_currentOption];
    }

    public void ExecuteCurrent(MoodPawn pawn, Vector3 direction)
    {
        GetCurrentSkill().Execute(pawn, direction);
    }

    public void MoveSelected(int add)
    {
        if(_currentOption != -1) SetSelected(_currentOption, false);
        _currentOption = Mathf.RoundToInt(Mathf.Repeat(_currentOption + add, _options.Count));
        SetSelected(_currentOption, true );
        SetActiveObjects(IsActivated(), GetCurrentSkill());
    }

    private void SetSelected(int index, bool selected)
    {
        Debug.LogFormat("Setting {0} as selected:{1}, ({2})", index, selected, Time.time);
        _options[index].SetSelected(selected);
    }
    

    private void SetActiveObjects(bool active, MoodSkill skillTo)
    {
        if (active)
        {
            if(skillTo is IRangeSphereSkill)
                _sphereIndicator.Show((skillTo as IRangeSphereSkill).GetRangeSphereProperties());
            else 
                _sphereIndicator.Hide();

            if (skillTo is IRangeArrowSkill)
                _arrowIndicator.Show((skillTo as IRangeArrowSkill).GetRangeArrowProperties());
            else 
                _arrowIndicator.Hide();
        }
        else
        {
            _sphereIndicator.Hide();
            _arrowIndicator.Hide();
        }
        
        _canvas.gameObject.SetActive(active);
    }

    public bool IsActivated()
    {
        return _canvas.gameObject.activeSelf;
    }

    public void SetDirection(Vector3 direction)
    {
        _arrowIndicator.SetDirection(direction);
    }
    
    
    
}
