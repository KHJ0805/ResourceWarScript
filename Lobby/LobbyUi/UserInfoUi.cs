using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UserInfoUi : BaseUi
{
    [Header("UI Elements")]
    public TextMeshProUGUI userNickNameText;

    protected override void Start()
    {
        base.Start();

        var currentPlayer = DataManager.Instance.CurrentPlayer;
        if (currentPlayer != null)
        {
            userNickNameText.text = currentPlayer.PlayerName;
        }
        else
        {
            userNickNameText.text = "Unknown Player";
        }
    }

}
