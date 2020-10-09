using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FocusControllerUI : MonoBehaviour
{
    public FocusController controller;

    [Space()]
    public PointsAsObjectsUI painPointsUI;
    public PointsAsObjectsUI availablePointsUI;

    [Space()]
    public FocusableUI focusableUIObjectPrefab;
    public RectTransform focusableObjectsContainer;
    public RectTransform focusableObjectSelectionUI;

    List<FocusableUI> _focusableUIList;

    // Start is called before the first frame update
    void Start()
    {
        if (controller == null)
            return;

        PrepareList();
    }

    void PrepareList()
    {
        if (_focusableUIList == null)
            _focusableUIList = new List<FocusableUI>(3);
        else return;

        foreach (var focusable in controller.focusableList)
        {
            var uiObj = Instantiate(focusableUIObjectPrefab, focusableObjectsContainer);
            uiObj.SetFocusable(focusable);
            uiObj.gameObject.SetActive(true);

            _focusableUIList.Add(uiObj);
        }
    }

    private void OnEnable()
    {
        if (controller == null)
        {
            enabled = false;
            return;
        }

        controller.OnAvailablePointsChanged += Controller_OnAvailablePointsChanged;
        controller.OnMaxPointsChanged += Controller_OnMaxPointsChanged;
        controller.OnPainPointsChanged += Controller_OnPainPointsChanged;
        controller.OnSelectedFocusableChanged += Controller_OnSelectedFocusableChanged;

        PrepareList();
        StartCoroutine(CheckSelectionOnFirstFrame());
    }

    IEnumerator CheckSelectionOnFirstFrame()
    {
        yield return new WaitForEndOfFrame();
        Controller_OnSelectedFocusableChanged(controller.SelectedFocusableIndex);
    }

    private void OnDisable()
    {
        if (controller == null)
            return;

        controller.OnAvailablePointsChanged -= Controller_OnAvailablePointsChanged;
        controller.OnMaxPointsChanged -= Controller_OnMaxPointsChanged;
        controller.OnPainPointsChanged -= Controller_OnPainPointsChanged;
        controller.OnSelectedFocusableChanged -= Controller_OnSelectedFocusableChanged;
    }

    private void Controller_OnSelectedFocusableChanged(int val)
    {
        focusableObjectSelectionUI.position = _focusableUIList[val].transform.position;
    }

    private void Controller_OnPainPointsChanged(int val)
    {
        painPointsUI.SetNPoints(val);
    }

    private void Controller_OnMaxPointsChanged(int val)
    {
        
    }

    private void Controller_OnAvailablePointsChanged(int val)
    {
        availablePointsUI.SetNPoints(val);
    }
}
