using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SecretRoomUi : BaseUi
{
    [Header("UI Elements")]
    public Button secretRoomEnterBtn;
    public Button setOffSecretRoomBtn;
    public TMP_InputField gameCodeInputField;

    public GameObject RoomUi;

    protected override void Start()
    {
        base.Start();

        AddButtonListener(secretRoomEnterBtn, OnSecretRoomBtnClicked);
        AddButtonListener(setOffSecretRoomBtn, OnSetOffSecretRoomBtnClicked);
    }

    private void OnSetOffSecretRoomBtnClicked()
    {
        gameObject.SetActive(false);
    }

    private void OnSecretRoomBtnClicked()
    {
        // 荤汲规 积己 肺流
        string gameCode = gameCodeInputField.text;
        DataManager.Instance.GameCode = gameCode;
        SessionNetworkManager.Instance.SendJoinRoomReq(gameCode);
    }
}
