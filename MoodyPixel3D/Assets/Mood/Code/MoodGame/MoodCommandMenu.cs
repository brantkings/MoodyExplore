using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class MoodCommandMenu : MonoBehaviour
{
    public RectTransform columnPrefab;
    public MoodCommandOption optionPrefab;

    public RectTransform columnParent;

    public ScriptableEvent[] onChangeOption;

    public float changeDuration = 0.25f;
    public float changeElasticOvershoot = 0.25f;
    public float changeElasticPeriod = 1f;
    public float trembleDuration = 0.15f;



    public class Option : IEnumerable<Option>
    {
        private IMoodSelectable selectable;
        public MoodItem item;
        public MoodCommandOption instance;
        internal OptionColumn children;
        internal OptionColumn parent;

        internal Option(OptionColumn parent, IMoodSelectable selectable, MoodItem owner)
        {
            this.parent = parent;
            this.selectable = selectable;
            item = owner;
        }
        internal Option(OptionColumn parent, IMoodSelectable selectable, MoodItem owner, OptionColumn children)
        {
            this.parent = parent;
            this.selectable = selectable;
            this.item = owner;
            this.children = children;
        }

        public IEnumerator<Option> GetEnumerator()
        {
            return children.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)children).GetEnumerator();
        }

        public IMoodSelectable GetSelectable()
        {
            return selectable;
        }

    }

    internal class OptionColumn : IEnumerable<Option>
    {
        public MoodSkillCategory category;
        internal List<Option> options;
        public RectTransform instance;

        public Option parentOption;

        public OptionColumn(MoodSkillCategory cat, int capacity = 8)
        {
            category = cat;
            options = new List<Option>(capacity);
        }

        public IEnumerator<Option> GetEnumerator()
        {
            return options.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)options).GetEnumerator();
        }

        public void SetParent(Option parent)
        {
            parentOption = parent;
        }

        public int IndexOf(Option child)
        {
            return options.IndexOf(child);
        }
    }

    private struct Selection
    {
        internal int index;
        internal OptionColumn current;

        public void Invalidate()
        {
            index = -1;
            current = null;
        }

        public bool IsExistingOption()
        {
            return current != null && index == Mathf.Clamp(index, 0, current.options.Count);
        }

        public bool IsValidOption()
        {
            return IsValid(GetCurrent());
        }

        private bool IsValid(Option opt)
        {
            return opt.instance.isActiveAndEnabled;
        }

        public bool ThereIsValidOption()
        {
            foreach(Option opt in current.options)
            {
                if (IsValid(opt)) return true;
            }
            return false;
        }

        public Option GetOption(int indexSelected)
        {
            //Debug.LogFormat("Getting current {0} and {1}: {2}", current?.instance, index, current?.options[index]?.instance.name);
            if (current != null && indexSelected >= 0 && indexSelected < current.options.Count)
                return current.options[indexSelected];
            else return null;
        }

        public Option GetCurrent()
        {
            return GetOption(index);
        }

        public OptionColumn GetCurrentColumn()
        {
            return current;
        }

        private void MoveIndex(int how)
        {
            index = current != null ? Mathf.RoundToInt(Mathf.Repeat(index + how, current.options.Count)) : -1;
        }

        public void Move(int how)
        {
            DeselectCurrent();
            MoveIndex(how);
            if(IsExistingOption() && !IsValidOption())
            {
                if(ThereIsValidOption())
                {
                    int miniMove = (int)Mathf.Sign(how);
                    do MoveIndex(miniMove); while (!IsValidOption());
                }
                else
                {
                    Exit();
                    return;
                }
            }
            SelectCurrent();
        }

        public void Set(OptionColumn column, int how)
        {
            DeselectCurrent();
            current = column;
            index = current != null ? Mathf.RoundToInt(Mathf.Clamp(how, 0, current.options.Count)) : -1;
            SelectCurrent();
        }

        public void Set(int how)
        {
            DeselectCurrent();
            index = current != null ? Mathf.RoundToInt(Mathf.Clamp(how, 0, current.options.Count)) : -1;
            SelectCurrent();
        }

        public bool WillEnter()
        {
            return GetCurrent()?.children != null;
        }

        public bool Enter()
        {
            Option currentOption = GetCurrent();
            if (currentOption?.children != null)
            {
                DeselectCurrent();
                index = 0;
                current = currentOption.children;
                SelectCurrent();
                return true;
            }
            else return false;
        }

        public bool Exit()
        {
            if(current?.parentOption != null)
            {
                DeselectCurrent();
                Option parentOption = current.parentOption;
                current = parentOption.parent;
                index = Mathf.Clamp(current.IndexOf(parentOption), 0, current.options.Count);
                SelectCurrent();
                return true;
            }
            return false;
        }

        public void DeselectCurrent()
        {
            GetCurrent()?.instance.SetSelected(false);
        }

        public void SelectCurrent()
        {
            Debug.LogFormat("[MENU] Setting '{0}' from '{1}', selected as true", GetCurrentColumn()?.instance, GetCurrent()?.instance);
            GetCurrent()?.instance.SetSelected(true);
        }
    }

    private OptionColumn main;
    private Selection current;

    private void Awake()
    {
        current.Invalidate();
    }

    #region Interface to outside
    private void FeedbackCurrentOption(bool feedbacks)
    {
        if (feedbacks) current.GetCurrent()?.instance.FeedbackConfirmSelection();
    }

    private void FeedbackChangeSound(bool feedbacks)
    {
        if (feedbacks) onChangeOption.Invoke(transform);
    }

    public void Select(bool feedbacks)
    {
        if (current.WillEnter())
        {
            SetTreeActivated(current);
            FeedbackCurrentOption(feedbacks);
        }
        bool moved = current.Enter();
        if (moved)
        {
            StartCoroutine(SelectFeedbackRoutine());
        }
        else
        {
            TrembleMovement(10, trembleDuration);
        }
        FeedbackChangeSound(feedbacks);
    }

    private IEnumerator SelectFeedbackRoutine()
    {
        yield return null;
        GotoColumn(current.GetCurrentColumn(), changeDuration);
    }

    public void Deselect(bool feedbacks)
    {
        bool moved = current.Exit();
        if (moved)
        {
            StartCoroutine(DeselectFeedbackRoutine());
        }
        else
        {
            TrembleMovement(-10, trembleDuration);
        }
        FeedbackChangeSound(feedbacks);
    }

    private IEnumerator DeselectFeedbackRoutine()
    {
        yield return GotoColumn(current.GetCurrentColumn(), changeDuration);
        SetTreeActivated(current);
    }

    public void DeselectAll(bool feedbacks)
    {
        bool did = false;
        while (current.Exit()) did = true;

        if (did) GotoColumn(current.GetCurrentColumn(), changeDuration);
    }


    public void ChangeSelection(int change, bool feedbacks)
    {
        current.Move(change);
        FeedbackChangeSound(feedbacks);
    }

    public Option GetCurrentOption()
    {
        return current.GetCurrent();
    }

    #region Tween

    private Tween _columnTween;

    private Tween TrembleMovement(float force, float duration)
    {
        _columnTween.KillIfActive(true);
        _columnTween = columnParent.transform.DOShakePosition(duration, Vector3.right * force, 60, 90).SetUpdate(true).SetEase(Ease.OutCirc);
        return _columnTween;
    }

    private Tween GotoColumn(OptionColumn column, float duration)
    {
        _columnTween.KillIfActive(true);
        _columnTween = columnParent.transform.DOLocalMoveX(-column.instance.localPosition.x, duration).SetUpdate(true).SetEase(Ease.OutElastic, changeElasticOvershoot, changeElasticPeriod);
        return _columnTween;
    }
    #endregion
    #endregion

    #region Make options
    public void CreateAndBuildOptions(IEnumerable<Tuple<MoodSkill, MoodItem>> skills)
    {
        Dictionary<MoodSkillCategory, OptionColumn> dic = new Dictionary<MoodSkillCategory, OptionColumn>(8);
        List<Option> categoryLessSkills = new List<Option>(8);

        foreach (Tuple<MoodSkill, MoodItem> skill in skills)
        {
            MoodSkillCategory cat = skill.Item1.GetCategory();
            Debug.LogFormat("Adding skill {0} of {1}, category {2}", skill.Item1, skill.Item2, skill.Item1.GetCategory());
            Option theOption = new Option(null, skill.Item1, skill.Item2);
            if (cat != null)
            {
                if (!dic.ContainsKey(cat))
                {
                    dic.Add(cat, new OptionColumn(cat));
                }
                theOption.parent = dic[cat];
                dic[cat].options.Add(theOption);
            }
            else
            {
                categoryLessSkills.Add(theOption);
            }
        }

        if (main == null) main = new OptionColumn(null);
        else main.options.Clear();

        foreach (var category in dic.Keys.OrderByDescending((x) => x.GetPriority()))
        {
            Option categoryOption = new Option(main, category, null, dic[category]);
            dic[category].SetParent(categoryOption);
            main.options.Add(categoryOption); //Make an option for each category
        }
        foreach (var s in categoryLessSkills)
        {
            main.options.Add(s);
        }

        MakeInstances(main);

        if (!current.IsExistingOption()) current.Set(main, 0);
        current.SelectCurrent();
    }

    private void MakeInstances(OptionColumn column)
    {
        if (column.options.Count <= 0) 
            return;


        column.instance = Instantiate(columnPrefab, columnParent);
        column.instance.localPosition = Vector3.zero;
        column.instance.localRotation = Quaternion.identity;
#if UNITY_EDITOR
        column.instance.name = "<CommandSet>" + (string.IsNullOrEmpty(column.category?.name) ? "" : "_" + column.category.name);
#endif
        foreach (Option opt in column)
        {
            opt.instance = Instantiate(optionPrefab, column.instance);
            opt.instance.transform.localPosition = Vector3.zero;
            opt.instance.transform.localRotation = Quaternion.identity;
#if UNITY_EDITOR
            opt.instance.name = "Command_" + (string.IsNullOrEmpty(opt.item?.name) ? "" : opt.item.name + "_") + opt.GetSelectable()?.name;
#endif
            opt.GetSelectable()?.DrawCommandOption(opt.instance);
            if (opt.children != null)
                MakeInstances(opt.children);
        }
    }

    private void SetTreeActivated(Selection select)
    {
        int i = 0;
        foreach (Option opt in select.current.options)
        {
            bool active = i++ == select.index;
            if(opt.children != null)
                SetTreeActivatedRecursively(opt.children, active);
        }
    }

    private void SetTreeActivatedRecursively(OptionColumn column, bool active)
    {
        Debug.LogFormat("Setting {0} activated {1}.", column.instance, active);
        column?.instance?.gameObject.SetActive(active);
        foreach(Option opt in column.options)
        {
            if (opt.children != null)
                SetTreeActivatedRecursively(opt.children, active);
        }
    }

    #endregion


    #region Getter
    public IEnumerable<Option> GetAllOptions(bool includeInactive = false)
    {
        return GetAllOptions(main, includeInactive);
    }

    private IEnumerable<Option> GetAllOptions(OptionColumn column, bool includeInactive = false)
    {
        if (column == null) yield break;
        foreach(Option opt in column)
        {
            yield return opt;
            if (opt.children != null)
            {
                if(!includeInactive)
                {
                    if (!opt.children.instance.gameObject.activeInHierarchy) 
                        continue;
                }
                foreach (Option child in GetAllOptions(opt.children)) 
                    yield return child;
            }
        }
    }

    #endregion

    #region Paint

    public void PaintOptions(MoodPawn pawn, Vector3 direction)
    {
        foreach(Option opt in GetAllOptions())
        {
            bool canBeShown = opt.GetSelectable().CanBeShown(pawn);
            if (!canBeShown)
            {
                PaintOption(opt, canBeShown, false);
            }
            else
            {
                bool canExecute = opt.GetSelectable().CanBePressed(pawn, direction);
                PaintOption(opt, canBeShown, canExecute);
            }
        }
    }

    private void PaintOption(Option opt, bool canBeShown, bool canExecute)
    {
        opt.instance.gameObject.SetActive(canBeShown);
        opt.instance.SetPossible(canExecute, opt.GetSelectable());
    }

    #endregion
}
