using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class Spawner : MonoBehaviour
{

    public int maxSpawned = 5;
    public double timer = 3;
    public GameObject[] prefabs;
    public Transform[] targets;
    public Transform[] spawnPoints;
    private GameObject[] spawned;
    private int[] spawnedPrefabIndices;
    private int currentSpawnPoint = 0;
    private double lastSpawnTime = 0;

    void Start()
    {
        spawned = new GameObject[maxSpawned];
        spawnedPrefabIndices = new int[maxSpawned];

        for (int i = 0; i < maxSpawned; i++)
        {
            spawnedPrefabIndices[i] = -1;
        }

		Events.destroyEvent += OnDestruction;

    }

    void FixedUpdate()
    {

        if (maxSpawned != spawned.Length)
        {
            System.Array.Resize(ref spawned, maxSpawned);
            var oldLength = spawnedPrefabIndices.Length;
            System.Array.Resize(ref spawnedPrefabIndices, maxSpawned);
            for (int i = oldLength; i < maxSpawned; i++)
            {
                spawnedPrefabIndices[i] = -1;
            }
        }

        for (int i = 0; i < maxSpawned; i++)
        {
            if (spawned[i] != null)
            {
                BotTarget(spawned[i], spawned[i].transform);
            }
        }

        if (Time.timeAsDouble > lastSpawnTime + timer)
        {

            for (int i = 0; i < maxSpawned; i++)
            {
                if (spawned[i] == null)
                {
                    currentSpawnPoint = (currentSpawnPoint + 1) % spawnPoints.Length;
                    Spawn(i, spawnPoints[currentSpawnPoint]);
                    break;
                }
            }

            lastSpawnTime = Time.timeAsDouble;
        }

    }

    public Transform GetClosestTarget(Vector3 position)
    {
        Transform closest = null;
        float minDistance = float.MaxValue;
        foreach (Transform target in targets)
        {
            if (!target || !target.gameObject || !target.gameObject.activeInHierarchy) continue;

            float distance = Vector3.Distance(position, target.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = target;
            }
        }

        return closest;
    }

    bool BotTarget(GameObject prefab, Transform point)
    {
        var agent = prefab.GetComponent<BotAgent>();
        if (agent != null)
        {
            var target = GetClosestTarget(point.position);
            if (!target) return true;

            if (!agent.p) agent.p = agent.GetComponent<BotParameters>();

            agent.p.target = target;
        }

        return true;
    }

    void Spawn(int index, Transform point)
    {
        var prefabIndex = spawnedPrefabIndices[index];
        if (prefabIndex == -1)
        {
            prefabIndex = Random.Range(0, prefabs.Length);
            spawnedPrefabIndices[index] = prefabIndex;
        }
        GameObject prefab = prefabs[prefabIndex];

        if (!BotTarget(prefab, point)) return;

        spawned[index] = Instantiate(prefab, point.position, point.rotation);
        spawned[index].transform.parent = transform;

    }

    public void OnDestruction(GameObject obj)
    {
        foreach(GameObject s in spawned)
        {
            if(s == obj && obj.GetComponent<BotAgent>())
            {
                Destroy(obj);
            }
        }
    }

	public void OnDestroy()
	{
		Events.destroyEvent -= OnDestruction;
	}

}
