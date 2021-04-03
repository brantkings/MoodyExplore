using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEditor;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;

public class MoodCommandController : MonoBehaviour
{
    [SerializeField]
    private MoodCommandOption _optionPrefab;
    [SerializeField]
    private GameObject _optionParent;

    [SerializeField]
    private MoodSkillSet _equippedSkills;

    private List<OptionTuple> _options;

    private MoodPawn _pawn;


    private RangeSphere _sphereIndicator;
    private RangeArrow _arrowIndicator;
    private RangeTarget _targetIndicator;
    private RangeArea _areaOfEffectIndicator;
    [SerializeField]
    private Canvas _canvas;
    [SerializeField]
    private Text _descriptor;
    [SerializeField]
    private CanvasGroup _canvasGroup;

    public float canvasGroupFadeDuration = 0.45f;

    private bool? _activated;

    private int _currentOption;
    
    private struct OptionTuple
    {
        public MoodCommandOption command;
        public NodeItem node;
    }


    public SoundEffect[] onChangeOption;

    private void Awake()
    {
        _sphereIndicator = GetComponentInChildren<RangeSphere>();
        _arrowIndicator = GetComponentInChildren<RangeArrow>();
        _targetIndicator = GetComponentInChildren<RangeTarget>();
        _areaOfEffectIndicator = GetComponentInChildren<RangeArea>();

        _pawn = GetComponentInParent<MoodPawn>();

        Deactivate();
        FadeCanvasGroup(false, 0f);
    }

    public Text GetDescriptorText()
    {
        return _descriptor;
    }

    private abstract class NodeItem
    {
        public abstract IMoodSelectable GetSelectable();

        public abstract void Select(ref NodeItemParent currentParent);

        public abstract void Deselect(ref NodeItemParent currentParent);
    }

    private class NodeItemParent : NodeItem, IEnumerable<NodeItem>
    {
        public MoodSkillCategory category;
        public List<NodeItem> items;

        private NodeItemParent _parent;

        public NodeItemParent(MoodSkillCategory cat)
        {
            category = cat;
            items = new List<NodeItem>(8);
        }

        public void Clear()
        {
            items.Clear();
        }

        public IEnumerator<NodeItem> GetEnumerator()
        {
            for (int i = 0, l = items.Count; i < l; i++) yield return items[i];
        }

        public override IMoodSelectable GetSelectable()
        {
            return category;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            for (int i = 0, l = items.Count; i < l; i++) yield return items[i];
        }

        public override void Select(ref NodeItemParent currentParent)
        {
            _parent = currentParent;
            currentParent = this;
        }

        public override void Deselect(ref NodeItemParent currentParent)
        {
            Debug.LogFormat("Deselecting {0} to {1}", currentParent, _parent);
            currentParent = _parent;
        }

        public override string ToString()
        {
            return $"Parent Node:{category?.name}";
        }
    }

    private class NodeItemLeaf : NodeItem
    {
        public MoodSkill skill;

        public NodeItemLeaf(MoodSkill s)
        {
            skill = s;
        }

        public override IMoodSelectable GetSelectable()
        {
            return skill;
        }

        public override void Select(ref NodeItemParent currentParent)
        {
            
        }

        public override void Deselect(ref NodeItemParent currentParent)
        {
            
        }
    }

    private NodeItemParent mainParent;
    private NodeItemParent _currentParent;


    private void Start()
    {
        OrganizeOptions(_equippedSkills);
        _currentParent = mainParent;
        RemakeOptions(_currentParent, true);
        
    }

    private void RemakeOptions(NodeItemParent parent, bool selectFirst)
    {
        MakeOptions(parent);
        if(selectFirst)
        {
            _currentOption = -1;
            MoveSelected(1);
        }
        else
        {
            MoveSelected(0);
        }
    }

    private void OrganizeOptions(IEnumerable<MoodSkill> skills)
    {
        Dictionary<MoodSkillCategory, NodeItemParent> dic = new Dictionary<MoodSkillCategory, NodeItemParent>(8);
        List<MoodSkill> categoryLessSkills = new List<MoodSkill>(8);

        foreach(MoodSkill skill in skills)
        {
            MoodSkillCategory cat = skill.GetCategory();
            if(cat != null)
            {
                if(!dic.ContainsKey(cat))
                {
                    dic.Add(cat, new NodeItemParent(cat));
                }
                dic[cat].items.Add(new NodeItemLeaf(skill));
            }
            else
            {
                categoryLessSkills.Add(skill);
            }
        }

        if (mainParent == null) mainParent = new NodeItemParent(null);
        else mainParent.Clear();

        //TODO: Pass through every t in dic again here, now putting every category in its parent.

        foreach (var t in dic)
        {
            mainParent.items.Add(t.Value);
        }

    }

    private IEnumerable<NodeItem> GetAvailableOptions(NodeItemParent parentToDraw)
    {
        foreach (NodeItem node in parentToDraw)
        {
            yield return node;
        }
    }

