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
    public TextMeshProUGUI team1Player1NickName; // �� 1 ù ��° �÷��̾� �ؽ�Ʈ
    public TextMeshProUGUI team1Player2NickName; // �� 1 �� ��° �÷��̾� �ؽ�Ʈ
    public TextMeshProUGUI team2Player1NickName; // �� 2 ù ��° �÷��̾� �ؽ�Ʈ
    public TextMeshProUGUI team2Player2NickName; // �� 2 �� ��° �÷��̾� �ؽ�Ʈ

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

    // �г��� ������Ʈ
    private void UpdateTeamNicknames()
    {
        var team1Players = DataManager.Instance.GetTeamPlayers(1);
        var team2Players = DataManager.Instance.GetTeamPlayers(2);

        // �� 1 �÷��̾� 1
        if (team1Players.Count > 0)
        {
            team1Player1NickName.text = team1Players[0].PlayerName;
        }
        else
        {
            team1Player1NickName.text = "Player1";
        }

        // �� 1 �÷��̾� 2
        if (team1Players.Count > 1)
        {
            team1Player2NickName.text = team1Players[1].PlayerName;
        }
        else
        {
            team1Player2NickName.text = "Player2";
        }

        // �� 2 �÷��̾� 1
        if (team2Players.Count > 0)
        {
            team2Player1NickName.text = team2Players[0].PlayerName;
        }
        else
        {
            team2Player1NickName.text = "Player1";
        }

        // �� 2 �÷��̾� 2
        if (team2Players.Count > 1)
        {
            team2Player2NickName.text = team2Players[1].PlayerName;
        }
        else
        {
            team2Player2NickName.text = "Player2";
        }
    }

    // �������� ���� �ε� ���൵�� ���� ������Ʈ
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

    // �ε� ���� ���¸� ������ ����
    public void SendLoadProgressNotification(uint progress)
    {
        GameServerManager.Instance.SendLoadProgressNotification(progress);
    }

    // �����κ��� ���� �ε� ���� ������ ó��
    public void ReceiveSyncLoadNotification(List<(uint playerId, uint progress)> loadProgressList)
    {
        foreach (var progressData in loadProgressList)
        {
            UpdateLoadingProgress(progressData.playerId, progressData.progress);
        }

        CheckAllPlayersLoadingComplete();
    }

    // ��� �÷��̾��� �ε� �Ϸ� ���θ� Ȯ��
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

    // ���� ������ ��ȯ
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

            //���� �÷��̾��� ���� ���¸� ������Ʈ
            UpdateLoadingProgress(DataManager.Instance.CurrentPlayer.PlayerId, totalProgress);

            if (totalProgress >= (progressCount + 1) * 10)
            {
                progressCount++;
                SendLoadProgressNotification(totalProgress);
            }

            yield return new WaitForSeconds(0.1f);
        }

        // �ε��� �Ϸ�Ǹ� �� ��ȯ
        if (totalProgress >= 100)
        {
            CheckAllPlayersLoadingComplete(); // �ε��� �Ϸ�Ǿ����� üũ
        }
    }
}
