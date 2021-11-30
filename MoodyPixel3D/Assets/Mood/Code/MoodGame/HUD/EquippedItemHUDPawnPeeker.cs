using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquippedItemHUDPawnPeeker : MonoBehaviour, IMoodPawnPeeker
{
    public Image icon;
    public RectTransform iconParent;

    public Text itemText;
    public RectTransform textParent;

    public Text itemQuantityText;
    public RectTransform itemQuantityTextParent;

    public RectTransform allParent;

    public UnityEngine.Events.UnityEvent onUseItem;

    public MoodItemCategory itemCategory;

    private void Awake()
    {
        HideItem();
    }

    public void SetTarget(MoodPawn pawn)
    {
        if(pawn.Inventory != null)
        {
            pawn.Inventory.OnEquipped += OnEquipped;
            pawn.Inventory.OnUnequipped += OnUnequipped;
        }
    }

    public void UnsetTarget(MoodPawn pawn)
    {
        if (pawn.Inventory != null)
        {
            pawn.Inventory.OnEquipped -= OnEquipped;
            pawn.Inventory.OnUnequipped -= OnUnequipped;
        }
    }

    private void OnEquipped(MoodItemInstance item)
    {
        SetLookingItem(item);
    }
    private void OnUnequipped(MoodItemInstance item)
    {
        UnsetLookingItem(item);
        HideItem();
    }


    private void SetLookingItem(MoodItemInstance item)
    {
        ShowItem(item);
        item.OnUse += OnUseItem;
        item.OnDestroy += OnDestroyItem;
    }

    private void UnsetLookingItem(MoodItemInstance item)
    {
        item.OnUse -= OnUseItem;
        item.OnDestroy -= OnDestroyItem;
    }

    private void OnDestroyItem(MoodItemInstance instance)
    {
        HideItem();
        UnsetLookingItem(instance);
    }

    private void OnUseItem(MoodPawn pawn, MoodItemInstance instance)
    {
        ShowItem(instance);
        onUseItem.Invoke();
    }

    private void ShowItem(MoodItemInstance item)
    {
        MoodItem data = item.itemData;
        if (data != null)
        {
            allParent.gameObject.SetActive(true);

            Sprite itemIcon = data.GetIcon();
            icon.sprite = itemIcon;
            iconParent.gameObject.SetActive(itemIcon != null);

            itemText.text = data.GetName();
            textParent.gameObject.SetActive(!string.IsNullOrWhiteSpace(itemText.text));

            itemQuantityText.text = data.GetItemStatusDescription(item.properties, false);
            itemQuantityTextParent.gameObject.SetActive(!string.IsNullOrWhiteSpace(itemText.text));
        }
        else
        {

            allParent.gameObject.SetActive(false);
        }

    }

    private void HideItem()
    {
        allParent.gameObject.SetActive(false);
    }

}
