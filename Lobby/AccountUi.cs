using System;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AccountUi : BaseUi
{
    [Header("UI Elements")]
    public TMP_InputField idInputField;
    public TMP_InputField passwordInputField;
    public TMP_InputField nicknameInputField;
    public Button signUpButton;
    public Button cancelButton;
    public GameObject warningUi;
    public TextMeshProUGUI warningUiText;

    [Header("Validation Settings")]
    // 아이디 : 영어 대소문자,숫자로만 6~36  , 비밀번호 : 영어 대소문자 최소 1개. 숫자 최소 1개, 특수문자 최소 1개, 6~36 닉네임 영문자,숫자,한글 특문은 _만 2~16
    private static readonly Regex IdRegex = new Regex("^[a-zA-Z0-9]{6,36}$");
    private static readonly Regex PasswordRegex = new Regex("^(?=.*[a-zA-Z])(?=.*\\d)(?=.*[!@#$%^&])[a-zA-Z0-9!@#$%^&*]{6,36}$");
    private static readonly Regex NicknameRegex = new Regex("^[a-zA-Z0-9ㄱ-ㅎ가-힣_]{2,16}$");

    protected override void Start()
    {
        base.Start();

        AddButtonListener(signUpButton, OnSignUpButtonClicked);
        AddButtonListener(cancelButton, OnCancelButtonClicked);
    }

    // 클라이언트 내부에서 유효성 검사
    private void OnSignUpButtonClicked()
    {
        string id = idInputField.text;
        string password = passwordInputField.text;
        string nickname = nicknameInputField.text;

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

        if (!ValidateNickname(nickname))
        {
            ShowWarning("닉네임은 영어, 숫자, 한글, '_'만 포함하며, 2~16자 사이여야 합니다.");
            return;
        }
        Debug.Log($"{nickname},{password},{id} 보냄");
        C2SSignUpReq(nickname, password, id);
    }

    // 서버로 회원가입 요청
    private void C2SSignUpReq(string nickname, string password, string id)
    {
        SessionNetworkManager.Instance.SendSignUpRequest(nickname, password, id);
    }

    private void OnCancelButtonClicked()
    {
        gameObject.SetActive(false);
    }

    private bool ValidateId(string id)
    {
        return !string.IsNullOrEmpty(id) && IdRegex.IsMatch(id);
    }

    private bool ValidatePassword(string password)
    {
        return !string.IsNullOrEmpty(password) && PasswordRegex.IsMatch(password);
    }

    private bool ValidateNickname(string nickname)
    {
        return !string.IsNullOrEmpty(nickname) && NicknameRegex.IsMatch(nickname);
    }

    private void ShowWarning(string message)
    {
        warningUiText.text = message;
        warningUi.SetActive(true);
    }
}
