using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorManager : PersistentSingleton<CursorManager>
{
    [ReadOnly]
    [SerializeField]
    private bool _cursorLocked;

    private void Start()
    {
        LockCursor(true);
    }

    public void LockCursor(bool set)
    {
        _cursorLocked = set;
        Cursor.visible = !set;
        if (set) Cursor.lockState = CursorLockMode.Locked;
        else Cursor.lockState = CursorLockMode.None;
    }
}
