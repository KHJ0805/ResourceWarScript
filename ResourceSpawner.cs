using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject objectPrefab; // 스폰할 오브젝트 프리팹
    private Vector2 spawnRange = new Vector2(20, 15); // x축, z축 범위
    private int spawnInterval = 30; // 오브젝트 스폰 간격
    private int minDistanceBetweenObjects = 3; // 오브젝트 간 최소 거리
    private int maxSpawnCount = 10; // 최대 스폰 개수
    private int poolSize = 15; // 풀링할 오브젝트 수

    private Queue<GameObject> objectPool = new Queue<GameObject>();
    private List<GameObject> activeObjects = new List<GameObject>();

    private Vector3 planeCenter;
    private Transform poolParent;

    private void Start()
    {
        planeCenter = transform.position;

        poolParent = new GameObject("ResourceObjectPool").transform;
        poolParent.SetParent(transform);

        InitializeObjectPool();

        // 초기 위치 스폰 (-8, 8)
        SpawnObjectAtPosition(new Vector3(planeCenter.x - 8, planeCenter.y, planeCenter.z));
        SpawnObjectAtPosition(new Vector3(planeCenter.x + 8, planeCenter.y, planeCenter.z));

        // 주기적으로 오브젝트 스폰
        StartCoroutine(SpawnObjectsPeriodically());
    }

    private void InitializeObjectPool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(objectPrefab, poolParent);
            obj.SetActive(false);
            objectPool.Enqueue(obj);
        }
    }

    private IEnumerator SpawnObjectsPeriodically()
    {
        while (activeObjects.Count < maxSpawnCount)
        {
            Vector3 spawnPosition = GetValidSpawnPosition();
            if (spawnPosition != Vector3.zero)
            {
                SpawnObjectAtPosition(spawnPosition);
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private Vector3 GetValidSpawnPosition()
    {
        for (int i = 0; i < 10; i++)
        {
            float randomX = Random.Range(-spawnRange.x / 2, spawnRange.x / 2);
            float randomZ = Random.Range(-spawnRange.y / 2, spawnRange.y / 2);
            Vector3 candidatePosition = new Vector3(planeCenter.x + randomX, planeCenter.y, planeCenter.z + randomZ);

            if (IsPositionValid(candidatePosition))
            {
                return candidatePosition;
            }
        }
        Debug.LogWarning("유효한 스폰 위치를 찾지 못함");
        return Vector3.zero;
    }

    private bool IsPositionValid(Vector3 position)
    {
        foreach (var obj in activeObjects)
        {
            if (Vector3.Distance(position, obj.transform.position) < minDistanceBetweenObjects)
            {
                return false;
            }
        }
        return true;
    }

    private void SpawnObjectAtPosition(Vector3 position)
    {
        if (objectPool.Count > 0)
        {
            GameObject obj = objectPool.Dequeue();
            obj.transform.position = position;
            obj.SetActive(true);
            activeObjects.Add(obj);
        }
        else
        {
            Debug.LogWarning("오브젝트 풀에 남은 오브젝트가 없음.");
        }
    }

    public void DespawnObject(GameObject obj)
    {
        if (activeObjects.Contains(obj))
        {
            activeObjects.Remove(obj);
            obj.SetActive(false);
            objectPool.Enqueue(obj);
        }
    }
}
