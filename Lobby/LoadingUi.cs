using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections;

public class LoadingUi : MonoBehaviour
{
    public static LoadingUi Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [Header("PlayerNickName")]
    public TextMeshProUGUI team1Player1NickName; // 팀 1 첫 번째 플레이어 텍스트
    public TextMeshProUGUI team1Player2NickName; // 팀 1 두 번째 플레이어 텍스트
    public TextMeshProUGUI team2Player1NickName; // 팀 2 첫 번째 플레이어 텍스트
    public TextMeshProUGUI team2Player2NickName; // 팀 2 두 번째 플레이어 텍스트

    [Header("PlayerLoadingProgress")]
    public TextMeshProUGUI team1Player1LoadingProgress;
    public TextMeshProUGUI team1Player2LoadingProgress;
    public TextMeshProUGUI team2Player1LoadingProgress;
    public TextMeshProUGUI team2Player2LoadingProgress;

    private Dictionary<uint, uint> progressData; // playerid, progress

    private void Start()
    {
        progressData = new Dictionary<uint, uint>();
        UpdateTeamNicknames();
        StartCoroutine(SimulateClientLoadingProgress());
    }

    // 닉네임 업데이트
    private void UpdateTeamNicknames()
    {
        var team1Players = DataManager.Instance.GetTeamPlayers(1);
        var team2Players = DataManager.Instance.GetTeamPlayers(2);

        // 팀 1 플레이어 1
        if (team1Players.Count > 0)
        {
            team1Player1NickName.text = team1Players[0].PlayerName;
        }
        else
        {
            team1Player1NickName.text = "Player1";
        }

        // 팀 1 플레이어 2
        if (team1Players.Count > 1)
        {
            team1Player2NickName.text = team1Players[1].PlayerName;
        }
        else
        {
            team1Player2NickName.text = "Player2";
        }

        // 팀 2 플레이어 1
        if (team2Players.Count > 0)
        {
            team2Player1NickName.text = team2Players[0].PlayerName;
        }
        else
        {
            team2Player1NickName.text = "Player1";
        }

        // 팀 2 플레이어 2
        if (team2Players.Count > 1)
        {
            team2Player2NickName.text = team2Players[1].PlayerName;
        }
        else
        {
            team2Player2NickName.text = "Player2";
        }
    }

    // 서버에서 받은 로딩 진행도에 따라 업데이트
    public void UpdateLoadingProgress(uint playerId, uint progress)
    {
        if (progressData.ContainsKey(playerId))
        {
            progressData[playerId] = progress;
        }
        else
        {
            progressData.Add(playerId, progress);
        }

        var team1Players = DataManager.Instance.GetTeamPlayers(1);
        if (team1Players.Count > 0 && playerId == team1Players[0].PlayerId)
        {
            team1Player1LoadingProgress.text = $"{progress}%";
        }
        else if (team1Players.Count > 1 && playerId == team1Players[1].PlayerId)
        {
            team1Player2LoadingProgress.text = $"{progress}%";
        }

        var team2Players = DataManager.Instance.GetTeamPlayers(2);
        if (team2Players.Count > 0 && playerId == team2Players[0].PlayerId)
        {
            team2Player1LoadingProgress.text = $"{progress}%";
        }
        else if (team2Players.Count > 1 && playerId == team2Players[1].PlayerId)
        {
            team2Player2LoadingProgress.text = $"{progress}%";
        }
    }

    // 로딩 진행 상태를 서버로 전송
    public void SendLoadProgressNotification(uint progress)
    {
        GameServerManager.Instance.SendLoadProgressNotification(progress);
    }

    // 서버로부터 받은 로딩 진행 데이터 처리
    public void ReceiveSyncLoadNotification(List<(uint playerId, uint progress)> loadProgressList)
    {
        foreach (var progressData in loadProgressList)
        {
            UpdateLoadingProgress(progressData.playerId, progressData.progress);
        }

        CheckAllPlayersLoadingComplete();
    }

    // 모든 플레이어의 로딩 완료 여부를 확인
    private void CheckAllPlayersLoadingComplete()
    {
        bool allPlayersLoaded = true;

        foreach (var progress in progressData.Values)
        {
            if (progress < 100)
            {
                allPlayersLoaded = false;
                break;
            }
        }

        if (allPlayersLoaded)
        {
            TransitionToGameScene();
        }
    }

    // 게임 씬으로 전환
    private void TransitionToGameScene()
    {
        SceneManager.LoadScene("MainScene");
    }

    private IEnumerator SimulateClientLoadingProgress()
    {
        uint totalProgress = 0;
        uint progressCount = 0;

        while (totalProgress < 100)
        {
            totalProgress += 1;

            //현재 플레이어의 진행 상태를 업데이트
            UpdateLoadingProgress(DataManager.Instance.CurrentPlayer.PlayerId, totalProgress);

            if (totalProgress >= (progressCount + 1) * 10)
            {
                progressCount++;
                SendLoadProgressNotification(totalProgress);
            }

            yield return new WaitForSeconds(0.1f);
        }

        // 로딩이 완료되면 씬 전환
        if (totalProgress >= 100)
        {
            CheckAllPlayersLoadingComplete(); // 로딩이 완료되었는지 체크
        }
    }
}
