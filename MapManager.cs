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
    //맵 데이터 로드, 배치, 플레이어 생성에 관여

    private int cellSize = 5; // 각 셀의 간격

    //0,0 부모 오브젝트
    private GameObject maps;

    //플레이어 프리펩
    public GameObject[] playerModels;

    [Header("Plane")]
    //10,2 전장 플레인(공용 , 1개만)
    private GameObject BattleMapPrefab;
    //8,12 1p 협력지 플레인, 16,6 2p 협력지
    private GameObject PlayMapPrefab1Team;
    private GameObject PlayMapPrefab2Team;
    //20,16 벌목지 플레인
    private GameObject SpawnWoodZone;
    //20,10 채광지 플레인
    private GameObject SpawnSteelZone;

    [Header("1p BattleArea")]
    //-1,2 1p원거리 스폰 포인트
    public GameObject Team1RangedMonsterSpawnPoint;
    //0,2 1p공중 겸 공용 스폰 포인트
    public GameObject Team1CommonMonsterSpawnPoint;
    //1,2 1p 근거리 스폰 포인트
    public GameObject Team1MeleeMonsterSpawnPoint;
    //3,2 1p 넥서스 존
    public GameObject Team1NexusBuilding;
    //5,2 1p 중간타워 존
    private GameObject Team1MiddleTowerBuilding;
    //8,2 1p기본건물 존
    private GameObject Team1BasicBuildingBuilding;
    //10,2 점령지 존
    private GameObject OccupiedZone;

    [Header("2p BattleArea")]
    //-1,2 2p원거리 스폰 포인트
    public GameObject Team2RangedMonsterSpawnPoint;
    //0,2 2p공중 겸 공용 스폰 포인트
    public GameObject Team2CommonMonsterSpawnPoint;
    //1,2 2p 근거리 스폰 포인트
    public GameObject Team2MeleeMonsterSpawnPoint;
    //3,2 2p 넥서스 존
    public GameObject Team2NexusBuilding;
    //5,2 2p 중간타워 존
    private GameObject Team2MiddleTowerBuilding;
    //8,2 2p기본건물 존
    private GameObject Team2BasicBuildingBuilding;

    [Header("CoopArea")]
    //4,10 용광로 오브젝트
    private GameObject BlastFurnace;
    //4,12 제제소 오브젝트
    private GameObject Sawmill;
    //10,10 1p 제작소 오브젝트
    public GameObject CraftingBench;
    //6,14 1p npc1
    private GameObject Npc1;
    //8,14 1p npc2
    private GameObject Npc2;
    //10,14 1p npc3
    private GameObject Npc3;
    // 텔레포트 존
    private GameObject TeleportArea;
    ////14,14 벌목지 이동존
    //private GameObject MoveLog;
    ////14,10 채광지 이동존
    //private GameObject MoveMine;
    ////16,16 벌목지 도착존, 이동존과 상호 이동, 유저 team에 따라 이동시키는 위치 변경 필요
    //private GameObject DestinationLog;
    ////16,10 채광지 도착존
    //private GameObject DestinationMine;

    void Start()
    {
        StartCoroutine(LoadMapAssets());
        ////서버 연결되었을 때 플레이어 프리펩 생성
        CreatePlayerPrefab();
        //서버 없을 때 테스트 용으로 플레이어 프리펩 생성
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
            Debug.Log($"현재 플레이어의 팀은 {team}, 현재 플레이어는 {playerNumber}P입니다.");
        }
        else
        {
            Debug.LogError("DataManager 또는 CurrentPlayer가 null입니다.");
        }

        GameObject playerPrefab = null;

        if (team == 1)
        {
            // 팀 2의 플레이어 프리펩 설정
            playerPrefab = playerModels[playerNumber - 1 + 2]; // 팀 2의 첫 번째, 두 번째 플레이어
            Vector3 spawnPosition = GetWorldPosition(32 - (playerNumber - 1), 14); // (32, 14)와 (33, 14)
            GameObject playerClone = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
            DataManager.Instance.AssignVirtualCamera(playerClone, DataManager.Instance.CurrentPlayer.PlayerId);
        }
        else if (team == 2)
        {
            // 팀 1의 플레이어 프리펩 설정
            playerPrefab = playerModels[playerNumber - 1]; // 팀 1의 첫 번째, 두 번째 플레이어
            Vector3 spawnPosition = GetWorldPosition(7 + (playerNumber - 1), 14); // (7, 14)와 (8, 14)
            GameObject playerClone = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
            DataManager.Instance.AssignVirtualCamera(playerClone, DataManager.Instance.CurrentPlayer.PlayerId);
        }
    }
    private void CreateTestPlayerPrefab()
    {
        GameObject playerPrefab = null;
        playerPrefab = playerModels[1];
        Vector3 spawnPosition = GetWorldPosition(7, 14); // 팀 1 의 1 플레이어 기준
        Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
     
    }

    // 비동기적 맵로딩
    private IEnumerator LoadMapAssets()
    {
        // 부모 오브젝트 생성
        maps = new GameObject("Maps");
        maps.transform.position = Vector3.zero;

        // 모든 프리팹 로드
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

        // 배치 시작
        StartCoroutine(CreateBuildings());
        UIManager.Instance.CraftBenchSetting();
    }
    private IEnumerator LoadAndAssignPrefab(string path, System.Action<GameObject> assignAction)
    {
        ResourceRequest request = Resources.LoadAsync<GameObject>(path);
        yield return request;
        assignAction(request.asset as GameObject);
    }

    // 맵 오브젝트 배치하는 함수
    private IEnumerator CreateBuildings()
    {
        //배틀맵 플레인(20,2)
        BattleMapPrefab = Instantiate(BattleMapPrefab, GetWorldPosition(20, 2), Quaternion.identity, maps.transform);

        //목재 채집 구역(20,18)
        SpawnWoodZone = Instantiate(SpawnWoodZone, GetWorldPosition(20, 18, 0.5f), Quaternion.identity, maps.transform);
        //광석 채집 구역(20,12)
        SpawnSteelZone = Instantiate(SpawnSteelZone, GetWorldPosition(20, 12, 0.5f), Quaternion.identity, maps.transform);

        // 1team 배틀 오브젝트
        // 플레이 맵 배치 (8, 14)
        PlayMapPrefab1Team = Instantiate(PlayMapPrefab1Team, GetWorldPosition(8, 14), Quaternion.identity, maps.transform);

        // 원거리 스폰 포인트 (9, 2)
        Team1RangedMonsterSpawnPoint = Instantiate(Team1RangedMonsterSpawnPoint, GetWorldPosition(9, 2), Quaternion.identity, maps.transform);

        // 공중/공용 스폰 포인트 (10, 2)
        Team1CommonMonsterSpawnPoint = Instantiate(Team1CommonMonsterSpawnPoint, GetWorldPosition(10, 2), Quaternion.identity, maps.transform);

        // 근거리 스폰 포인트 (11, 2)
        Team1MeleeMonsterSpawnPoint = Instantiate(Team1MeleeMonsterSpawnPoint, GetWorldPosition(11, 2), Quaternion.identity, maps.transform);

        //// 넥서스 존 (3, 2)
        //Team1NexusBuilding = Instantiate(Team1NexusBuilding, GetWorldPosition(3, 2), Quaternion.identity, maps.transform);

        //// 중간 타워 존 (5, 2)
        //Team1MiddleTowerBuilding = Instantiate(Team1MiddleTowerBuilding, GetWorldPosition(5, 2), Quaternion.identity, maps.transform);

        //// 기본 건물 존 (8, 2)
        //Team1BasicBuildingBuilding = Instantiate(Team1BasicBuildingBuilding, GetWorldPosition(8, 2), Quaternion.identity, maps.transform);


        // 2team 배틀 오브젝트
        // 플레이 맵 배치 (32, 14)
        PlayMapPrefab2Team = Instantiate(PlayMapPrefab2Team, GetWorldPosition(32, 14), Quaternion.identity, maps.transform);

        // 원거리 스폰 포인트 (31, 2)
        Team2RangedMonsterSpawnPoint = Instantiate(Team2RangedMonsterSpawnPoint, GetWorldPosition(31, 2), Quaternion.identity, maps.transform);

        // 공중/공용 스폰 포인트 (30, 2)
        Team2CommonMonsterSpawnPoint = Instantiate(Team2CommonMonsterSpawnPoint, GetWorldPosition(30, 2), Quaternion.identity, maps.transform);

        // 근거리 스폰 포인트 (29, 2)
        Team2MeleeMonsterSpawnPoint = Instantiate(Team2MeleeMonsterSpawnPoint, GetWorldPosition(29, 2), Quaternion.identity, maps.transform);

        //// 넥서스 존 (17, 2)
        //Team2NexusBuilding = Instantiate(Team2NexusBuilding, GetWorldPosition(17, 2), Quaternion.identity, maps.transform);

        //// 중간 타워 존 (15, 2)
        //Team2MiddleTowerBuilding = Instantiate(Team2MiddleTowerBuilding, GetWorldPosition(15, 2), Quaternion.identity, maps.transform);

        //// 기본 건물 존 (12, 2)
        //Team2BasicBuildingBuilding = Instantiate(Team2BasicBuildingBuilding, GetWorldPosition(12, 2), Quaternion.identity, maps.transform);

        //점령지
        // 점령지 존 (20, 2)
        OccupiedZone = Instantiate(OccupiedZone, GetWorldPosition(20, 2), Quaternion.identity, maps.transform);

        //1team 협력 오브젝트
        // 용광로 (4, 12)
        BlastFurnace = Instantiate(BlastFurnace, GetWorldPosition(4, 12), Quaternion.identity, maps.transform);

        // 제재소 (4, 14)
        Sawmill = Instantiate(Sawmill, GetWorldPosition(4, 14), Quaternion.identity, maps.transform);

        // 1P 제작소 (10, 12)
        CraftingBench = Instantiate(CraftingBench, GetWorldPosition(10, 12), Quaternion.identity, maps.transform);

        // 1P NPC1 (6, 16)
        Npc1 = Instantiate(Npc1, GetWorldPosition(6, 16), Quaternion.identity, maps.transform);

        // 1P NPC2 (8, 16)
        Npc2 = Instantiate(Npc2, GetWorldPosition(8, 16), Quaternion.identity, maps.transform);

        // 1P NPC3 (10, 16)
        Npc3 = Instantiate(Npc3, GetWorldPosition(10, 16), Quaternion.identity, maps.transform);

        // 1팀에서 벌목지로 이동시켜주는 area (12, 16)
        TeleportArea = Instantiate(TeleportArea, GetWorldPosition(12, 16), Quaternion.identity, maps.transform);

        // 1팀에서 광산으로 이동시켜주는 area (12, 12)
        TeleportArea = Instantiate(TeleportArea, GetWorldPosition(12, 12), Quaternion.identity, maps.transform);

        // 벌목지 도착 존이자 벌목지에서 12,16으로 이동하는 area (16, 18)
        TeleportArea = Instantiate(TeleportArea, GetWorldPosition(16, 18), Quaternion.identity, maps.transform);

        // 벌목지 도착 존이자 벌목지에서 12,12으로 이동하는 area (16, 12)
        TeleportArea = Instantiate(TeleportArea, GetWorldPosition(16, 12), Quaternion.identity, maps.transform);

        //2team 협력 오브젝트
        // 용광로 (36, 12)
        BlastFurnace = Instantiate(BlastFurnace, GetWorldPosition(36, 12), Quaternion.identity, maps.transform);

        // 제재소 (36, 14)
        Sawmill = Instantiate(Sawmill, GetWorldPosition(36, 14), Quaternion.identity, maps.transform);

        // 2P 제작소 (30, 12)
        CraftingBench = Instantiate(CraftingBench, GetWorldPosition(30, 12), Quaternion.identity, maps.transform);

        // 2P NPC1 (34, 16)
        Npc1 = Instantiate(Npc1, GetWorldPosition(34, 16), Quaternion.identity, maps.transform);

        // 2P NPC2 (32, 16)
        Npc2 = Instantiate(Npc2, GetWorldPosition(32, 16), Quaternion.identity, maps.transform);

        // 2P NPC3 (30, 16)
        Npc3 = Instantiate(Npc3, GetWorldPosition(30, 16), Quaternion.identity, maps.transform);

        // 벌목지 이동 존 (28, 16)
        TeleportArea = Instantiate(TeleportArea, GetWorldPosition(28, 16), Quaternion.identity, maps.transform);

        // 채광지 이동 존 (28, 12)
        TeleportArea = Instantiate(TeleportArea, GetWorldPosition(28, 12), Quaternion.identity, maps.transform);

        // 벌목지 도착 존 (24, 18)
        TeleportArea = Instantiate(TeleportArea, GetWorldPosition(24, 18), Quaternion.identity, maps.transform);

        // 채광지 도착 존 (24, 12)
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

    // 그리드 좌표를 월드 좌표로 변환하는 함수
    public Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(x * cellSize, 0, y * cellSize);

    }

    private Vector3 GetWorldPosition(int x, int y, float z)
    {
        return new Vector3(x * cellSize, z, y * cellSize);
    }

    // 월드 좌표를 그리드 좌표로 변환하는 함수 서버와 위치동기화를 하는 좌표는 퍼블릭으로 바꾸고 위치 보낼것
    private Vector2Int GetGridPosition(Vector3 worldPosition)
    {
        int x = Mathf.RoundToInt(worldPosition.x / cellSize);
        int y = Mathf.RoundToInt(worldPosition.z / cellSize);
        return new Vector2Int(x, y);
    }
}
