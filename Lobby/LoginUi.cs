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
            ShowWarning("ID�� ����� ���ڷθ� �̷������ �ϸ�, 6~36�� ���̿��� �մϴ�.");
            return;
        }

        if (!ValidatePassword(password))
        {
            ShowWarning("��й�ȣ�� ����, ����, Ư�����ڸ� �����Ͽ� 6~36�� ���̿��� �մϴ�.");
            return;
        }
        Debug.Log("��ȿ�� �˻� ����, ���ǳ�Ʈ��ũ �Ŵ����� ��û ����");
        SessionNetworkManager.Instance.SendSignInRequest(id, password);
    }

    //�α��� ��û
    private void C2SSignInReq(string id, string password)
    {
        // ������ ��ŷ
        var signUpData = new
        {
            id = id,
            password = password,
        };
        // ���� ������ ����
    }

    private void S2CSignInRes(int signInResultCode, string token, int expirationTime)
    {
        //�α��� �㰡, ��ū���� ���� ���� ����, �����͸Ŵ����� �������� ����
        //�κ������ �̵�
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
        Debug.Log("���� ����");
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
