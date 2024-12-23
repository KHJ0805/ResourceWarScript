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

    // 방 코드
    [SerializeField] private TextMeshProUGUI gameCodeText;

    private const int maxPlayersPer0Team = 4;
    private const int maxPlayersPerCompetitiveTeam = 2;
    private const int maxTotalPlayers = 4;

    //레디 상태 저장
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
            Debug.Log("게임 시작!");
            GameServerManager.Instance.GameStartReq();
        }
        else if (AreAllPlayersReady() && !DataManager.Instance.IsTeamBalanced())
        {
            Debug.Log("2:2 팀 구성이 필요합니다.");
            GameServerManager.Instance.GameStartReq();
        }
    }

    private void OnLeaveRoom()
    {
        // 방 코드 초기화
        gameCodeText.text = "";

        SessionUiManager.Instance.LobbyScenePopUpUi("퇴장 중...");

        GameServerManager.Instance.SendQuitRoomReq();
    }

    private void SwitchTeam(int targetTeamNumber)
    {
        //같은 팀으로 이동할때
        if (DataManager.Instance.CurrentPlayer.TeamIndex == targetTeamNumber)
        {
            Debug.Log("이미 선택한 팀에 있습니다.");
            return;
        }
        //레디 상태에서 이동할때
        bool isReady = DataManager.Instance.GetPlayerReadyState(DataManager.Instance.CurrentPlayer.PlayerName);
        if (isReady)
        {
            Debug.Log("레디 상태에서는 팀을 변경할 수 없습니다.");
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
        // 닉네임에 해당하는 텍스트 부모 오브젝트의 배경색 변경
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