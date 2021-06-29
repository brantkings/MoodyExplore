using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class FocusableObject : MonoBehaviour 
{
    public delegate void FocusChangeDelegate(int newFocus);
    public event FocusChangeDelegate OnFocusChanged;

    public Sprite focusableIcon;
    public int maxFocus;

    int _currentFocus;

    //Tries to set the level to the exact value, returns the actual focus that was set. The return value might be different due to level capping/greater than 0 etc.
    public int TrySetFocus(int focus)
    {
        int previousFocus = _currentFocus;
        _currentFocus = focus;

        if (_currentFocus > maxFocus)
            _currentFocus = maxFocus;
        else if (_currentFocus < 0)
            _currentFocus = 0;

        if(previousFocus != _currentFocus)
        {
            ApplyFocus(_currentFocus);
            OnFocusChanged?.Invoke(_currentFocus);
        }

        return _currentFocus;
    }

    public int GetFocus() 
    {
        return _currentFocus;
    }

    //Tries add exact value (value can be negative), returns the actual focus that was set. The return value might be different from the expected due to level capping/greater than 0 etc.
    public bool TryAddOneFocus()
    {
        if(_currentFocus < maxFocus)
        {
            TrySetFocus(_currentFocus + 1);
            return true;
        }
        return false;
    }

    public bool TryRemoveOneFocus()
    {
        if (_currentFocus > 0)
        {
            TrySetFocus(_currentFocus - 1);
            return true;
        }
        return false;
    }

    public int TryAddFocus(int ammount)
    {
        int beforeSetting = _currentFocus;
        return (TrySetFocus(_currentFocus + ammount) - beforeSetting);
    }

    protected abstract void ApplyFocus(int focus);
}
