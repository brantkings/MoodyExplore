using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LHH.Menu;
using LHH.Utils;
using DG.Tweening;
using System.Linq;

public class MoodInventoryMenu : PrefabListMenu<MoodInventoryMenuItem, MoodItemInstance>
{
    private IMoodInventory _inventory;
    private MoodPawn _pawn;
    public RectTransform contentWalker;
    public RectTransform possibleView;
    public Transform bagParent;
    public Transform equippedParent;

    [Space()]

    public float tweenDuration = 0.15f;
    public Ease tweenEase = Ease.OutSine;

    private float _optionHeight;
    private bool _started;

    private void Awake()
    {
        Get(ref _inventory);
        Get(ref _pawn);
        _optionHeight = optionPrefab.GetComponent<RectTransform>().rect.height;
    }

    private void Get<T>(ref T comp)
    {
        if (comp == null)
            comp = GetComponentInParent<T>();
        if(comp == null)
        {
            Debug.LogErrorFormat(this, "No {0} for {1}'s parents.", typeof(T), name);
        }
    }

    private void OnEnable()
    {
        _inventory.OnInventoryChange += OnInventoryChange;
    }

    private void OnDisable()
    {
        _inventory.OnInventoryChange -= OnInventoryChange;
    }

    private IEnumerator Start()
    {
        yield return null;
        _started = true;
    }

    override public void SetActive(bool active)
    {
        gameObject.SetActive(active);
        SetSelected(CurrentOption, false, false);
        _currentSelection = 0;
        SetSelected(CurrentOption, true, true);
    }

    override public bool IsActive()
    {
        return gameObject.activeSelf;
    }

    private void OnInventoryChange()
    {
        RepopulateWithDifferences();
    }

    public override IEnumerable<MoodItemInstance> GetOptionsPopulation()
    {
        return _inventory.GetAllItems();
    }

    public override int GetOptionsPopulationLength()
    {
        return _inventory.GetAllItemsLength();
    }

    public override void PopulateInstance(ref MoodInventoryMenuItem instance, MoodItemInstance origin)
    {
        instance.name = "Option_" + origin.itemData.name;
        if (instance.itemName != null) instance.itemName.text = origin.itemData.GetName();
        if (instance.itemSecondary != null) instance.itemSecondary.text = origin.itemData.WriteItemStatus(origin.properties, _pawn.HasEquipped(origin));
        if (instance.itemIcon != null)
        {
            instance.itemIcon.sprite = origin.itemData.GetIcon();
            instance.itemIcon.enabled = instance.itemIcon.sprite != null;
        }
    }

    public override void Reposition(Option option, int index, int length, bool justCreated)
    {
        Debug.LogFormat(option.currentOptionView, "Repositioning {0} {1}", option.currentInformation, justCreated);
        if(justCreated)
        {
            option.currentOptionView.transform.SetParent(bagParent, false);
        }
        option.currentOptionView.transform.SetSiblingIndex(index);
    }

    protected override void Select(Option option, bool feedbacks = true)
    {
        if(feedbacks) option.currentOptionView.anim.SetTrigger("Select");
        Debug.LogFormat(option.currentOptionView, "Just selected {0}", option);

        StartCoroutine(ItemSelectRoutine(_pawn.EquipSkill, _pawn.UnequipSkill, option.currentInformation));
        
    }

    private IEnumerator ItemSelectRoutine(MoodSkill equipSkill, MoodSkill unequipSkill, MoodItemInstance item)
    {
        if (!_pawn.HasEquipped(item))
        {
            if(_pawn.HasAnotherItemEquippedInSlot(item))
            {
                if (unequipSkill != null)
                {
                    yield return UseSkillWithItem(unequipSkill, _pawn.GetCurrentItemEquippedInSlot(item).First());
                }
                else
                {
                    _pawn.Unequip(item);
                }
            }

            if (equipSkill != null)
            {
                yield return UseSkillWithItem(equipSkill, item);
            }
            else
            {
                _pawn.Equip(item);
            }
        }
        else
        {
            if (unequipSkill != null)
            {
                yield return UseSkillWithItem(unequipSkill, item);
            }
            else
            {
                _pawn.Unequip(item);
            }
        }
    }

    private IEnumerator UnequipRoutine(MoodSkill skill, MoodItemInstance item)
    {
        if (skill != null)
        {
            yield return UseSkillWithItem(skill, item);
        }
        else
        {
            _pawn.Unequip(item);
        }
    }


    private IEnumerator UseSkillWithItem(MoodSkill skill, MoodItemInstance item)
    {
        if (_pawn.CanUseSkill(skill))
           yield return _pawn.ExecuteSkill(skill, Vector3.zero, item);
    }

    protected override void SetSelected(Option option, bool selected, bool feedbacks = true)
    {
        if (option == null) return;

        option.currentOptionView.anim.SetBool("Selected", selected);

        if(selected && _started)
        {
            RectTransformUtils.GetRectInRelationToParent(possibleView, contentWalker.parent as RectTransform, out Rect viewRect);
            RectTransformUtils.GetRectInRelationToParent(option.currentOptionView.transform as RectTransform, contentWalker.parent as RectTransform, out Rect optionRect);
            if(!viewRect.Overlaps(optionRect))
            {
                Vector2 targetMovement = Vector2.zero;
                if(optionRect.yMax < viewRect.yMin)
                {
                    targetMovement.y += viewRect.yMin - optionRect.yMax;
                }
                else if(optionRect.yMin > viewRect.yMax)
                {
                    targetMovement.y -= optionRect.yMin - viewRect.yMax;
                }

                if (optionRect.xMax < viewRect.xMin)
                {
                    targetMovement.x += viewRect.xMin - optionRect.xMax;
                }
                else if (optionRect.xMin > viewRect.xMax)
                {
                    targetMovement.x -= optionRect.xMin - viewRect.xMax;
                }

                DOTween.Kill(this);
                contentWalker.DOAnchorPos(targetMovement, tweenDuration, false).SetRelative(true).SetEase(tweenEase).SetUpdate(true).SetId(this);
            }
        }
    }
    
    private string GetInfoFromRectTransform(RectTransform t)
    {
        return $"{t.name}->anchorPosition: {t.anchoredPosition}, offsetMax:{t.offsetMax}, offsetMin:{t.offsetMin}, rect: {t.rect} (rectMinMaxY: {t.rect.min} {t.rect.max}).";
    }

}
