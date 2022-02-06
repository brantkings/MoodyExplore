using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class StaminaHUDPawnPeeker : MonoBehaviour, IMoodPawnPeeker
{
    public Image border;
    public Image fill;

    public RectTransform carat;


    float width = 70f;

    private void Awake()
    {
        width = border.rectTransform.rect.width;
    }

    public void SetTarget(MoodPawn pawn)
    {
        pawn.OnChangeStamina += OnChangeStamina;
        OnChangeStamina(pawn, 0f, pawn.GetStamina());
    }
    
    public void UnsetTarget(MoodPawn pawn)
    {
        pawn.OnChangeStamina -= OnChangeStamina;
    }
    
    private void OnChangeStamina(MoodPawn pawn, in float oldValue, in float newValue)
    {
        fill.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, pawn.GetStaminaRatio() * width);
        SetNumberOfCarats(Mathf.RoundToInt(pawn.GetMaxStamina()));
    }

    private void SetNumberOfCarats(int num)
    {
        Transform parent = carat.transform.parent;
        //Debug.LogFormat("Starting loop {0}, have {1}", num, parent.childCount);
        int count = parent.childCount;
        while (count > num)
        {
            if(count == 1)
            {
                //Debug.LogFormat("Not doing anything to {0}, have {1}", num, parent.childCount);
                carat.gameObject.SetActive(false);
                return;
            }
            else
            {
                //Debug.LogFormat("Destroying to {0}, have {1}", num, parent.childCount);
                Destroy(parent.GetChild(--count).gameObject);
                
            }
            //i++;
            //Debug.LogFormat("Infinite loop? {0}, {1} {2}", this, i, parent.childCount);
            //if (i > 20) return;
        }
        while(parent.childCount < num)
        {
            Instantiate(carat, carat.transform.position, carat.transform.rotation, parent);
        }
    }
}
