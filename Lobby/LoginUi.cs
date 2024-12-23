using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginUi : BaseUi
{
    [Header("UI Elements")]
    public TMP_InputField idInputField;
    public TMP_InputField passwordInputField;
    public Button loginButton;
    public Button createAccountButton;
    public Button quitButton;
    public GameObject createAccountUI;
    public GameObject warningUi;
    public TextMeshProUGUI warningUiText;

    [Header("Validation Settings")]
    private static readonly Regex IdRegex = new Regex("^[a-zA-Z0-9]{6,36}$");
    private static readonly Regex PasswordRegex = new Regex("^(?=.*[a-zA-Z])(?=.*\\d)(?=.*[!@#$%^&])[a-zA-Z0-9!@#$%^&*]{6,36}$");

    protected override void Start()
    {
        base.Start();

        AddButtonListener(loginButton, OnLoginButtonClicked);
        AddButtonListener(createAccountButton, OnCreateAccountButtonClicked);
        AddButtonListener(quitButton, OnQuitButtonClicked);
    }

    private void OnLoginButtonClicked()
    {
        string id = idInputField.text;
        string password = passwordInputField.text;

        if (!ValidateId(id))
        {
            ShowWarning("ID는 영어와 숫자로만 이루어져야 하며, 6~36자 사이여야 합니다.");
            return;
        }

        if (!ValidatePassword(password))
        {
            ShowWarning("비밀번호는 영어, 숫자, 특수문자를 포함하여 6~36자 사이여야 합니다.");
            return;
        }
        Debug.Log("유효성 검사 성공, 세션네트워크 매니저에 요청 보냄");
        SessionNetworkManager.Instance.SendSignInRequest(id, password);
    }

    //로그인 요청
    private void C2SSignInReq(string id, string password)
    {
        // 데이터 패킹
        var signUpData = new
        {
            id = id,
            password = password,
        };
        // 서버 데이터 전송
    }

    private void S2CSignInRes(int signInResultCode, string token, int expirationTime)
    {
        //로그인 허가, 토큰에서 유저 정보 추출, 데이터매니저에 유저정보 전달
        //로비씬으로 이동
        //LoadLobbyScene();
    }

    private bool ValidateId(string id)
    {
        return !string.IsNullOrEmpty(id) && IdRegex.IsMatch(id);
    }

    private bool ValidatePassword(string password)
    {
        return !string.IsNullOrEmpty(password) && PasswordRegex.IsMatch(password);
    }

    private void OnCreateAccountButtonClicked()
    {
        if (createAccountUI != null)
        {
            createAccountUI.SetActive(true);
        }
    }

    private void ShowWarning(string message)
    {
        warningUiText.text = message;
        warningUi.SetActive(true);
    }

    private void OnQuitButtonClicked()
    {
        Debug.Log("게임 종료");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void LoadLobbyScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("LobbyScene");
    }

}
