using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FocusableUI : MonoBehaviour
{
    FocusableObject _focusable;

    public Image iconImage;
    public PointsAsObjectsUI focusPointsUI;

    public void SetFocusable(FocusableObject focusable)
    {
        if (_focusable == focusable)
            return;

        if(_focusable != null)
        {
            _focusable.OnFocusChanged -= Focusable_OnFocusChanged;
        }

        _focusable = focusable;

        if (_focusable == null)
        {
            gameObject.SetActive(false);
            return;
        }

        if (iconImage != null)
            iconImage.sprite = _focusable.focusableIcon;

        _focusable.OnFocusChanged += Focusable_OnFocusChanged;
    }

    private void Focusable_OnFocusChanged(int newFocus)
    {
        if (focusPointsUI == null)
            return;

        focusPointsUI.SetNPoints(newFocus);
    }
}
