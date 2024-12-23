using Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DataManager : Singleton<DataManager>
{
    [Header("Player Info")]
    private readonly List<PlayerInfo> team0Players = new List<PlayerInfo>();
    private readonly List<PlayerInfo> team1Players = new List<PlayerInfo>();
    private readonly List<PlayerInfo> team2Players = new List<PlayerInfo>();

    [Header("Token")]
    private Coroutine tokenCheckCoroutine;
    public string Token { get; private set; } = string.Empty;
    public ulong TokenExpirationTime { get; private set; }

    private float TokenTimeLeftSeconds;

    private RoomUi cachedRoomUi;
    private GameObject LobbyUi;
    public GameObject RoomUi;

    private int requestedTeamChange = -1; // �⺻���� -1, �� ������ ��û���� ����

    public string GameCode { get; set; }
    public string GameUrl { get; private set; }

    private S2CSyncRoomNoti pendingSyncRoomNoti;

    public PlayerInfo CurrentPlayer { get; private set; }

    [System.Serializable]
    public class PlayerInfo
    {
        public uint PlayerId { get; set; } // �÷��̾� Ŭ���̾�Ʈ ���̵�
        public string PlayerName { get; set; } // �÷��̾� �г���
        public uint AvartarItem { get; set; } // ���� ��� ����
        public uint TeamIndex { get; set; } // �÷��̾ �� ������?
        public bool Ready { get; set; } // �뿡�� ���� �ߴ���? ���ߴ���?
        public int PlayerNum { get; set; } // �� ����Ʈ���� �÷��̾ ������ ���° �÷��̾�����
    }

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "LobbyScene")
        {
            // LobbyScene�� ���� RoomUi�� ĳ��
            LobbyUi = GameObject.Find("LobbyUi");
            if (LobbyUi != null)
            {
                RoomUi = LobbyUi.transform.Find("RoomUi").gameObject;
                cachedRoomUi = RoomUi.GetComponent<RoomUi>();
            }
        }
        else
        {
            cachedRoomUi = null;
        }
    }

    public void SetUserData(string token, ulong expirationTime, string playerNickName)
    {
        Token = token;
        TokenExpirationTime = expirationTime;
        TokenTimeLeftSeconds = expirationTime - Time.time;

        CurrentPlayer = new PlayerInfo
        {
            PlayerId = 0, // �⺻��
            PlayerName = playerNickName,
            AvartarItem = 0, // �⺻��
            TeamIndex = 0,
            Ready = false,
            PlayerNum = 0,
        };

        if (tokenCheckCoroutine == null)
        {
            tokenCheckCoroutine = StartCoroutine(CheckTokenExpiration());
        }
        Debug.Log($"Updated Token: {token}, Expiration Time: {expirationTime}");
    }

    public void SetTeamPlayers(List<PlayerInfo> team0, List<PlayerInfo> team1, List<PlayerInfo> team2)
    {
        team0Players.Clear();
        team1Players.Clear();
        team2Players.Clear();

        team0Players.AddRange(team0);
        team1Players.AddRange(team1);
        team2Players.AddRange(team2);

        UpdateCurrentPlayerNumber();
    }

    public bool IsTeamBalanced()
    {
        return team1Players.Count == 2 && team2Players.Count == 2;
    }

    public void RequestTeamChange(int targetTeam)
    {
        requestedTeamChange = targetTeam;
        GameServerManager.Instance.SendTeamChangeReq(targetTeam);
    }

    public void SetGameRoomData(string gameCode)
    {
        GameCode = gameCode;
        Debug.Log($"Game Code: {GameCode}");
    }

    private IEnumerator CheckTokenExpiration()
    {
        while (true)
        {
            if (TokenTimeLeftSeconds <= 60)
            {
                Debug.Log("Token ���� �ӹ�. ��߱� ��û.");
                SessionNetworkManager.Instance.SendRefreshTokenRequest(Token);
            }
            yield return new WaitForSeconds(30f);
        }
    }

    public void RefreshToken(string newToken, ulong newExpirationTime)
    {
        Token = newToken;
        TokenExpirationTime = newExpirationTime;
    }

    public void UpdateTeamData(S2CSyncRoomNoti syncRoomNoti)
    {
        if (!cachedRoomUi.gameObject.activeSelf)
        {
            pendingSyncRoomNoti = syncRoomNoti;
            Debug.Log("RoomUi�� ��Ȱ��ȭ �����Դϴ�. syncRoomNoti�� �е��س����ϴ�.");
            return;
        }

        try
        {
            List<PlayerInfo> team0 = new List<PlayerInfo>();
            List<PlayerInfo> team1 = new List<PlayerInfo>();
            List<PlayerInfo> team2 = new List<PlayerInfo>();

            foreach (var player in syncRoomNoti.Players)
            {
                var playerInfo = new PlayerInfo
                {
                    PlayerId = player.PlayerId > 0 ? player.PlayerId : 0,
                    PlayerName = player.PlayerName,
                    AvartarItem = player.AvartarItem > 0 ? player.AvartarItem : 0,
                    TeamIndex = player.TeamIndex > 0 ? player.TeamIndex : 0,
                    Ready = player.Ready,
                };
                // ��� �÷��̾� ����
                playerInfo.PlayerNum = GetPlayerNumInTeam((int)player.TeamIndex, player.PlayerName);
                // CurrentPlayer ������Ʈ (�г��� ��ġ ��)
                if (player.PlayerName == DataManager.Instance.CurrentPlayer.PlayerName)
                {
                    DataManager.Instance.CurrentPlayer = playerInfo;
                }

                // �� ���� �÷��̾� �߰�
                if (player.TeamIndex == 0) team0.Add(playerInfo);
                else if (player.TeamIndex == 1) team1.Add(playerInfo);
                else if (player.TeamIndex == 2) team2.Add(playerInfo);
            }

            SetTeamPlayers(team0, team1, team2);

            List<string> team1Nicknames = GetTeamNicknames(1);
            List<string> team2Nicknames = GetTeamNicknames(2);
            List<string> team0Nicknames = GetTeamNicknames(0);

            if (cachedRoomUi != null)
            {
                cachedRoomUi.UpdateTeamUI(team1Nicknames, team2Nicknames, team0Nicknames);
            }
            else
            {
                Debug.LogError("RoomUi�� �������� �ʽ��ϴ�.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to update team data: {ex.Message}");
        }
    }

    public void OnRoomUiActivated()
    {
        if (cachedRoomUi == null)
        {
            Debug.LogWarning("RoomUi�� Ȱ��ȭ���� �ʾҽ��ϴ�.");
            return;
        }

        if (pendingSyncRoomNoti != null)
        {
            UpdateTeamData(pendingSyncRoomNoti);
            pendingSyncRoomNoti = null;
        }
    }

    public void HandleTeamChangeResponse()
    {
        if (requestedTeamChange != -1)
        {
            CurrentPlayer.TeamIndex = (uint)requestedTeamChange;
            requestedTeamChange = -1;

            if (cachedRoomUi != null)
            {
                cachedRoomUi.UpdateTeamUI(
                    GetTeamNicknames(1),
                    GetTeamNicknames(2),
                    GetTeamNicknames(0)
                );
            }
        }
    }

    public void TogglePlayerReadyState(string playerNickname)
    {
        foreach (var team in new List<List<PlayerInfo>> { team1Players, team2Players })
        {
            foreach (var player in team)
            {
                if (player.PlayerName == playerNickname)
                {
                    if (cachedRoomUi != null)
                    {
                        cachedRoomUi.UpdateReadyStateUI(playerNickname, player.Ready);
                    }
                    return;
                }
            }
        }
    }

    public void UpdateCurrentPlayerNumber()
    {
        foreach (var player in team1Players.Concat(team2Players))
        {
            if (player.PlayerName == CurrentPlayer.PlayerName)
            {
                CurrentPlayer.PlayerId = (uint)(team1Players.Contains(player)
                    ? team1Players.IndexOf(player) + 1
                    : team2Players.IndexOf(player) + 1);
                CurrentPlayer.TeamIndex = player.TeamIndex;
                return;
            }
        }
    }

    public List<PlayerInfo> GetTeamPlayers(int teamIndex)
    {
        return teamIndex switch
        {
            0 => team0Players,
            1 => team1Players,
            2 => team2Players,
            _ => new List<PlayerInfo>()
        };
    }

    private int GetPlayerNumInTeam(int teamIndex, string playerName)
    {
        List<PlayerInfo> teamPlayers = null;

        if (teamIndex == 0) teamPlayers = team0Players;
        else if (teamIndex == 1) teamPlayers = team1Players;
        else if (teamIndex == 2) teamPlayers = team2Players;

        if (teamPlayers != null)
        {
            for (int i = 0; i < teamPlayers.Count; i++)
            {
                if (teamPlayers[i].PlayerName == playerName)
                {
                    return i + 1;
                }
            }
        }

        return 0;
    }

    public List<string> GetTeamNicknames(int teamIndex)
    {
        return GetTeamPlayers(teamIndex).ConvertAll(player => player.PlayerName);
    }

    public bool GetPlayerReadyState(string playerNickname)
    {
        foreach (var team in new List<List<PlayerInfo>> { team1Players, team2Players })
        {
            foreach (var player in team)
            {
                if (player.PlayerName == playerNickname)
                {
                    return player.Ready;
                }
            }
        }
        return false;
    }
    public void AssignVirtualCamera(GameObject playerObject, uint playerID)
    {
        if (playerID == CurrentPlayer.PlayerId)
        {
            PlayerController controller = playerObject.GetComponent<PlayerController>();
            controller.SetIsMine(true);
            Debug.Log("ī�޶� ���õ�");
        }
    }
}
