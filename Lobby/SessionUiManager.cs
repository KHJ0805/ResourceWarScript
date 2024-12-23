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
        SceneManager.sceneLoaded += OnSceneLoaded; // 씬 로드 후 UI 할당 처리
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

    //회원 가입
    public void SignUpSuccess()
    {
        ShowWarning("회원 가입에 성공했습니다.");
        accountUi.SetActive(false);
        logInUi.SetActive(true);
    }

    public void SignUpFail()
    {
        ShowWarning("일시적 오류로 인해 회원 가입에 실패했습니다.");
    }

    //로그인
    public void SignInSuccess()
    {
        Debug.Log("서버 검증 성공, Ui매니저로 옴");
        ShowWarning("로그인 성공! 로비로 진입합니다.");
        UnityEngine.SceneManagement.SceneManager.LoadScene("LobbyScene");
    }

    public void SignInFail()
    {
        ShowWarning("일시적 오류로 인해 로그인에 실패했습니다.");
    }

    private void ShowWarning(string message)
    {
        warningUiText.text = message;
        warningUi.SetActive(true);
    }

    //서버에서 방제작 허가 받음 서버 연결하는동안 팻말 띄우기
    public void GameCodeRegistRoom(string gameCode)
    {
        DataManager.Instance.SetGameRoomData(gameCode);
        CreateRoomUi.SetActive(false);
        LobbyScenePopUpUi("방을 생성 중입니다");
    }

    public void GameCodeEnterRoom(string gameCode)
    {
        DataManager.Instance.SetGameRoomData(gameCode);
        CreateRoomUi.SetActive(false);
        LobbyScenePopUpUi("방에 참가 중입니다");
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

    //서버 연결 끝나고 연결
    public void CreateRoomSuccess()
    {
        PopUpUi.SetActive(false);
        RoomUi.SetActive(true);
        DataManager.Instance.OnRoomUiActivated();
    }

    //게임 시작, loadingScene으로 이동합니다.
    public void GameStartSuccess()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("LoadingScene");
    }
}
