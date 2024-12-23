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
    // ¾ÆÀÌµð : ¿µ¾î ´ë¼Ò¹®ÀÚ,¼ýÀÚ·Î¸¸ 6~36  , ºñ¹Ð¹øÈ£ : ¿µ¾î ´ë¼Ò¹®ÀÚ ÃÖ¼Ò 1°³. ¼ýÀÚ ÃÖ¼Ò 1°³, Æ¯¼ö¹®ÀÚ ÃÖ¼Ò 1°³, 6~36 ´Ð³×ÀÓ ¿µ¹®ÀÚ,¼ýÀÚ,ÇÑ±Û Æ¯¹®Àº _¸¸ 2~16
    private static readonly Regex IdRegex = new Regex("^[a-zA-Z0-9]{6,36}$");
    private static readonly Regex PasswordRegex = new Regex("^(?=.*[a-zA-Z])(?=.*\\d)(?=.*[!@#$%^&])[a-zA-Z0-9!@#$%^&*]{6,36}$");
    private static readonly Regex NicknameRegex = new Regex("^[a-zA-Z0-9¤¡-¤¾°¡-ÆR_]{2,16}$");

    protected override void Start()
    {
        base.Start();

        AddButtonListener(signUpButton, OnSignUpButtonClicked);
        AddButtonListener(cancelButton, OnCancelButtonClicked);
    }

    // Å¬¶óÀÌ¾ðÆ® ³»ºÎ¿¡¼­ À¯È¿¼º °Ë»ç
    private void OnSignUpButtonClicked()
    {
        string id = idInputField.text;
        string password = passwordInputField.text;
        string nickname = nicknameInputField.text;

        if (!ValidateId(id))
        {
            ShowWarning("ID´Â ¿µ¾î¿Í ¼ýÀÚ·Î¸¸ ÀÌ·ç¾îÁ®¾ß ÇÏ¸ç, 6~36ÀÚ »çÀÌ¿©¾ß ÇÕ´Ï´Ù.");
            return;
        }

        if (!ValidatePassword(password))
        {
            ShowWarning("ºñ¹Ð¹øÈ£´Â ¿µ¾î, ¼ýÀÚ, Æ¯¼ö¹®ÀÚ¸¦ Æ÷ÇÔÇÏ¿© 6~36ÀÚ »çÀÌ¿©¾ß ÇÕ´Ï´Ù.");
            return;
        }

        if (!ValidateNickname(nickname))
        {
            ShowWarning("´Ð³×ÀÓÀº ¿µ¾î, ¼ýÀÚ, ÇÑ±Û, '_'¸¸ Æ÷ÇÔÇÏ¸ç, 2~16ÀÚ »çÀÌ¿©¾ß ÇÕ´Ï´Ù.");
            return;
        }
        Debug.Log($"{nickname},{password},{id} º¸³¿");
        C2SSignUpReq(nickname, password, id);
    }

    // ¼­¹ö·Î È¸¿ø°¡ÀÔ ¿äÃ»
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
