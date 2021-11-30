using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEditor;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;
using System.Linq;
using LHH.Menu;

public class MoodCommandController : MonoBehaviour
{
    [SerializeField]
    private MoodCommandMenu _menu;

    private MoodPawn _pawn;


    private RangeSphere _sphereIndicator;
    private RangeArrow _arrowIndicator;
    private RangeTarget _targetIndicator;
    private RangeArea _areaOfEffectIndicator;
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
        _menu.CreateAndBuildOptions(_pawn, ListAllSkills());
    }

    private IEnumerable<(MoodSkill, MoodItemInstance)> ListAllSkills()
    {
        if (_innateEquippedSkills != null)
            foreach (MoodSkill skill in _innateEquippedSkills) yield return (skill, null);
        if (_pawn.Inventory != null)
            foreach ((MoodSkill, MoodItemInstance) tuple in _pawn.Inventory.GetAllUsableSkills()) yield return tuple;
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
            _menu.gameObject.SetActive(set);
            SetActiveObjects(set, set? GetCurrentSkill() : null);
            _activated = set;
        }
    }


    private int GetIndexOf(NodeItemParent context, NodeItem indexer)
    {
        return context.items.IndexOf(indexer);
    }

    public MoodCommandOption GetCurrentCommandOption()
    {
        return _menu.GetCurrentOption()?.instance;
    }

    public MoodSkill GetCurrentSkill()
    {
        return _menu.GetCurrentOption()?.GetSelectable() as MoodSkill;
    }

    public MoodItemInstance GetCurrentItem()
    {
        return _menu.GetCurrentOption()?.item;
    }


    public void MoveSelected(int add, bool feedbacks = true)
    {
        _menu.ChangeSelection(add, feedbacks);
        SetCurrentActiveObjects();
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

    private void SetCurrentActiveObjects()
    {
        SetActiveObjects(_activated.HasValue ? _activated.Value : false, GetCurrentSkill());
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
        _menu.PaintOptions(pawn, sanitizedDirection);
        if(assureSelected) AssureSelectedOptionIsVisible();

        if(currentSkill != null)
        {
            foreach (IRangeShowDirected directed in AllRangeShowDirected()) directed.SetDirection(pawn, currentSkill, sanitizedDirection);
            currentSkill.SetShowDirection(pawn, sanitizedDirection);
        }
    }

    public IEnumerable<(MoodSkill, MoodItemInstance)> GetAllMoodSkills(bool includeInactive = false)
    {
        foreach (MoodCommandMenu.Option opt in _menu.GetAllOptions(includeInactive))
        {
            IMoodSelectable select = opt.GetSelectable();
            if(select is MoodSkill)
            {
                yield return (select as MoodSkill, opt.item);
            }
        }
    }

    public MoodCommandMenu.SelectCategoryResult SelectCategory(MoodSkillCategory category, bool feedbacks)
    {
        return _menu.SelectCategory(_pawn, category, feedbacks);
    }

    private void AssureSelectedOptionIsVisible()
    {
        _menu.AssureSelectedIsVisible(_pawn);
    }


    private void SetNullAsSelected(bool setNotSelectingAnyone)
    {
        GetCurrentCommandOption().SetSelected(false);
        if(setNotSelectingAnyone) _currentOption = -1;
    }

    public void SelectCurrentOption(bool feedbacks = true)
    {
        _menu.Select(_pawn, feedbacks);
        SetCurrentActiveObjects();
    }

    public void ShowCurrentSelected(bool feedbacks = true)
    {
        _menu.ChangeSelection(0, feedbacks);
        SetCurrentActiveObjects();
    }

    public void Deselect(bool toRoot = false, bool feedbacks = true)
    {
        if (toRoot)
            _menu.DeselectAll(_pawn, feedbacks);
        else
            _menu.Deselect(_pawn, feedbacks);
        SetCurrentActiveObjects();
    }

    
    public void DeselectToNull(bool feedbacks, bool setNotSelectingAnyone)
    {
        Deselect(true, feedbacks);
        SetNullAsSelected(setNotSelectingAnyone);
    }

    private void OnInventoryChange()
    {
        _menu.CreateAndBuildOptions(_pawn, ListAllSkills());
        AssureSelectedOptionIsVisible();
        ShowCurrentSelected();
    }
}