    private void MakeOptions(NodeItemParent parentToDraw)
    {
        foreach (Transform child in _optionParent.transform)
        {
            Destroy(child.gameObject);
        }
        _options = new List<OptionTuple>(12);
        foreach (NodeItem node in GetAvailableOptions(parentToDraw))
        {
            MoodCommandOption child = Instantiate(_optionPrefab, _optionParent.transform);
            node.GetSelectable().DrawCommandOption(child);
            _options.Add(new OptionTuple()
            {
                node = node,
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
                bool canBeShown = opt.node.GetSelectable().CanBeShown(pawn);
                if(!canBeShown)
                {
                    PaintOption(opt, canBeShown, false);
                }
                else
                {
                    bool canExecute = opt.node.GetSelectable().CanBePressed(pawn, direction);
                    PaintOption(opt, canBeShown, canExecute);
                }
            }
        }
    }

    private void PaintOption(OptionTuple opt, bool canBeShown, bool canExecute)
    {
        opt.command.gameObject.SetActive(canBeShown);
        opt.command.SetPossible(canExecute, opt.node.GetSelectable());
    }

    public void Activate()
    {
        SetActive(false);
    }

    public void Deactivate()
    {
        SetActive(false);
    }

    public void SetActive(bool set)
    {
        if(set != _activated)
        {
            SetActiveObjects(set, GetCurrentSkill());
            _activated = set;
        }
    }

    private OptionTuple? GetCurrentOption()
    {
        if (_options == null || _currentOption < 0 || _currentOption >= _options.Count) return null;
        return _options[_currentOption];
    }

    public MoodSkill GetCurrentSkill()
    {
        return GetCurrentOption()?.node.GetSelectable() as MoodSkill;
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
            //Debug.LogFormat("Hey trying {0}", _currentOption);
        }
        SetSelected(_currentOption, true );
        SetActiveObjects(IsActivated(), GetCurrentSkill());
        onChangeOption.Invoke(transform);
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
        yield return _areaOfEffectIndicator;
    }

    private IRangeShowDirected[] directed;
    private IRangeShowDirected[] AllRangeShowDirected()
    {
        if (directed == null) directed = GetComponentsInChildren<IRangeShowDirected>();
        return directed;
    }


    private void SetActiveObjects(bool active, MoodSkill skillTo)
    {
        if (active && skillTo != null)
        {
            //TODO this sucks. The skill should be able to both execute and draw itself. Not every drawer should be asking the current skill if the drawer can draw it. This sucks.
            foreach(IRangeShow show in AllRangeShows()) 
                show.ShowSkill(_pawn, skillTo);
        }
        else
        {
            foreach(IRangeShow show in AllRangeShows()) 
                if(!show.Equals(null))
                    show.Hide(_pawn);
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
        return _activated.HasValue && _activated.Value;
    }

    public void UpdateCommandView(MoodPawn pawn, MoodSkill currentSkill, Vector3 sanitizedDirection)
    {
        PaintOptions(pawn, sanitizedDirection);
        AssureSelectedOptionIsVisible();

        if(currentSkill != null)
        {
            foreach (IRangeShowDirected directed in AllRangeShowDirected()) directed.SetDirection(pawn, currentSkill, sanitizedDirection);
            currentSkill.SetShowDirection(pawn, sanitizedDirection);
        }
    }

    public IEnumerable<MoodSkill> GetAllMoodSkills()
    {
        foreach (MoodSkill skill in GetAllMoodSkills(mainParent)) yield return skill;
    }

    private IEnumerable<MoodSkill> GetAllMoodSkills(NodeItemParent parent)
    {
        foreach(NodeItem node in parent)
        {
            if(node is NodeItemParent)
            {
                var nodeParent = node as NodeItemParent;
                foreach (MoodSkill skill in GetAllMoodSkills(nodeParent)) yield return skill;
            }
            else if(node is NodeItemLeaf)
            {
                var leaf = node as NodeItemLeaf;
                yield return leaf.skill;
            }
        }
    }

    public IEnumerable<MoodSkill> GetShowingMoodSkills()
    {
        if (_options != null)
        {
            foreach (OptionTuple opt in _options)
            {
                MoodSkill skill = opt.node.GetSelectable() as MoodSkill;
                if(skill != null)
                    yield return skill;
            }
        }
    }

    public void SelectCurrentOption()
    {
        NodeItemParent oldParent = _currentParent;
        GetCurrentOption()?.node.Select(ref _currentParent);
        if(oldParent != _currentParent)
        {
            RemakeOptions(_currentParent, true);
        }
    }

    public bool Deselect()
    {
        NodeItemParent oldParent = _currentParent;
        _currentParent.Deselect(ref _currentParent);
        if (_currentParent == null) _currentParent = mainParent;
        if (oldParent != _currentParent)
        {
            RemakeOptions(_currentParent, false);
            return true;
        }
        else return false;

    }
}
