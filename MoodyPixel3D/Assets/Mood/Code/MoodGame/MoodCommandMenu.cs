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

    #region Option types
    

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

        public void Clear()
        {
            children?.Clear();
            Destroy(instance.gameObject);
            instance = null;
        }

        public bool IsValid()
        {
            return instance != null && instance.gameObject.activeSelf;
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
            options = new List<Option>(capacity);
            category = cat;
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

        public void Clear()
        {
            foreach(Option opt in options)
            {
                opt.Clear();
            }
            options.Clear();
            if(instance != null) Destroy(instance.gameObject);
            instance = null;
        }

        public bool IsValid()
        {
            return instance != null;
        }
    }

    #endregion

    #region Selection struct

    private struct Selection
    {
        internal int index;
        internal OptionColumn current;

        public void Invalidate()
        {
            index = -1;
            current = null;
        }

        public bool IsValidSelection()
        {
            return current != null && index >= 0 && index < current.options.Count;
        }

        public bool IsExistingOption()
        {
            return current != null && index == Mathf.Clamp(index, 0, current.options.Count);
        }

        public bool IsCurrentValid()
        {
            return IsValid(GetCurrent());
        }

        private bool IsValid(Option opt)
        {
            return opt != null && opt.IsValid();
        }

        public bool ThereIsValidOption()
        {
            if (current == null) return false;
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
            if(IsExistingOption() && !IsCurrentValid())
            {
                if(ThereIsValidOption())
                {
                    int miniMove = (int)Mathf.Sign(how);
                    do MoveIndex(miniMove); while (!IsCurrentValid());
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
                ExitCurrentOption();
                SelectCurrent();
                return true;
            }
            return false;
        }

        private void ExitCurrentOption()
        {
            Option parentOption = current.parentOption;
            current = parentOption.parent;
            index = Mathf.Clamp(current.IndexOf(parentOption), 0, current.options.Count);
        }

        public void DeselectCurrent()
        {
            GetCurrent()?.instance?.SetSelected(false);
        }


        public bool Validate()
        {
            if (current == null) return false;

            bool changed = false;
            while (current != null && !current.IsValid() || !ThereIsValidOption())
            {
                DeselectCurrent();
                ExitCurrentOption();
                changed = true;
            }
            while (IsExistingOption() && !IsCurrentValid())
            {
                DeselectCurrent();
                MoveIndex(-1);
                changed = true;
            }
            return changed;
            
        }

        public void SelectCurrent()
        {
            Debug.LogFormat("[MENU] Setting '{0}' from '{1}', selected as true", GetCurrentColumn()?.instance, GetCurrent()?.instance);
            GetCurrent()?.instance.SetSelected(true);
        }
    }

    #endregion

    private OptionColumn main;
    private Selection current;

    private void Awake()
    {
        current.Invalidate();
        current.Set(main, 0);
    }

    private void OnEnable()
    {
        if(!current.IsValidSelection())
        {
            if (current.ThereIsValidOption()) current.Validate();
            else current.Set(main, 0);
        }
        else
        {
        }
        SetTreeActivated(current);
        StartCoroutine(SelectFeedbackRoutine(0f));
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
            StartCoroutine(SelectFeedbackRoutine(changeDuration));
        }
        else
        {
            TrembleMovement(10, trembleDuration);
        }
        FeedbackChangeSound(feedbacks);
    }

    private IEnumerator SelectFeedbackRoutine(float duration)
    {
        yield return null;
        GotoColumn(current.GetCurrentColumn(), duration);
    }

    public void Deselect(bool feedbacks)
    {
        bool moved = current.Exit();
        if (moved)
        {
            StartCoroutine(DeselectFeedbackRoutine(changeDuration));
        }
        else
        {
            TrembleMovement(-10, trembleDuration);
        }
        FeedbackChangeSound(feedbacks);
    }

    private IEnumerator DeselectFeedbackRoutine(float duration)
    {
        yield return GotoColumn(current.GetCurrentColumn(), duration);
        SetTreeActivated(current);
    }

    public void DeselectAll(bool feedbacks)
    {
        bool did = false;
        while (current.Exit()) did = true;

        if (did) StartCoroutine(DeselectFeedbackRoutine(changeDuration));
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
        else main.Clear();


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
        if(current.Validate())
        {
            StartCoroutine(DeselectFeedbackRoutine(changeDuration));
        }
        
        current.SelectCurrent();
    }

    private void MakeInstances(OptionColumn column)
    {
        if (column.options.Count <= 0) 
            return;

        Debug.LogFormat("Making instance of {0}", column.category);

        if(column.instance == null) column.instance = Instantiate(columnPrefab, columnParent);
        column.instance.localPosition = Vector3.zero;
        column.instance.localRotation = Quaternion.identity;
#if UNITY_EDITOR
        column.instance.name = "<CommandSet>" + (string.IsNullOrEmpty(column.category?.name) ? "" : "_" + column.category.name);
#endif
        foreach (Option opt in column)
        {
            if(opt.instance == null) opt.instance = Instantiate(optionPrefab, column.instance);
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
        foreach (Option opt in select.current?.options)
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

    #region Paint Options

    public void PaintOptions(MoodPawn pawn, Vector3 direction)
    {
        if(current.Validate())
        {
            current.SelectCurrent();
        }
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
