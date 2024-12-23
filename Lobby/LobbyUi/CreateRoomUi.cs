using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreateRoomUi : BaseUi
{
    [Header("UI Elements")]
    public Button closeCreateRoomBtn;
    public Button makeRoomBtn;

    public GameObject RoomUi;

    [Header("variable")]
    private bool isPrivate = false;

    protected override void Start()
    {
        base.Start();

        AddButtonListener(closeCreateRoomBtn, OnCloseCreateRoomBtnClicked);
        AddButtonListener(makeRoomBtn, OnMakeRoomBtnClicked);
    }

    private void OnMakeRoomBtnClicked()
    {
        // 서버에 공개방/사설방 여부 통보 isPrivate = 1 일 경우 사설, 0의 경우 공개
        SessionNetworkManager.Instance.SendCreateRoomReq(isPrivate);
    }

    private void OnCloseCreateRoomBtnClicked()
    {
        gameObject.SetActive(false);
    }

    public void OnPublicToggleClicked()
    {
        isPrivate = false;
        Debug.Log("공개방으로 설정");
    }

    public void OnPrivateToggleClicked()
    {
        isPrivate = true;
        Debug.Log("비공개방으로 설정");
    }
}
