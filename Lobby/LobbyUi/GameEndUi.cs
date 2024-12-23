using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameEndUi : BaseUi
{
    [Header("UI Elements")]
    public Button closeGameEndUiBtn;
    public Button gameEndBtn;


    protected override void Start()
    {
        base.Start();

        AddButtonListener(closeGameEndUiBtn, OnCloseGameEndUiBtnClicked);
        AddButtonListener(gameEndBtn, OnGameEndBtnClicked);
    }

    private void OnGameEndBtnClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnCloseGameEndUiBtnClicked()
    {
        gameObject.SetActive(false);
    }
}
