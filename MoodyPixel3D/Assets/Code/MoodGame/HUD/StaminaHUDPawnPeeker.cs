using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class StaminaHUDPawnPeeker : MonoBehaviour, IMoodPawnPeeker
{
    public Image border;
    public Image fill;

    public Image carat;

    public void SetTarget(MoodPawn pawn)
    {
        pawn.OnChangeStamina += OnChangeStamina;
        OnChangeStamina(pawn);
    }
    
    public void UnsetTarget(MoodPawn pawn)
    {
        pawn.OnChangeStamina -= OnChangeStamina;
    }
    
    private void OnChangeStamina(MoodPawn pawn)
    {
        //float maxWidth = border.rectTransform.;
        //Vector2 size = fill.rectTransform.sizeDelta;
        //size.x = maxWidth * pawn.GetStaminaRatio();
        //Debug.LogFormat("Stamina depleted oh my god!! {0} {1} = {2} * {3}/{4} ({5}) rect is {6}", pawn, size.x, maxWidth, pawn.GetStamina(), pawn.GetMaxStamina(), pawn.GetStaminaRatio(), border.rectTransform.rect);
        fill.rectTransform.localScale = new Vector3(pawn.GetStaminaRatio(), 1f, 1f);
        SetNumberOfCarats(Mathf.RoundToInt(pawn.GetMaxStamina() - 1));
    }

    private void SetNumberOfCarats(int num)
    {
        Transform parent = carat.transform.parent;
        while(parent.childCount > num)
        {
            if(parent.childCount == 1)
            {
                carat.gameObject.SetActive(false);
                return;
            }
            else 
                Destroy(parent.GetChild(parent.childCount - 1).gameObject);
        }
        while(parent.childCount < num)
        {
            Instantiate(carat, carat.transform.position, carat.transform.rotation, parent);
        }
    }
}
