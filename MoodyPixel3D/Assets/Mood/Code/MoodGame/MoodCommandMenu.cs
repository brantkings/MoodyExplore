using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using LHH.ScriptableObjects.Events;
using LHH.Menu;

public class MoodCommandMenu : MonoBehaviour
{
    public RectTransform columnPrefab;
    public MoodCommandOption optionPrefab;

    public Text titleText;
    public string titlePrefix = "< ";

    public RectTransform columnParent;

    public ScriptableEvent[] onChangeOption;

    public MoodSkillCategory inventoryItemCategory;
    public MoodInventoryMenu inventoryMenu;

    public float changeDuration = 0.25f;
    public float changeElasticOvershoot = 0.25f;
    public float changeElasticPeriod = 1f;
    public float trembleDuration = 0.15f;

    #region Option types
    
    public class CustomCommand : IMoodSelectable, IMoodSelectableCustomPress
    {
        public delegate void ArbitraryFunction();
        ArbitraryFunction onSelect;
        ArbitraryFunction onDeselect;
        public string name;
        public string buttonText;
        public Color? color;
        MoodSkillCategory category;

        string IMoodSelectable.name => name;

        public CustomCommand(string name, string buttonText, MoodSkillCategory category, ArbitraryFunction onSelect, ArbitraryFunction onDeselect, Color? color = null)
        {
            this.onSelect = onSelect;
            this.onDeselect = onDeselect;
            this.name = name;
            this.buttonText = buttonText;
            this.color = color;
            this.category = category;
        }


        public bool CanBePressed(MoodPawn pawn, Vector3 where)
        {
            return true;
        }

        public bool CanBeShown(MoodPawn pawn)
        {
            return !pawn.Inventory.Equals(null);
        }

        public void DrawCommandOption(MoodPawn pawn, MoodCommandOption option)
        {
            option.SetText(GetName(pawn), "Open up your inventory", 0f);
            option.SetFocusCost(0);
            option.SetStancePreview(null);
        }

        public Color? GetColor()
        {
            return category.GetColor();
        }

        public string GetName(MoodPawn pawn)
        {
            return name;
        }

        public void Press()
        {
            onSelect?.Invoke();
        }

        public void Unpress()
        {
            onDeselect?.Invoke();
        }
    }

    public class Option : IEnumerable<Option>
    {
        private IMoodSelectable selectable;
        public MoodItemInstance item;
        public MoodCommandOption instance;
        internal OptionColumn children;
        internal OptionColumn parent;

        internal Option(OptionColumn parent, IMoodSelectable selectable, MoodItemInstance owner)
        {
            this.parent = parent;
            this.selectable = selectable;
            item = owner;
        }
        internal Option(OptionColumn parent, IMoodSelectable selectable, MoodItemInstance owner, OptionColumn children)
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

