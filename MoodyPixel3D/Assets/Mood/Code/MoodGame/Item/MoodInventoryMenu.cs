using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LHH.Menu;

public class MoodInventoryMenu : PrefabListMenu<MoodInventoryMenuItem, MoodItemInstance>
{
    public IMoodInventory _inventory;
    public RectTransform contentWalker;

    private float _optionHeight;

    private void Awake()
    {
        if(_inventory == null)
            _inventory = GetComponentInParent<MoodInventoryOld>();
        if(_inventory == null)
        {
            Debug.LogErrorFormat(this, "No inventory for {0}", this);
        }
        _optionHeight = optionPrefab.GetComponent<RectTransform>().rect.height;
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
        if (instance.itemName != null) instance.itemName.text = origin.itemData.GetName();
        if (instance.itemSecondary != null) instance.itemSecondary.text = origin.GetStatusText();
        //if(instance.itemIcon != null) instance.itemIcon.sprite = origin.itemData.algumacoisa;
    }

    public override void Reposition(Option option, int index, int length, bool justCreated)
    {
        contentWalker.anchoredPosition = new Vector3(contentWalker.anchoredPosition.x, index * _optionHeight);
    }

    protected override void Select(Option option)
    {
        option.currentOptionView.anim.SetTrigger("Select");
    }

    protected override void SetSelected(Option option, bool selected)
    {
        option.currentOptionView.anim.SetBool("Selected", selected);
    }

}
