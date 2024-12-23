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
        // ������ ������/�缳�� ���� �뺸 isPrivate = 1 �� ��� �缳, 0�� ��� ����
        SessionNetworkManager.Instance.SendCreateRoomReq(isPrivate);
    }

    private void OnCloseCreateRoomBtnClicked()
    {
        gameObject.SetActive(false);
    }

    public void OnPublicToggleClicked()
    {
        isPrivate = false;
        Debug.Log("���������� ����");
    }

    public void OnPrivateToggleClicked()
    {
        isPrivate = true;
        Debug.Log("����������� ����");
    }
}
