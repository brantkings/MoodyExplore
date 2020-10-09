using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FocusController : MonoBehaviour
{
    public delegate void NoParameterFocusControllerDelegate();
    public delegate void IntParameterFocusControllerDelegate(int val);

    public event IntParameterFocusControllerDelegate OnMaxPointsChanged;
    public event IntParameterFocusControllerDelegate OnAvailablePointsChanged;
    public event IntParameterFocusControllerDelegate OnPainPointsChanged;

    public event IntParameterFocusControllerDelegate OnSelectedFocusableChanged;

    public Focusable[] focusableList;

    [SerializeField]
    int _maxFocusPoints;

    int _availableFocusPoints;
    int _selectedFocusableIndex;

    int _currentPain;

    public int MaxPoints { get => _maxFocusPoints; }

    public int AvailablePoints { get => _availableFocusPoints; }

    public int CurrentPain { get => _currentPain; }

    public int MaxMinusPainPoints { get => _maxFocusPoints - _currentPain; }

    public int SelectedFocusableIndex { get => _selectedFocusableIndex; }

    // Start is called before the first frame update
    void Start()
    {
        _availableFocusPoints = MaxMinusPainPoints;
        RebalanceFocusableList();

        OnMaxPointsChanged?.Invoke(_maxFocusPoints);
        OnAvailablePointsChanged?.Invoke(_availableFocusPoints);
        OnPainPointsChanged?.Invoke(_currentPain);

        OnSelectedFocusableChanged?.Invoke(_selectedFocusableIndex);
    }

    public void RebalanceFocusableList()
    {
        int spentPoints = 0;

        foreach (var focusable in focusableList)
        {
            spentPoints += focusable.GetFocus();
            if(spentPoints > MaxMinusPainPoints)
            {
                focusable.TryAddFocus(MaxMinusPainPoints - spentPoints);
                spentPoints = MaxMinusPainPoints;
            }
        }

        _availableFocusPoints = MaxMinusPainPoints - spentPoints;

        OnAvailablePointsChanged?.Invoke(_availableFocusPoints);
    }

    public void SetPain(int pain)
    {
        pain = Mathf.Clamp(pain, 0, _maxFocusPoints);

        if (pain == _currentPain)
            return;

        _currentPain = pain;
        OnPainPointsChanged?.Invoke(_currentPain);
        RebalanceFocusableList();
    }

    [ContextMenu("Add One Pain")]
    public void AddOnePain()
    {
        AddPain(1);
    }

    [ContextMenu("Remove One Pain")]
    public void RemoveOnePain()
    {
        AddPain(-1);
    }

    public void AddPain(int value)
    {
        SetPain(_currentPain + value);
    }

    // Update is called once per frame
    void Update()
    {
        if (focusableList.Length <= 0)
            return;

        if (Input.mouseScrollDelta.y < 0)
        {
            _selectedFocusableIndex += 1;

            while (_selectedFocusableIndex >= focusableList.Length)
            {
                _selectedFocusableIndex = _selectedFocusableIndex - focusableList.Length;
            }

            OnSelectedFocusableChanged?.Invoke(_selectedFocusableIndex);
        }
        else if (Input.mouseScrollDelta.y > 0)
        {
            _selectedFocusableIndex -= 1;

            while (_selectedFocusableIndex < 0)
            {
                _selectedFocusableIndex = focusableList.Length + _selectedFocusableIndex;
            }

            OnSelectedFocusableChanged?.Invoke(_selectedFocusableIndex);
        }

        if (Input.GetKeyDown(KeyCode.E) && _availableFocusPoints > 0)
        {
            Focusable focusable = focusableList[_selectedFocusableIndex];

            if (focusable.TryAddOneFocus())
            {
                _availableFocusPoints -= 1;
                OnAvailablePointsChanged?.Invoke(_availableFocusPoints);
            }
        }
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            Focusable focusable = focusableList[_selectedFocusableIndex];

            if (focusable.TryRemoveOneFocus()) 
            { 
                _availableFocusPoints += 1;
                OnAvailablePointsChanged?.Invoke(_availableFocusPoints);

                Debug.Assert(_availableFocusPoints <= _maxFocusPoints, "Somehow it was possible to get more points out of the Focusables than the total points should be", this.gameObject);
            }
        }
    }
}
