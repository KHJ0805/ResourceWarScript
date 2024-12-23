using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConnectFailUi : BaseUi
{
    public Button endGameBtn;
    public Button reConnectBtn;
    public GameObject sessionConnectUi;

    protected override void Start()
    {
        base.Start();

        AddButtonListener(endGameBtn, OnEndGameBtnClicked);
        AddButtonListener(reConnectBtn, OnReConnectBtnClicked);
    }

    private void OnReConnectBtnClicked()
    {
        gameObject.SetActive(false);
        sessionConnectUi.SetActive(true);
        ReconnectToServer();
    }

    private void ReconnectToServer()
    {
        SessionNetworkManager.Instance.StartConnect("SessionIp", "SessionPort");
    }

    private void OnEndGameBtnClicked()
    {
        Debug.Log("게임 종료");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
