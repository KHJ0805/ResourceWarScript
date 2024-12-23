using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject objectPrefab; // ������ ������Ʈ ������
    private Vector2 spawnRange = new Vector2(20, 15); // x��, z�� ����
    private int spawnInterval = 30; // ������Ʈ ���� ����
    private int minDistanceBetweenObjects = 3; // ������Ʈ �� �ּ� �Ÿ�
    private int maxSpawnCount = 10; // �ִ� ���� ����
    private int poolSize = 15; // Ǯ���� ������Ʈ ��

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

        // �ʱ� ��ġ ���� (-8, 8)
        SpawnObjectAtPosition(new Vector3(planeCenter.x - 8, planeCenter.y, planeCenter.z));
        SpawnObjectAtPosition(new Vector3(planeCenter.x + 8, planeCenter.y, planeCenter.z));

        // �ֱ������� ������Ʈ ����
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
        Debug.LogWarning("��ȿ�� ���� ��ġ�� ã�� ����");
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
            Debug.LogWarning("������Ʈ Ǯ�� ���� ������Ʈ�� ����.");
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