        public bool Is(IMoodSelectable selectable)
        {
            return GetSelectable() == selectable;
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
            if(IsValidSelection() && !IsCurrentValid())
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

        public bool Set(OptionColumn column, int newIndex)
        {
            int oldIndex = index;
            OptionColumn oldColumn = current;
            DeselectCurrent();
            current = column;
            index = current != null ? Mathf.RoundToInt(Mathf.Clamp(newIndex, 0, current.options.Count)) : -1;
            SelectCurrent();
            return oldColumn != current || oldIndex != index;
        }

        public bool Set(int newIndex)
        {
            int oldIndex = index;
            DeselectCurrent();
            index = current != null ? Mathf.RoundToInt(Mathf.Clamp(newIndex, 0, current.options.Count)) : -1;
            SelectCurrent();
            return oldIndex != index;
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
            while (IsValidSelection() && !IsCurrentValid())
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

        public bool Is(IMoodSelectable selectable)
        {
            Option current = GetCurrent();
            if(current != null)
            {
                return current.Is(selectable);
            }
            return false;
        }

        public bool IsOrIsChildOf(IMoodSelectable selectable)
        {
            Option current = GetCurrent();
            while (current != null)
            {
                if (current.Is(selectable)) return true;
                current = current.parent?.parentOption;
            }
            return false;
        }
    }

    #endregion

    private OptionColumn main;
    private Selection current;
    private MoodPawn commandPawn;
    private IMoodSelectableCustomPress customPress;

    private void Awake()
    {
        current.Invalidate();
        current.Set(main, 0);

        commandPawn = GetComponentInParent<MoodPawn>();
        if (commandPawn == null) commandPawn = transform.root.GetComponentInChildren<MoodPawn>();
    }

    private void OnEnable()
    {
        foreach(var menu in GetSecondaryMenus())
        {
            if (!menu.Equals(null)) menu.SetActive(false);
        }

        
        current.Set(main, 0);
        SetChildrenColumnsActivated(current);
        CheckCurrentCustomPress(current);
        StartCoroutine(JustSelectFeedbackRoutine(commandPawn, 0f));
    }

    #region Interface to outside

    public void Select(MoodPawn pawn, bool feedbacks)
    {
        var sMenu = GetFirstActiveSecondaryMenu();
        if (sMenu != null)
        {
            sMenu.SelectCurrent(feedbacks);
            return;
        }

        if(customPress != null && !customPress.Equals(null))
        {
            customPress.Press();
        }

        if (current.WillEnter())
        {
            SetChildrenColumnsActivated(current);
            FeedbackCurrentOption(feedbacks);
        }
        bool moved = current.Enter();
        FeedbackTreeMovementTry(pawn, moved, feedbacks, JustSelectFeedbackRoutine);
    }


    public void Deselect(MoodPawn pawn, bool feedbacks)
    {
        var sMenu = GetFirstActiveSecondaryMenu();
        if (sMenu != null && sMenu is IPrefabListMenuDeselectCommand)
        {
            (sMenu as IPrefabListMenuDeselectCommand).Deselect(feedbacks);
            return;
        }

        if (customPress != null && !customPress.Equals(null))
        {
            customPress.Unpress();
        }


        bool moved = current.Exit();
        FeedbackTreeMovementTry(pawn, moved, feedbacks, SelectAndActivateTreeFeedbackRoutine);
    }

    public void DeselectAll(MoodPawn pawn, bool feedbacks)
    {
        bool did = false;
        while (current.Exit()) did = true;

        if (did) StartCoroutine(SelectAndActivateTreeFeedbackRoutine(pawn, changeDuration));
    }


    public void ChangeSelection(int change, bool feedbacks)
    {
        var sMenu = GetFirstActiveSecondaryMenu();
        if (sMenu != null)
        {
            sMenu.MoveSelection(change, feedbacks);
            return;
        }

        current.Move(change);
        FeedbackChangeSound(feedbacks);
        CheckCurrentCustomPress(current);
    }


    public Option GetCurrentOption()
    {
        return current.GetCurrent();
    }

    private void CheckCurrentCustomPress(Selection s)
    {
        //Current
        if (customPress != null && !customPress.Equals(null))
        {
            customPress.Unpress();
        }

        //Get next
        IMoodSelectable sel = s.GetCurrent()?.GetSelectable();
        if(sel != null && sel is IMoodSelectableCustomPress)
        {
            customPress = sel as IMoodSelectableCustomPress;
        }
        else
        {
            customPress = null;
        }
    }

    private delegate IEnumerator DelMovementRoutine(MoodPawn pawn, float duration);
    private void FeedbackTreeMovementTry(MoodPawn pawn, bool moved, bool feedbacks, DelMovementRoutine movementRoutine)
    {
        if (moved)
        {
            StartCoroutine(movementRoutine(pawn, changeDuration));
        }
        else
        {
            TrembleMovement(10, trembleDuration);
        }
        FeedbackChangeSound(feedbacks);
    }

    private void FeedbackCurrentOption(bool feedbacks)
    {
        if (feedbacks) current.GetCurrent()?.instance.FeedbackConfirmSelection();
    }

    private void FeedbackChangeSound(bool feedbacks)
    {
        if (feedbacks) onChangeOption.Invoke(transform);
    }

    /// <summary>
    /// Select directly an option that represents a category and returns if it changed the option or not.
    /// </summary>
    /// <param name="category"></param>
    /// <returns>If the category changed</returns>
    /// 
    public enum SelectCategoryResult
    {
        Changed,
        Unchanged,
        ParameterNotValid
    }
    public SelectCategoryResult SelectCategory(MoodPawn pawn, MoodSkillCategory category, bool feedbacks)
    {
        foreach(var sMenu in GetActiveSecondaryMenus())
        {
            Debug.LogFormat("[MENU] {0} is active", sMenu);
            sMenu.SetActive(false);
        }

        Option option = main.options.FirstOrDefault((x) => (x.GetSelectable() as MoodSkillCategory) == category);
        if(option != null)
        {
            if (current.IsOrIsChildOf(category)) return SelectCategoryResult.Unchanged;
            bool changed = current.Set(option.parent, option.parent.IndexOf(option));
            FeedbackTreeMovementTry(pawn, changed, feedbacks, SelectAndActivateTreeFeedbackRoutine);
            CheckCurrentCustomPress(current);
            return SelectCategoryResult.Changed;
        }
        else return SelectCategoryResult.ParameterNotValid;
    }

    #region Tween
    private IEnumerator JustSelectFeedbackRoutine(MoodPawn pawn, float duration)
    {
        yield return null;
        yield return GotoColumn(pawn, current.GetCurrentColumn(), duration);
    }

    private IEnumerator SelectAndActivateTreeFeedbackRoutine(MoodPawn pawn, float duration)
    {
        yield return null;
        yield return GotoColumn(pawn, current.GetCurrentColumn(), duration).OnKill(()=>SetChildrenColumnsActivated(current));
    }

    private Tween _columnTween;

    private Tween TrembleMovement(float force, float duration)
    {
        _columnTween.CompleteIfActive(true);
        _columnTween = columnParent.transform.DOShakePosition(duration, Vector3.right * force, 60, 90).SetUpdate(true).SetEase(Ease.OutCirc);
        return _columnTween;
    }

    private Tween GotoColumn(MoodPawn pawn, OptionColumn column, float duration)
    {
        _columnTween.CompleteIfActive(true);
        _columnTween = columnParent.transform.DOLocalMoveX(-column.instance.localPosition.x, duration).SetUpdate(true).SetEase(Ease.OutElastic, changeElasticOvershoot, changeElasticPeriod);
        if(titleText != null)
        {
            if(column.parentOption != null)
            {
                titleText.text = titlePrefix + column.parentOption.GetSelectable().GetName(pawn);
                Color? parentColor = column.parentOption.GetSelectable().GetColor();
                if (parentColor.HasValue)
                    titleText.color = parentColor.Value;
                else
                    titleText.color = Color.white;
                titleText.transform.position = column.parentOption.instance.transform.position;
                titleText.transform.DOLocalMove(Vector3.zero, duration).SetUpdate(true).SetEase(Ease.OutExpo, changeElasticOvershoot, changeElasticPeriod);
            }
            else
            {
                titleText.text = "";
                titleText.color = Color.white;
            }
        }
        return _columnTween;
    }
    #endregion

    #endregion

    #region Make options
    public void CreateAndBuildOptions(MoodPawn pawn, IEnumerable<(MoodSkill, MoodItemInstance)> skills)
    {
        Dictionary<MoodSkillCategory, OptionColumn> dic = new Dictionary<MoodSkillCategory, OptionColumn>(8);
        List<Option> categoryLessSkills = new List<Option>(8);

        foreach ((MoodSkill, MoodItemInstance) skill in skills)
        {
            MoodSkillCategory cat = skill.Item1.GetCategory();
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

        //Custom options
        CustomCommand custom1 = new CustomCommand("Inventory", "Inventory", inventoryItemCategory, () => {
            if(!inventoryMenu.IsActive()) 
                inventoryMenu.RepopulateWithDifferences();
            inventoryMenu.SetActive(true);
        }, () => inventoryMenu.SetActive(false), Color.blue);
        main.options.Add(new Option(main, custom1, null));

        MakeInstances(pawn, main);

        if (!current.IsValidSelection()) current.Set(main, 0);
        if(current.Validate())
        {
            StartCoroutine(SelectAndActivateTreeFeedbackRoutine(pawn, changeDuration));
        }
        
        current.SelectCurrent();
    }

    private void MakeInstances(MoodPawn pawn, OptionColumn column)
    {
        if (column.options.Count <= 0) 
            return;

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
            opt.instance.name = "<Command>" + (string.IsNullOrEmpty(opt.item?.itemData.name) ? "" : opt.item.itemData.name + "_") + opt.GetSelectable()?.name;
#endif

            opt.GetSelectable()?.DrawCommandOption(pawn, opt.instance);
            if (opt.children != null)
                MakeInstances(pawn, opt.children);
        }
    }


    private void SetChildrenColumnsActivated(Selection select)
    {
        int i = 0;
        if(select.current != null)
        {
            foreach (Option opt in select.current.options)
            {
                bool active = i++ == select.index;
                if(opt.children != null)
                    SetTreeActivatedRecursively(opt.children, active);
            }
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

    #region Paint Options

    public void PaintOptions(MoodPawn pawn, Vector3 direction)
    {
        if (current.Validate())
        {
            current.SelectCurrent();
        }
        foreach (Option opt in GetAllOptions())
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

    #region SecondaryMenus
    public IEnumerable<LHH.Menu.IPrefabListMenu> GetSecondaryMenus()
    {
        yield return inventoryMenu;
    }

    public IEnumerable<LHH.Menu.IPrefabListMenu> GetActiveSecondaryMenus()
    {
        return GetSecondaryMenus().Where((x) => x != null && !x.Equals(null) && x.IsActive());
    }

    public LHH.Menu.IPrefabListMenu GetFirstActiveSecondaryMenu()
    {
        return GetActiveSecondaryMenus().FirstOrDefault();
    }

    #endregion

}
