using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SessionUiManager : Singleton<SessionUiManager>
{
    [SerializeField] private GameObject warningUi;
    [SerializeField] private TextMeshProUGUI warningUiText;
    [SerializeField] private GameObject logInUi;
    [SerializeField] private GameObject accountUi;

    [Header ("LobbySceneUi")]
    public GameObject LobbyUi;
    public GameObject CreateRoomUi;
    public GameObject RoomUi;
    public GameObject PopUpUi;
    public TextMeshProUGUI PopUpTxt;
    protected override void Awake()
    {
        base.Awake();
        SceneManager.sceneLoaded += OnSceneLoaded; // �� �ε� �� UI �Ҵ� ó��
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "LobbyScene")
        {
            LobbyUi = GameObject.Find("LobbyUi");
            if (LobbyUi != null)
            {
                CreateRoomUi = LobbyUi.transform.Find("CreateRoomUi").gameObject;
                RoomUi = LobbyUi.transform.Find("RoomUi").gameObject;
                PopUpUi = LobbyUi.transform.Find("PopUpUi").gameObject;
                if (PopUpUi != null)
                {
                    PopUpTxt = PopUpUi.GetComponentInChildren<TextMeshProUGUI>();
                }
            }
        }
    }

    //ȸ�� ����
    public void SignUpSuccess()
    {
        ShowWarning("ȸ�� ���Կ� �����߽��ϴ�.");
        accountUi.SetActive(false);
        logInUi.SetActive(true);
    }

    public void SignUpFail()
    {
        ShowWarning("�Ͻ��� ������ ���� ȸ�� ���Կ� �����߽��ϴ�.");
    }

    //�α���
    public void SignInSuccess()
    {
        Debug.Log("���� ���� ����, Ui�Ŵ����� ��");
        ShowWarning("�α��� ����! �κ�� �����մϴ�.");
        UnityEngine.SceneManagement.SceneManager.LoadScene("LobbyScene");
    }

    public void SignInFail()
    {
        ShowWarning("�Ͻ��� ������ ���� �α��ο� �����߽��ϴ�.");
    }

    private void ShowWarning(string message)
    {
        warningUiText.text = message;
        warningUi.SetActive(true);
    }

    //�������� ������ �㰡 ���� ���� �����ϴµ��� �ָ� ����
    public void GameCodeRegistRoom(string gameCode)
    {
        DataManager.Instance.SetGameRoomData(gameCode);
        CreateRoomUi.SetActive(false);
        LobbyScenePopUpUi("���� ���� ���Դϴ�");
    }

    public void GameCodeEnterRoom(string gameCode)
    {
        DataManager.Instance.SetGameRoomData(gameCode);
        CreateRoomUi.SetActive(false);
        LobbyScenePopUpUi("�濡 ���� ���Դϴ�");
    }

    public void LobbyScenePopUpUi(string message)
    {
        PopUpTxt.text = message;
        PopUpUi.SetActive(true);
    }
    public void LobbyScenePopUpUiClose()
    {
        PopUpUi.SetActive(false);
    }

    //���� ���� ������ ����
    public void CreateRoomSuccess()
    {
        PopUpUi.SetActive(false);
        RoomUi.SetActive(true);
        DataManager.Instance.OnRoomUiActivated();
    }

    //���� ����, loadingScene���� �̵��մϴ�.
    public void GameStartSuccess()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("LoadingScene");
    }
}
