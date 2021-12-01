using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoodInventoryMenuItem : MonoBehaviour
{
    public Animator anim;
    public Text itemName;
    public Text itemSecondary;
    public Image itemIcon;

    [Space()]
    public string triggerUse = "Select";
    public string boolUsing = "Using";


    private void OnDisable()
    {
        FeedbackUsingItem(false);
    }

    public void FeedbackUse()
    {
        anim.SetTrigger(triggerUse);
    }

    public void FeedbackNegativeUse()
    {
          
    }

    public void FeedbackUsingItem(bool set)
    {
        anim.SetBool(boolUsing, set);
    }
}
