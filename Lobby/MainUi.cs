using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainUi : BaseUi
{
    [Header("UI Elements")]
    public Button autoEntranceBtn;
    public Button createRoomBtn;
    public Button secretRoomBtn;
    public Button tutorialBtn;
    public Button endBtn;

    public GameObject autoEntranceUi;
    public GameObject createRoomUI;
    public GameObject secretRoomUI;
    public GameObject tutorialUI;
    public GameObject endUi;

    protected override void Start()
    {
        base.Start();

        AddButtonListener(autoEntranceBtn, OnAutoEntranceBtnClicked);
        AddButtonListener(createRoomBtn, OnCreateRoomBtnClicked);
        AddButtonListener(secretRoomBtn, OnSecretRoomBtnClicked);
        AddButtonListener(tutorialBtn, OnTutorialBtnClicked);
        AddButtonListener(endBtn, OnEndBtnClicked);
    }

    private void OnAutoEntranceBtnClicked()
    {
        autoEntranceUi.SetActive(true);
    }

    private void OnEndBtnClicked()
    {
        endUi.SetActive(true);
    }

    private void OnTutorialBtnClicked()
    {
        //tutorialUI.SetActive(true);
        SessionNetworkManager.Instance.TestSceneGameServerConnect();
        UnityEngine.SceneManagement.SceneManager.LoadScene("MoveTestScene");
    }

    private void OnSecretRoomBtnClicked()
    {
        secretRoomUI.SetActive(true);
    }

    private void OnCreateRoomBtnClicked()
    {
        createRoomUI.SetActive(true);
    }
}
