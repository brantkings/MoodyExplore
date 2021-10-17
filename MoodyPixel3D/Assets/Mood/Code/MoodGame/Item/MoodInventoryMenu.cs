using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LHH.Menu;
using LHH.Utils;
using DG.Tweening;

public class MoodInventoryMenu : PrefabListMenu<MoodInventoryMenuItem, MoodItemInstance>
{
    public IMoodInventory _inventory;
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
        if(_inventory == null)
            _inventory = GetComponentInParent<IMoodInventory>();
        if(_inventory == null)
        {
            Debug.LogErrorFormat(this, "No inventory for {0}", this);
        }
        _optionHeight = optionPrefab.GetComponent<RectTransform>().rect.height;
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
        if (instance.itemSecondary != null) instance.itemSecondary.text = origin.itemData.WriteItemStatus(origin.properties);
        //if(instance.itemIcon != null) instance.itemIcon.sprite = origin.itemData.algumacoisa;
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

    protected override void Select(Option option)
    {
        option.currentOptionView.anim.SetTrigger("Select");
    }

    protected override void SetSelected(Option option, bool selected)
    {
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
