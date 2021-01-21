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

    private Focusable[] _focusableList;

    [SerializeField]
    private Focusable[] _initialSetup;

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
        if (_focusableList == null) InitFocusables();
        _availableFocusPoints = MaxMinusPainPoints;
        RebalanceFocusableList();

        OnMaxPointsChanged?.Invoke(_maxFocusPoints);
        OnAvailablePointsChanged?.Invoke(_availableFocusPoints);
        OnPainPointsChanged?.Invoke(_currentPain);

        OnSelectedFocusableChanged?.Invoke(_selectedFocusableIndex);

        GetInitialSetup();
    }

    private void GetInitialSetup()
    {
        if(_initialSetup != null)
        {
            foreach(var sensor in _initialSetup)
            {
                AddFocus(sensor, 1);
            }
        }
    }

    private void InitFocusables()
    {
        _focusableList = gameObject.GetComponentsInChildren<Focusable>();
    }

    public IEnumerable<Focusable> Focusables
    {
        get
        {
            if (_focusableList == null) InitFocusables();
            foreach (Focusable f in _focusableList) yield return f;
        }
    }

    public void RebalanceFocusableList()
    {
        int spentPoints = 0;

        foreach (var focusable in _focusableList)
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

    [LHH.Unity.Button]
    public void AddOnePain()
    {
        AddPain(1);
    }

    [LHH.Unity.Button]
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
        if (_focusableList.Length <= 0)
            return;

        if (Input.mouseScrollDelta.y < 0)
        {
            _selectedFocusableIndex += 1;

            while (_selectedFocusableIndex >= _focusableList.Length)
            {
                _selectedFocusableIndex = _selectedFocusableIndex - _focusableList.Length;
            }

            OnSelectedFocusableChanged?.Invoke(_selectedFocusableIndex);
        }
        else if (Input.mouseScrollDelta.y > 0)
        {
            _selectedFocusableIndex -= 1;

            while (_selectedFocusableIndex < 0)
            {
                _selectedFocusableIndex = _focusableList.Length + _selectedFocusableIndex;
            }

            OnSelectedFocusableChanged?.Invoke(_selectedFocusableIndex);
        }

        if (Input.GetKeyDown(KeyCode.E) && _availableFocusPoints > 0)
        {
            Focusable focusable = _focusableList[_selectedFocusableIndex];

            AddFocus(focusable, 1);
        }
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            Focusable focusable = _focusableList[_selectedFocusableIndex];

            if(AddFocus(focusable, -1))
            {
                Debug.Assert(_availableFocusPoints <= _maxFocusPoints, "Somehow it was possible to get more points out of the Focusables than the total points should be", this.gameObject);
            }
        }
    }

    private bool AddFocus(Focusable focusable, int amount)
    {
        if(focusable != null && focusable.TryAddFocus(amount) != 0)
        {
            _availableFocusPoints = Mathf.Clamp(_availableFocusPoints - amount, 0, MaxPoints);
            OnAvailablePointsChanged?.Invoke(_availableFocusPoints);
            return true;
        }
        return false;
    }
}
