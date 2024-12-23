using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomUi : BaseUi
{
    public Button readyButton;
    public Button leaveRoomButton;
    public Button switchToTeam1Button;
    public Button switchToTeam2Button;
    public Button switchToTeam0Button;
    public Button gameStartButton;

    public TextMeshProUGUI[] team1Slots;
    public TextMeshProUGUI[] team2Slots;
    public TextMeshProUGUI[] team0Slots;

    // �� �ڵ�
    [SerializeField] private TextMeshProUGUI gameCodeText;

    private const int maxPlayersPer0Team = 4;
    private const int maxPlayersPerCompetitiveTeam = 2;
    private const int maxTotalPlayers = 4;

    //���� ���� ����
    private Dictionary<string, bool> playerReadyStates = new Dictionary<string, bool>();

    protected override void Start()
    {
        base.Start();

        gameCodeText.text = DataManager.Instance.GameCode;

        //gameStartButton.gameObject.SetActive(false);

        readyButton.onClick.AddListener(OnGameReady);
        leaveRoomButton.onClick.AddListener(OnLeaveRoom);
        switchToTeam1Button.onClick.AddListener(() => SwitchTeam(1));
        switchToTeam2Button.onClick.AddListener(() => SwitchTeam(2));
        switchToTeam0Button.onClick.AddListener(() => SwitchTeam(0));
        gameStartButton.onClick.AddListener(OnGameStart);
    }

    private void OnGameReady()
    {
        bool currentReadyState = DataManager.Instance.GetPlayerReadyState(DataManager.Instance.CurrentPlayer.PlayerName);
        GameServerManager.Instance.SendPlayerIsReadyChangeReq(currentReadyState);
    }

    private void OnGameStart()
    {
        SendGameStartReq();
    }

    private void SendGameStartReq()
    {
        if (AreAllPlayersReady() && DataManager.Instance.IsTeamBalanced())
        {
            Debug.Log("���� ����!");
            GameServerManager.Instance.GameStartReq();
        }
        else if (AreAllPlayersReady() && !DataManager.Instance.IsTeamBalanced())
        {
            Debug.Log("2:2 �� ������ �ʿ��մϴ�.");
            GameServerManager.Instance.GameStartReq();
        }
    }

    private void OnLeaveRoom()
    {
        // �� �ڵ� �ʱ�ȭ
        gameCodeText.text = "";

        SessionUiManager.Instance.LobbyScenePopUpUi("���� ��...");

        GameServerManager.Instance.SendQuitRoomReq();
    }

    private void SwitchTeam(int targetTeamNumber)
    {
        //���� ������ �̵��Ҷ�
        if (DataManager.Instance.CurrentPlayer.TeamIndex == targetTeamNumber)
        {
            Debug.Log("�̹� ������ ���� �ֽ��ϴ�.");
            return;
        }
        //���� ���¿��� �̵��Ҷ�
        bool isReady = DataManager.Instance.GetPlayerReadyState(DataManager.Instance.CurrentPlayer.PlayerName);
        if (isReady)
        {
            Debug.Log("���� ���¿����� ���� ������ �� �����ϴ�.");
            return;
        }
        DataManager.Instance.RequestTeamChange(targetTeamNumber);
    }

    public void UpdateTeamUI(List<string> team1Nicknames, List<string> team2Nicknames, List<string> team0Nicknames)
    {
        for (int i = 0; i < team1Slots.Length; i++)
            team1Slots[i].text = i < team1Nicknames.Count ? team1Nicknames[i] : "";

        for (int i = 0; i < team2Slots.Length; i++)
            team2Slots[i].text = i < team2Nicknames.Count ? team2Nicknames[i] : "";

        for (int i = 0; i < team0Slots.Length; i++)
            team0Slots[i].text = i < team0Nicknames.Count ? team0Nicknames[i] : "";

        UpdateGameStartButtonState();
    }

    private void UpdateGameStartButtonState()
    {
        bool allReady = AreAllPlayersReady();
        bool balancedTeams = DataManager.Instance.IsTeamBalanced();
        //if(allReady && balancedTeams)
        //{
        //    gameStartButton.gameObject.SetActive(true);
        //}
        //else { gameObject.SetActive(false); }
        
    }

    public void UpdateReadyStateUI(string playerNickname, bool isReady)
    {
        // �г��ӿ� �ش��ϴ� �ؽ�Ʈ �θ� ������Ʈ�� ���� ����
        foreach (var slot in team1Slots)
        {
            if (slot.text == playerNickname)
            {
                slot.transform.parent.GetComponent<Image>().color = isReady ? Color.yellow : Color.white;
                return;
            }
        }

        foreach (var slot in team2Slots)
        {
            if (slot.text == playerNickname)
            {
                slot.transform.parent.GetComponent<Image>().color = isReady ? Color.yellow : Color.white;
                return;
            }
        }
    }

    private bool AreAllPlayersReady()
    {
        foreach (var isReady in playerReadyStates.Values)
        {
            if (!isReady)
                return false;
        }
        return true;
    }
}