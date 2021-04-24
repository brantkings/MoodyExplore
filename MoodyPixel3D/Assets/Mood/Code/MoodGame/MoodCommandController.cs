using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEditor;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;
using System.Linq;

public class MoodCommandController : MonoBehaviour
{
    [SerializeField]
    private MoodCommandOption _optionPrefab;
    [SerializeField]
    private GameObject _optionParent;


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

    [Space()]
    [SerializeField]
    private MoodSkillSet _innateEquippedSkills;

    [Space()]
    public bool unselectOnDeactivated = true;

    public float canvasGroupFadeDuration = 0.45f;

    private bool? _activated;

    private int _currentOption;
    
    private struct OptionTuple
    {
        public MoodCommandOption command;
        public NodeItem node;
    }


    public SoundEffect[] onChangeOption;


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
        public MoodItem item;

        public NodeItemLeaf(MoodSkill s, MoodItem i)
        {
            skill = s;
            item = i;
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

    private void OnEnable()
    {
        _pawn.Inventory.OnInventoryChange += OnInventoryChange;
    }


    private void OnDisable()
    {

        _pawn.Inventory.OnInventoryChange -= OnInventoryChange;
    }

    private void Start()
    {
        OrganizeOptions(ListAllSkills());
        _currentParent = mainParent;
        RemakeOptions(_currentParent, true, false);
    }

    private IEnumerable<Tuple<MoodSkill, MoodItem>> ListAllSkills()
    {
        if (_innateEquippedSkills != null)
            foreach (MoodSkill skill in _innateEquippedSkills) yield return new Tuple<MoodSkill, MoodItem>(skill, null);
        if (_pawn.Inventory != null)
            foreach (Tuple<MoodSkill, MoodItem> tuple in _pawn.Inventory.GetAllUsableSkills()) yield return tuple;

    }

    private void RemakeOptions(NodeItemParent parent, bool selectFirst, bool feedbacks)
    {
        MakeOptions(parent);
        if(selectFirst)
        {
            _currentOption = -1;
            MoveSelected(1, feedbacks);
        }
        else
        {
            MoveSelected(0, feedbacks);
        }
    }

    private void OrganizeOptions(IEnumerable<Tuple<MoodSkill, MoodItem>> skills)
    {
        Dictionary<MoodSkillCategory, NodeItemParent> dic = new Dictionary<MoodSkillCategory, NodeItemParent>(8);
        List<NodeItemLeaf> categoryLessSkills = new List<NodeItemLeaf>(8);

        foreach(Tuple<MoodSkill, MoodItem> skill in skills)
        {
            MoodSkillCategory cat = skill.Item1.GetCategory();
            Debug.LogFormat("Adding skill {0} of {1}, category {2}", skill.Item1, skill.Item2, skill.Item1.GetCategory());
            if(cat != null)
            {
                if(!dic.ContainsKey(cat))
                {
                    dic.Add(cat, new NodeItemParent(cat));
                }
                dic[cat].items.Add(new NodeItemLeaf(skill.Item1, skill.Item2));
            }
            else
            {
                categoryLessSkills.Add(new NodeItemLeaf(skill.Item1, skill.Item2));
            }
        }

        if (mainParent == null) mainParent = new NodeItemParent(null);
        else mainParent.Clear();

        //TODO: Pass through every t in dic again here, now putting every category in its parent.

        foreach (var t in dic.Keys.OrderByDescending((x)=>x.GetPriority()))
        {
            mainParent.items.Add(dic[t]);
        }
        foreach(var s in categoryLessSkills)
        {
            mainParent.items.Add(s);
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
            if (set && unselectOnDeactivated && _currentParent != null) DeselectToRoot(false);
            SetActiveObjects(set, GetCurrentSkill());
            _activated = set;
        }
    }

    private OptionTuple? GetCurrentOption()
    {
        if (_options == null || _currentOption < 0 || _currentOption >= _options.Count) return null;
        return _options[_currentOption];
    }

    private int GetIndexOf(NodeItemParent context, NodeItem indexer)
    {
        return context.items.IndexOf(indexer);
    }

    private int GetIndexOfInCurrentContext(NodeItem indexer)
    {
        return GetIndexOf(_currentParent, indexer);
    }

    public MoodCommandOption GetCurrentCommandOption()
    {
        return GetCurrentOption()?.command;
    }

    public MoodSkill GetCurrentSkill()
    {
        return GetCurrentOption()?.node.GetSelectable() as MoodSkill;
    }

    public MoodItem GetCurrentItem()
    {
        return (GetCurrentOption()?.node as NodeItemLeaf)?.item;
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

    public void MoveSelected(int add, bool feedbacks = true)
    {
        int oldOption = _currentOption;
        while (_options.Count <= 0) Deselect(false, false);
        if(_currentOption != -1) SetSelected(_currentOption, false);
        MoveIndex(ref _currentOption, add);
        while(!IsVisible(_currentOption) && _currentOption != oldOption)
        {
            MoveIndex(ref _currentOption, Mathf.RoundToInt(Mathf.Sign(add)));
            //Debug.LogFormat("Hey trying {0}", _currentOption);
        }
        SetSelected(_currentOption, true );
        SetActiveObjects(IsActivated(), GetCurrentSkill());
        if(feedbacks) onChangeOption.Invoke(transform);
    }

    private bool IsVisible(int index) 
    {
        if (index < 0 || index >= _options.Count) return false;
        return _options[index].command.gameObject.activeSelf;
    }

    private void SetSelected(int index, bool selected)
    {
        if (index < 0 || index >= _options.Count) return;
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

    public void UpdateCommandView(MoodPawn pawn, MoodSkill currentSkill, Vector3 sanitizedDirection, bool assureSelected)
    {
        PaintOptions(pawn, sanitizedDirection);
        if(assureSelected) AssureSelectedOptionIsVisible();

        if(currentSkill != null)
        {
            foreach (IRangeShowDirected directed in AllRangeShowDirected()) directed.SetDirection(pawn, currentSkill, sanitizedDirection);
            currentSkill.SetShowDirection(pawn, sanitizedDirection);
        }
    }

    public IEnumerable<Tuple<MoodSkill, MoodItem>> GetAllMoodSkills()
    {
        foreach (Tuple<MoodSkill, MoodItem> tuple in GetAllMoodSkills(mainParent)) yield return tuple;
    }

    private IEnumerable<Tuple<MoodSkill, MoodItem>> GetAllMoodSkills(NodeItemParent parent)
    {
        foreach(NodeItem node in parent)
        {
            if(node is NodeItemParent)
            {
                var nodeParent = node as NodeItemParent;
                foreach (var tuple in GetAllMoodSkills(nodeParent)) yield return tuple;
            }
            else if(node is NodeItemLeaf)
            {
                var leaf = node as NodeItemLeaf;
                yield return new Tuple<MoodSkill, MoodItem>(leaf.skill, leaf.item);
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
        GetCurrentOption()?.command.FeedbackConfirmSelection();
        GetCurrentOption()?.node.Select(ref _currentParent);
        if(oldParent != _currentParent)
        {
            RemakeOptions(_currentParent, true, true);
        }
    }

    private void SetNullAsSelected(bool setNotSelectingAnyone)
    {
        GetCurrentOption()?.command.SetSelected(false);
        if(setNotSelectingAnyone) _currentOption = -1;
    }

    public bool Deselect(bool toRoot = false, bool feedbacks = true)
    {
        NodeItemParent oldParent = _currentParent;
        if (_currentParent != null) _currentParent.Deselect(ref _currentParent);
        if (_currentParent == null || toRoot) _currentParent = mainParent;
        if (oldParent != _currentParent)
        {
            RemakeOptions(_currentParent, false, false);
            //_currentOption = 0;
            int wantIndex = GetIndexOfInCurrentContext(oldParent);
            MoveSelected(wantIndex - _currentOption, feedbacks);
            return true;
        }
        else return false;
    }

    public bool DeselectToRoot(bool feedbacks)
    {
        return Deselect(true, feedbacks);
    }
    
    public void DeselectToNull(bool feedbacks, bool setNotSelectingAnyone)
    {
        DeselectToRoot(feedbacks);
        SetNullAsSelected(setNotSelectingAnyone);
    }

    private void OnInventoryChange(MoodInventory inventory)
    {
        Debug.LogFormat("[COMMAND] Inventory changed.");
        OrganizeOptions(ListAllSkills());
        _currentParent = mainParent;
        RemakeOptions(_currentParent, false, false);
    }
}
