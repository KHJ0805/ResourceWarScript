using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialUi : BaseUi
{
    [Header("UI Elements")]
    public Button closeTutorialUiBtn;


    protected override void Start()
    {
        base.Start();

        AddButtonListener(closeTutorialUiBtn, OnCloseTutorialUiBtnClicked);
    }

    private void OnCloseTutorialUiBtnClicked()
    {
        gameObject.SetActive(false);
    }
}
