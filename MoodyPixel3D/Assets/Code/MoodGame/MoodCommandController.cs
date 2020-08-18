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
    private RangeTarget _targetIndicator;
    [SerializeField]
    private Canvas _canvas;

    private int _currentOption;

    private void Awake()
    {
        _sphereIndicator = GetComponentInChildren<RangeSphere>();
        _arrowIndicator = GetComponentInChildren<RangeArrow>();
        _targetIndicator = GetComponentInChildren<RangeTarget>();

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

    public IEnumerator ExecuteCurrent(MoodPawn pawn, Vector3 direction)
    {
        yield return pawn.ExecuteSkill(GetCurrentSkill(), direction);
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
        _options[index].SetSelected(selected);
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
        
        _canvas.gameObject.SetActive(active);
    }

    public bool IsActivated()
    {
        return _canvas.gameObject.activeSelf;
    }

    public void UpdateCommandView(MoodPawn pawn, Vector3 direction)
    {
        GetCurrentSkill().SetShowDirection(pawn, direction);
        
        _arrowIndicator.SetDirection(direction);
    }
    
    
    
}
