using UnityEngine;
using System.Collections;
using System;
using Unity.AI.Navigation;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }

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
    //�� ������ �ε�, ��ġ, �÷��̾� ������ ����

    private int cellSize = 5; // �� ���� ����

    //0,0 �θ� ������Ʈ
    private GameObject maps;

    //�÷��̾� ������
    public GameObject[] playerModels;

    [Header("Plane")]
    //10,2 ���� �÷���(���� , 1����)
    private GameObject BattleMapPrefab;
    //8,12 1p ������ �÷���, 16,6 2p ������
    private GameObject PlayMapPrefab1Team;
    private GameObject PlayMapPrefab2Team;
    //20,16 ������ �÷���
    private GameObject SpawnWoodZone;
    //20,10 ä���� �÷���
    private GameObject SpawnSteelZone;

    [Header("1p BattleArea")]
    //-1,2 1p���Ÿ� ���� ����Ʈ
    public GameObject Team1RangedMonsterSpawnPoint;
    //0,2 1p���� �� ���� ���� ����Ʈ
    public GameObject Team1CommonMonsterSpawnPoint;
    //1,2 1p �ٰŸ� ���� ����Ʈ
    public GameObject Team1MeleeMonsterSpawnPoint;
    //3,2 1p �ؼ��� ��
    public GameObject Team1NexusBuilding;
    //5,2 1p �߰�Ÿ�� ��
    private GameObject Team1MiddleTowerBuilding;
    //8,2 1p�⺻�ǹ� ��
    private GameObject Team1BasicBuildingBuilding;
    //10,2 ������ ��
    private GameObject OccupiedZone;

    [Header("2p BattleArea")]
    //-1,2 2p���Ÿ� ���� ����Ʈ
    public GameObject Team2RangedMonsterSpawnPoint;
    //0,2 2p���� �� ���� ���� ����Ʈ
    public GameObject Team2CommonMonsterSpawnPoint;
    //1,2 2p �ٰŸ� ���� ����Ʈ
    public GameObject Team2MeleeMonsterSpawnPoint;
    //3,2 2p �ؼ��� ��
    public GameObject Team2NexusBuilding;
    //5,2 2p �߰�Ÿ�� ��
    private GameObject Team2MiddleTowerBuilding;
    //8,2 2p�⺻�ǹ� ��
    private GameObject Team2BasicBuildingBuilding;

    [Header("CoopArea")]
    //4,10 �뱤�� ������Ʈ
    private GameObject BlastFurnace;
    //4,12 ������ ������Ʈ
    private GameObject Sawmill;
    //10,10 1p ���ۼ� ������Ʈ
    public GameObject CraftingBench;
    //6,14 1p npc1
    private GameObject Npc1;
    //8,14 1p npc2
    private GameObject Npc2;
    //10,14 1p npc3
    private GameObject Npc3;
    // �ڷ���Ʈ ��
    private GameObject TeleportArea;
    ////14,14 ������ �̵���
    //private GameObject MoveLog;
    ////14,10 ä���� �̵���
    //private GameObject MoveMine;
    ////16,16 ������ ������, �̵����� ��ȣ �̵�, ���� team�� ���� �̵���Ű�� ��ġ ���� �ʿ�
    //private GameObject DestinationLog;
    ////16,10 ä���� ������
    //private GameObject DestinationMine;

    void Start()
    {
        StartCoroutine(LoadMapAssets());
        ////���� ����Ǿ��� �� �÷��̾� ������ ����
        CreatePlayerPrefab();
        //���� ���� �� �׽�Ʈ ������ �÷��̾� ������ ����
        //CreateTestPlayerPrefab();
    }


    private void CreatePlayerPrefab()
    {
        uint team = 1; 
        int playerNumber = 1; 

        if (DataManager.Instance != null && DataManager.Instance.CurrentPlayer != null)
        {
            team = DataManager.Instance.CurrentPlayer.TeamIndex;
            playerNumber = (int)DataManager.Instance.CurrentPlayer.PlayerNum;
            Debug.Log($"���� �÷��̾��� ���� {team}, ���� �÷��̾�� {playerNumber}P�Դϴ�.");
        }
        else
        {
            Debug.LogError("DataManager �Ǵ� CurrentPlayer�� null�Դϴ�.");
        }

        GameObject playerPrefab = null;

        if (team == 1)
        {
            // �� 2�� �÷��̾� ������ ����
            playerPrefab = playerModels[playerNumber - 1 + 2]; // �� 2�� ù ��°, �� ��° �÷��̾�
            Vector3 spawnPosition = GetWorldPosition(32 - (playerNumber - 1), 14); // (32, 14)�� (33, 14)
            GameObject playerClone = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
            DataManager.Instance.AssignVirtualCamera(playerClone, DataManager.Instance.CurrentPlayer.PlayerId);
        }
        else if (team == 2)
        {
            // �� 1�� �÷��̾� ������ ����
            playerPrefab = playerModels[playerNumber - 1]; // �� 1�� ù ��°, �� ��° �÷��̾�
            Vector3 spawnPosition = GetWorldPosition(7 + (playerNumber - 1), 14); // (7, 14)�� (8, 14)
            GameObject playerClone = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
            DataManager.Instance.AssignVirtualCamera(playerClone, DataManager.Instance.CurrentPlayer.PlayerId);
        }
    }
    private void CreateTestPlayerPrefab()
    {
        GameObject playerPrefab = null;
        playerPrefab = playerModels[1];
        Vector3 spawnPosition = GetWorldPosition(7, 14); // �� 1 �� 1 �÷��̾� ����
        Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
     
    }

    // �񵿱��� �ʷε�
    private IEnumerator LoadMapAssets()
    {
        // �θ� ������Ʈ ����
        maps = new GameObject("Maps");
        maps.transform.position = Vector3.zero;

        // ��� ������ �ε�
        yield return StartCoroutine(LoadAndAssignPrefab("Map/BattleMap", prefab => BattleMapPrefab = prefab));
        yield return StartCoroutine(LoadAndAssignPrefab("Map/PlayMap1Team", prefab => PlayMapPrefab1Team = prefab));
        yield return StartCoroutine(LoadAndAssignPrefab("Map/PlayMap2Team", prefab => PlayMapPrefab2Team = prefab));
        yield return StartCoroutine(LoadAndAssignPrefab("Map/SpawnWoodZone", prefab => SpawnWoodZone = prefab));
        yield return StartCoroutine(LoadAndAssignPrefab("Map/SpawnSteelZone", prefab => SpawnSteelZone = prefab));

        yield return StartCoroutine(LoadAndAssignPrefab("Map/BattleObject/Team1RangedMonsterSpawnPoint", prefab => Team1RangedMonsterSpawnPoint = prefab));
        yield return StartCoroutine(LoadAndAssignPrefab("Map/BattleObject/Team1CommonMonsterSpawnPoint", prefab => Team1CommonMonsterSpawnPoint = prefab));
        yield return StartCoroutine(LoadAndAssignPrefab("Map/BattleObject/Team1MeleeMonsterSpawnPoint", prefab => Team1MeleeMonsterSpawnPoint = prefab));
        //yield return StartCoroutine(LoadAndAssignPrefab("Map/BattleObject/Team1NexusBuilding", prefab => Team1NexusBuilding = prefab));
        //yield return StartCoroutine(LoadAndAssignPrefab("Map/BattleObject/Team1MiddleTowerBuilding", prefab => Team1MiddleTowerBuilding = prefab));
        //yield return StartCoroutine(LoadAndAssignPrefab("Map/BattleObject/Team1BasicBuildingBuilding", prefab => Team1BasicBuildingBuilding = prefab));

        yield return StartCoroutine(LoadAndAssignPrefab("Map/BattleObject/Team2RangedMonsterSpawnPoint", prefab => Team2RangedMonsterSpawnPoint = prefab));
        yield return StartCoroutine(LoadAndAssignPrefab("Map/BattleObject/Team2CommonMonsterSpawnPoint", prefab => Team2CommonMonsterSpawnPoint = prefab));
        yield return StartCoroutine(LoadAndAssignPrefab("Map/BattleObject/Team2MeleeMonsterSpawnPoint", prefab => Team2MeleeMonsterSpawnPoint = prefab));
        //yield return StartCoroutine(LoadAndAssignPrefab("Map/BattleObject/Team2NexusBuilding", prefab => Team2NexusBuilding = prefab));
        //yield return StartCoroutine(LoadAndAssignPrefab("Map/BattleObject/Team2MiddleTowerBuilding", prefab => Team2MiddleTowerBuilding = prefab));
        //yield return StartCoroutine(LoadAndAssignPrefab("Map/BattleObject/Team2BasicBuildingBuilding", prefab => Team2BasicBuildingBuilding = prefab));

        yield return StartCoroutine(LoadAndAssignPrefab("Map/BattleObject/OccupiedZone", prefab => OccupiedZone = prefab));

        yield return StartCoroutine(LoadAndAssignPrefab("Map/CoopObject/BlastFurnace", prefab => BlastFurnace = prefab));
        yield return StartCoroutine(LoadAndAssignPrefab("Map/CoopObject/Sawmill", prefab => Sawmill = prefab));
        yield return StartCoroutine(LoadAndAssignPrefab("Map/CoopObject/CraftingBench", prefab => CraftingBench = prefab));
        yield return StartCoroutine(LoadAndAssignPrefab("Map/CoopObject/Npc1", prefab => Npc1 = prefab));
        yield return StartCoroutine(LoadAndAssignPrefab("Map/CoopObject/Npc2", prefab => Npc2 = prefab));
        yield return StartCoroutine(LoadAndAssignPrefab("Map/CoopObject/Npc3", prefab => Npc3 = prefab));
        yield return StartCoroutine(LoadAndAssignPrefab("Map/CoopObject/TeleportArea", prefab => TeleportArea = prefab));

        // ��ġ ����
        StartCoroutine(CreateBuildings());
        UIManager.Instance.CraftBenchSetting();
    }
    private IEnumerator LoadAndAssignPrefab(string path, System.Action<GameObject> assignAction)
    {
        ResourceRequest request = Resources.LoadAsync<GameObject>(path);
        yield return request;
        assignAction(request.asset as GameObject);
    }

    // �� ������Ʈ ��ġ�ϴ� �Լ�
    private IEnumerator CreateBuildings()
    {
        //��Ʋ�� �÷���(20,2)
        BattleMapPrefab = Instantiate(BattleMapPrefab, GetWorldPosition(20, 2), Quaternion.identity, maps.transform);

        //���� ä�� ����(20,18)
        SpawnWoodZone = Instantiate(SpawnWoodZone, GetWorldPosition(20, 18, 0.5f), Quaternion.identity, maps.transform);
        //���� ä�� ����(20,12)
        SpawnSteelZone = Instantiate(SpawnSteelZone, GetWorldPosition(20, 12, 0.5f), Quaternion.identity, maps.transform);

        // 1team ��Ʋ ������Ʈ
        // �÷��� �� ��ġ (8, 14)
        PlayMapPrefab1Team = Instantiate(PlayMapPrefab1Team, GetWorldPosition(8, 14), Quaternion.identity, maps.transform);

        // ���Ÿ� ���� ����Ʈ (9, 2)
        Team1RangedMonsterSpawnPoint = Instantiate(Team1RangedMonsterSpawnPoint, GetWorldPosition(9, 2), Quaternion.identity, maps.transform);

        // ����/���� ���� ����Ʈ (10, 2)
        Team1CommonMonsterSpawnPoint = Instantiate(Team1CommonMonsterSpawnPoint, GetWorldPosition(10, 2), Quaternion.identity, maps.transform);

        // �ٰŸ� ���� ����Ʈ (11, 2)
        Team1MeleeMonsterSpawnPoint = Instantiate(Team1MeleeMonsterSpawnPoint, GetWorldPosition(11, 2), Quaternion.identity, maps.transform);

        //// �ؼ��� �� (3, 2)
        //Team1NexusBuilding = Instantiate(Team1NexusBuilding, GetWorldPosition(3, 2), Quaternion.identity, maps.transform);

        //// �߰� Ÿ�� �� (5, 2)
        //Team1MiddleTowerBuilding = Instantiate(Team1MiddleTowerBuilding, GetWorldPosition(5, 2), Quaternion.identity, maps.transform);

        //// �⺻ �ǹ� �� (8, 2)
        //Team1BasicBuildingBuilding = Instantiate(Team1BasicBuildingBuilding, GetWorldPosition(8, 2), Quaternion.identity, maps.transform);


        // 2team ��Ʋ ������Ʈ
        // �÷��� �� ��ġ (32, 14)
        PlayMapPrefab2Team = Instantiate(PlayMapPrefab2Team, GetWorldPosition(32, 14), Quaternion.identity, maps.transform);

        // ���Ÿ� ���� ����Ʈ (31, 2)
        Team2RangedMonsterSpawnPoint = Instantiate(Team2RangedMonsterSpawnPoint, GetWorldPosition(31, 2), Quaternion.identity, maps.transform);

        // ����/���� ���� ����Ʈ (30, 2)
        Team2CommonMonsterSpawnPoint = Instantiate(Team2CommonMonsterSpawnPoint, GetWorldPosition(30, 2), Quaternion.identity, maps.transform);

        // �ٰŸ� ���� ����Ʈ (29, 2)
        Team2MeleeMonsterSpawnPoint = Instantiate(Team2MeleeMonsterSpawnPoint, GetWorldPosition(29, 2), Quaternion.identity, maps.transform);

        //// �ؼ��� �� (17, 2)
        //Team2NexusBuilding = Instantiate(Team2NexusBuilding, GetWorldPosition(17, 2), Quaternion.identity, maps.transform);

        //// �߰� Ÿ�� �� (15, 2)
        //Team2MiddleTowerBuilding = Instantiate(Team2MiddleTowerBuilding, GetWorldPosition(15, 2), Quaternion.identity, maps.transform);

        //// �⺻ �ǹ� �� (12, 2)
        //Team2BasicBuildingBuilding = Instantiate(Team2BasicBuildingBuilding, GetWorldPosition(12, 2), Quaternion.identity, maps.transform);

        //������
        // ������ �� (20, 2)
        OccupiedZone = Instantiate(OccupiedZone, GetWorldPosition(20, 2), Quaternion.identity, maps.transform);

        //1team ���� ������Ʈ
        // �뱤�� (4, 12)
        BlastFurnace = Instantiate(BlastFurnace, GetWorldPosition(4, 12), Quaternion.identity, maps.transform);

        // ����� (4, 14)
        Sawmill = Instantiate(Sawmill, GetWorldPosition(4, 14), Quaternion.identity, maps.transform);

        // 1P ���ۼ� (10, 12)
        CraftingBench = Instantiate(CraftingBench, GetWorldPosition(10, 12), Quaternion.identity, maps.transform);

        // 1P NPC1 (6, 16)
        Npc1 = Instantiate(Npc1, GetWorldPosition(6, 16), Quaternion.identity, maps.transform);

        // 1P NPC2 (8, 16)
        Npc2 = Instantiate(Npc2, GetWorldPosition(8, 16), Quaternion.identity, maps.transform);

        // 1P NPC3 (10, 16)
        Npc3 = Instantiate(Npc3, GetWorldPosition(10, 16), Quaternion.identity, maps.transform);

        // 1������ �������� �̵������ִ� area (12, 16)
        TeleportArea = Instantiate(TeleportArea, GetWorldPosition(12, 16), Quaternion.identity, maps.transform);

        // 1������ �������� �̵������ִ� area (12, 12)
        TeleportArea = Instantiate(TeleportArea, GetWorldPosition(12, 12), Quaternion.identity, maps.transform);

        // ������ ���� ������ ���������� 12,16���� �̵��ϴ� area (16, 18)
        TeleportArea = Instantiate(TeleportArea, GetWorldPosition(16, 18), Quaternion.identity, maps.transform);

        // ������ ���� ������ ���������� 12,12���� �̵��ϴ� area (16, 12)
        TeleportArea = Instantiate(TeleportArea, GetWorldPosition(16, 12), Quaternion.identity, maps.transform);

        //2team ���� ������Ʈ
        // �뱤�� (36, 12)
        BlastFurnace = Instantiate(BlastFurnace, GetWorldPosition(36, 12), Quaternion.identity, maps.transform);

        // ����� (36, 14)
        Sawmill = Instantiate(Sawmill, GetWorldPosition(36, 14), Quaternion.identity, maps.transform);

        // 2P ���ۼ� (30, 12)
        CraftingBench = Instantiate(CraftingBench, GetWorldPosition(30, 12), Quaternion.identity, maps.transform);

        // 2P NPC1 (34, 16)
        Npc1 = Instantiate(Npc1, GetWorldPosition(34, 16), Quaternion.identity, maps.transform);

        // 2P NPC2 (32, 16)
        Npc2 = Instantiate(Npc2, GetWorldPosition(32, 16), Quaternion.identity, maps.transform);

        // 2P NPC3 (30, 16)
        Npc3 = Instantiate(Npc3, GetWorldPosition(30, 16), Quaternion.identity, maps.transform);

        // ������ �̵� �� (28, 16)
        TeleportArea = Instantiate(TeleportArea, GetWorldPosition(28, 16), Quaternion.identity, maps.transform);

        // ä���� �̵� �� (28, 12)
        TeleportArea = Instantiate(TeleportArea, GetWorldPosition(28, 12), Quaternion.identity, maps.transform);

        // ������ ���� �� (24, 18)
        TeleportArea = Instantiate(TeleportArea, GetWorldPosition(24, 18), Quaternion.identity, maps.transform);

        // ä���� ���� �� (24, 12)
        TeleportArea = Instantiate(TeleportArea, GetWorldPosition(24, 12), Quaternion.identity, maps.transform);
        FindNexusBuildings();
        yield return new WaitForSecondsRealtime(0.5f);
        yield return null;
    }

    private void FindNexusBuildings()
    {
        if (BattleMapPrefab != null)
        {
            Team1NexusBuilding = BattleMapPrefab.transform.Find("Team1NexusBuilding")?.gameObject;
            Team2NexusBuilding = BattleMapPrefab.transform.Find("Team2NexusBuilding")?.gameObject;
        }
    }

    private Vector3 GetSpawnPosition(int team, int playerNumber)
    {
        Vector3 spawnPosition = Vector3.zero;

        if (team == 1)
        {
            if (playerNumber == 1)
            {
                spawnPosition = GetWorldPosition(3, 6); // team1 1p
            }
            else if (playerNumber == 2)
            {
                spawnPosition = GetWorldPosition(5, 6); // team1 2p
            }
        }
        else if (team == 2)
        {
            if (playerNumber == 1)
            {
                spawnPosition = GetWorldPosition(15, 6); // team2 1p
            }
            else if (playerNumber == 2)
            {
                spawnPosition = GetWorldPosition(17, 6); // team2 2p
            }
        }

        return spawnPosition;
    }

    // �׸��� ��ǥ�� ���� ��ǥ�� ��ȯ�ϴ� �Լ�
    public Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(x * cellSize, 0, y * cellSize);

    }

    private Vector3 GetWorldPosition(int x, int y, float z)
    {
        return new Vector3(x * cellSize, z, y * cellSize);
    }

    // ���� ��ǥ�� �׸��� ��ǥ�� ��ȯ�ϴ� �Լ� ������ ��ġ����ȭ�� �ϴ� ��ǥ�� �ۺ����� �ٲٰ� ��ġ ������
    private Vector2Int GetGridPosition(Vector3 worldPosition)
    {
        int x = Mathf.RoundToInt(worldPosition.x / cellSize);
        int y = Mathf.RoundToInt(worldPosition.z / cellSize);
        return new Vector2Int(x, y);
    }
}
