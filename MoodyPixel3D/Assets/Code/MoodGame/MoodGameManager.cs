using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MoodGameManager : PersistentSingleton<MoodGameManager>
{
    [SerializeField]
    private LayerMask _pawnlayer;

    public LayerMask GetPawnBodyLayer()
    {
        return _pawnlayer;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetGame();
        }
    }

    private static void ResetGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
