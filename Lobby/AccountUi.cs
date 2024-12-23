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
    // ���̵� : ���� ��ҹ���,���ڷθ� 6~36  , ��й�ȣ : ���� ��ҹ��� �ּ� 1��. ���� �ּ� 1��, Ư������ �ּ� 1��, 6~36 �г��� ������,����,�ѱ� Ư���� _�� 2~16
    private static readonly Regex IdRegex = new Regex("^[a-zA-Z0-9]{6,36}$");
    private static readonly Regex PasswordRegex = new Regex("^(?=.*[a-zA-Z])(?=.*\\d)(?=.*[!@#$%^&])[a-zA-Z0-9!@#$%^&*]{6,36}$");
    private static readonly Regex NicknameRegex = new Regex("^[a-zA-Z0-9��-����-�R_]{2,16}$");

    protected override void Start()
    {
        base.Start();

        AddButtonListener(signUpButton, OnSignUpButtonClicked);
        AddButtonListener(cancelButton, OnCancelButtonClicked);
    }

    // Ŭ���̾�Ʈ ���ο��� ��ȿ�� �˻�
    private void OnSignUpButtonClicked()
    {
        string id = idInputField.text;
        string password = passwordInputField.text;
        string nickname = nicknameInputField.text;

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

        if (!ValidateNickname(nickname))
        {
            ShowWarning("�г����� ����, ����, �ѱ�, '_'�� �����ϸ�, 2~16�� ���̿��� �մϴ�.");
            return;
        }
        Debug.Log($"{nickname},{password},{id} ����");
        C2SSignUpReq(nickname, password, id);
    }

    // ������ ȸ������ ��û
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
