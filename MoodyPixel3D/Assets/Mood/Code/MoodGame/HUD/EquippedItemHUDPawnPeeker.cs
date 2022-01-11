using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

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
            foreach (var item in pawn.Inventory.GetEquippedItems()) OnEquipped(item);
        }
    }

    public void UnsetTarget(MoodPawn pawn)
    {
        if (pawn.Inventory != null)
        {
            pawn.Inventory.OnEquipped -= OnEquipped;
            pawn.Inventory.OnUnequipped -= OnUnequipped;
            OnEquipped(null);
        }
    }

    private bool CanShow(MoodItemInstance item)
    {
        return itemCategory == null || item.itemData.category == itemCategory;
    }

    private void OnEquipped(MoodItemInstance item)
    {
        Debug.LogFormat("[PAWN PEEKER ITEM] {2} found item {0}. Can show? {1}", item, CanShow(item), this);
        if (item != null && CanShow(item))
        {
            SetLookingItem(item);
            ShowItem(item);
        }
    }
    private void OnUnequipped(MoodItemInstance item)
    {
        if(item != null && CanShow(item))
        {
            UnsetLookingItem(item);
            HideItem();
        }
    }


    private void SetLookingItem(MoodItemInstance item)
    {
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
        Debug.LogFormat("Show item {0}", item);
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
        Debug.LogFormat("Hide item!");  
        allParent.gameObject.SetActive(false);
    }

}
