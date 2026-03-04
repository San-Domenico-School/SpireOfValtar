using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPackSpawner : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private HealthPack healthPackPrefab;

    [Header("Spawn Points")]
    [SerializeField] private List<Transform> spawnPoints = new List<Transform>();

    [Header("Timing")]
    [Min(0.1f)] [SerializeField] private float minSpawnInterval = 30f;
    [Min(0.1f)] [SerializeField] private float maxSpawnInterval = 60f;

    [Header("Rules")]
    [SerializeField] private bool replaceExisting = true;

    private HealthPack activePack;
    private Coroutine spawnRoutine;

    private void OnEnable()
    {
        spawnRoutine = StartCoroutine(SpawnLoop());
    }

    private void OnDisable()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            SpawnNext();
            float delay = Random.Range(minSpawnInterval, maxSpawnInterval);
            yield return new WaitForSeconds(delay);
        }
    }

    private void SpawnNext()
    {
        if (healthPackPrefab == null)
        {
            Debug.LogWarning("HealthPackSpawner: Missing healthPackPrefab.");
            return;
        }

        if (spawnPoints == null || spawnPoints.Count == 0)
        {
            Debug.LogWarning("HealthPackSpawner: No spawn points assigned.");
            return;
        }

        if (replaceExisting && activePack != null)
        {
            Destroy(activePack.gameObject);
            activePack = null;
        }

        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
        if (spawnPoint == null)
        {
            Debug.LogWarning("HealthPackSpawner: A spawn point is missing.");
            return;
        }

        activePack = Instantiate(
            healthPackPrefab,
            spawnPoint.position,
            spawnPoint.rotation,
            transform
        );
        activePack.OnConsumed += HandlePackConsumed;
    }

    private void HandlePackConsumed(HealthPack pack)
    {
        if (pack == null)
        {
            return;
        }

        pack.OnConsumed -= HandlePackConsumed;
        if (activePack == pack)
        {
            activePack = null;
        }
    }
}
