using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
	[Serializable]
	public class SpawnEntry
	{
		public GameObject prefab;
		[Range(0f, 1f)] public float weight = 1f;
		[Min(0)] public int initialPoolSize = 0;
		[Min(1)] public int maxPoolSize = 100;
	}

	public enum SpawnMode
	{
		Manual,
		Continuous,
		Waves
	}

	[Header("Prefabs & Points")]
	public List<SpawnEntry> enemyPrefabs = new List<SpawnEntry>();
	public List<Transform> spawnPoints = new List<Transform>();

	[Tooltip("Used when no explicit spawn points are set.")]
	public Transform fallbackAreaCenter;
	public float fallbackAreaRadius = 10f;

	[Tooltip("Raycast mask to find ground when using fallback area.")]
	public LayerMask groundMask;
	public bool alignToGroundNormal = false;

	[Header("General Rules")]
	public SpawnMode mode = SpawnMode.Continuous;
	public bool spawnOnStart = true;
	[Tooltip("0 = unlimited")]
	public int totalSpawnLimit = 0;
	[Tooltip("0 = unlimited")]
	public int maxAlive = 10;

	[Header("Timing")]
	[Min(0.01f)] public float baseSpawnInterval = 3f;
	[Range(0f, 1f)] public float spawnIntervalVariance = 0.25f;

	[Header("Waves")]
	[Tooltip("Number of enemies per wave. Leave empty if not using Waves mode.")]
	public int[] waveCounts;
	public float waveStartDelay = 2f;
	public float timeBetweenWaves = 5f;

	[Header("Difficulty Scaling")]
	[Tooltip("X axis is normalized time (0..1), Y is rate multiplier (0.1..2 typically).")]
	public AnimationCurve spawnRateOverTime = AnimationCurve.Linear(0f, 1f, 1f, 1f);
	[Tooltip("How long it takes to reach the end of the curve, in seconds.")]
	public float difficultyTimeSeconds = 300f;

	public event Action<GameObject> OnEnemySpawned;
	public event Action<GameObject> OnEnemyDespawned;
	public event Action<int> OnWaveStarted;
	public event Action<int> OnWaveCompleted;

	readonly Dictionary<GameObject, Queue<GameObject>> prefabToPool = new Dictionary<GameObject, Queue<GameObject>>();
	readonly Dictionary<GameObject, GameObject> instanceToPrefab = new Dictionary<GameObject, GameObject>();
	readonly HashSet<GameObject> activeInstances = new HashSet<GameObject>();

	bool isSpawning;
	float elapsedSeconds;
	int totalSpawnedCount;

	void Awake()
	{
		InitializePools();
	}

	void Start()
	{
		if (spawnOnStart)
		{
			StartSpawning();
		}
	}

	void OnDestroy()
	{
		StopAllCoroutines();
	}

	public void StartSpawning()
	{
		if (isSpawning) return;
		isSpawning = true;
		StopAllCoroutines();

		switch (mode)
		{
			case SpawnMode.Continuous:
				StartCoroutine(ContinuousRoutine());
				break;
			case SpawnMode.Waves:
				StartCoroutine(WaveRoutine());
				break;
		}
	}

	public void StopSpawning()
	{
		isSpawning = false;
		StopAllCoroutines();
	}

	public bool TrySpawnOne()
	{
		if (!ShouldSpawnNext()) return false;

		var entry = ChooseWeightedEntry();
		if (entry == null || entry.prefab == null) return false;

		var instance = GetFromPool(entry);
		FindSpawnTransform(out var spawnPos, out var spawnRot);
		instance.transform.SetPositionAndRotation(spawnPos, spawnRot);
		instance.SetActive(true);

		activeInstances.Add(instance);
		totalSpawnedCount++;
		OnEnemySpawned?.Invoke(instance);
		return true;
	}

	public void NotifyEnemyDefeated(GameObject instance)
	{
		ReturnToPool(instance);
	}

	public void ForceDespawnAll()
	{
		var snapshot = new List<GameObject>(activeInstances);
		foreach (var inst in snapshot)
		{
			if (inst != null)
			{
				ReturnToPool(inst);
			}
		}
		activeInstances.Clear();
	}

	System.Collections.IEnumerator ContinuousRoutine()
	{
		elapsedSeconds = 0f;
		while (isSpawning)
		{
			elapsedSeconds += Time.deltaTime;

			if (ShouldSpawnNext())
			{
				TrySpawnOne();
			}

			yield return new WaitForSeconds(GetNextInterval());
		}
	}

	System.Collections.IEnumerator WaveRoutine()
	{
		yield return new WaitForSeconds(waveStartDelay);
		if (waveCounts == null || waveCounts.Length == 0)
		{
			isSpawning = false;
			yield break;
		}

		for (int waveIndex = 0; waveIndex < waveCounts.Length; waveIndex++)
		{
			if (!isSpawning) yield break;

			OnWaveStarted?.Invoke(waveIndex);

			int count = Mathf.Max(0, waveCounts[waveIndex]);
			for (int i = 0; i < count; i++)
			{
				while (!TrySpawnOne())
				{
					if (!isSpawning) yield break;
					yield return null;
				}
				yield return new WaitForSeconds(GetNextInterval());
			}

			yield return new WaitForSeconds(timeBetweenWaves);
			OnWaveCompleted?.Invoke(waveIndex);
		}

		isSpawning = false;
	}

	bool ShouldSpawnNext()
	{
		if (!isSpawning) return false;
		if (maxAlive > 0 && activeInstances.Count >= maxAlive) return false;
		if (totalSpawnLimit > 0 && totalSpawnedCount >= totalSpawnLimit) return false;
		return true;
	}

	float GetNextInterval()
	{
		float tNorm = difficultyTimeSeconds > 0f ? Mathf.Clamp01(elapsedSeconds / difficultyTimeSeconds) : 0f;
		float rateMultiplier = Mathf.Max(0.01f, spawnRateOverTime.Evaluate(tNorm));
		float baseInterval = Mathf.Max(0.05f, baseSpawnInterval * rateMultiplier);
		float variance = spawnIntervalVariance * baseInterval;
		return UnityEngine.Random.Range(baseInterval - variance, baseInterval + variance);
	}

	void InitializePools()
	{
		for (int i = 0; i < enemyPrefabs.Count; i++)
		{
			var entry = enemyPrefabs[i];
			if (entry == null || entry.prefab == null) continue;
			if (!prefabToPool.ContainsKey(entry.prefab))
			{
				prefabToPool[entry.prefab] = new Queue<GameObject>();
			}

			int toWarm = Mathf.Max(0, entry.initialPoolSize);
			for (int j = 0; j < toWarm; j++)
			{
				var inst = CreateInstance(entry.prefab);
				ReturnToPool(inst);
			}
		}
	}

	GameObject CreateInstance(GameObject prefab)
	{
		var go = Instantiate(prefab);
		go.SetActive(false);

		var tracker = go.GetComponent<SpawnedObjectTracker>();
		if (tracker == null) tracker = go.AddComponent<SpawnedObjectTracker>();
		tracker.Configure(this, prefab);

		instanceToPrefab[go] = prefab;
		return go;
	}

	GameObject GetFromPool(SpawnEntry entry)
	{
		if (!prefabToPool.TryGetValue(entry.prefab, out var pool))
		{
			pool = new Queue<GameObject>();
			prefabToPool[entry.prefab] = pool;
		}

		while (pool.Count > 0)
		{
			var obj = pool.Dequeue();
			if (obj != null) return obj;
		}

		return CreateInstance(entry.prefab);
	}

	internal void ReturnToPool(GameObject instance)
	{
		if (instance == null) return;

		activeInstances.Remove(instance);

		if (!instanceToPrefab.TryGetValue(instance, out var prefabKey) || prefabKey == null)
		{
			instance.SetActive(false);
			OnEnemyDespawned?.Invoke(instance);
			return;
		}

		if (!prefabToPool.TryGetValue(prefabKey, out var pool))
		{
			pool = new Queue<GameObject>();
			prefabToPool[prefabKey] = pool;
		}

		var tracker = instance.GetComponent<SpawnedObjectTracker>();
		if (tracker != null) tracker.SuppressNextDisableNotify();

		instance.SetActive(false);
		pool.Enqueue(instance);
		OnEnemyDespawned?.Invoke(instance);
	}

	void FindSpawnTransform(out Vector3 position, out Quaternion rotation)
	{
		Transform chosen = null;
		if (spawnPoints != null && spawnPoints.Count > 0)
		{
			chosen = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Count)];
		}

		if (chosen != null)
		{
			position = chosen.position;
			rotation = chosen.rotation;
			return;
		}

		var center = fallbackAreaCenter != null ? fallbackAreaCenter.position : transform.position;
		var sample = center + UnityEngine.Random.insideUnitSphere * fallbackAreaRadius;
		sample.y += 100f;

		int mask = groundMask.value == 0 ? Physics.DefaultRaycastLayers : groundMask.value;
		if (Physics.Raycast(sample, Vector3.down, out var hit, 250f, mask))
		{
			position = hit.point;
			rotation = alignToGroundNormal ? Quaternion.FromToRotation(Vector3.up, hit.normal) : Quaternion.identity;
		}
		else
		{
			position = center + UnityEngine.Random.insideUnitSphere * fallbackAreaRadius;
			position.y = center.y;
			rotation = Quaternion.identity;
		}
	}

	SpawnEntry ChooseWeightedEntry()
	{
		float total = 0f;
		for (int i = 0; i < enemyPrefabs.Count; i++)
		{
			var e = enemyPrefabs[i];
			if (e == null || e.prefab == null || e.weight <= 0f) continue;
			total += e.weight;
		}
		if (total <= 0f) return null;

		float r = UnityEngine.Random.value * total;
		float cumulative = 0f;
		for (int i = 0; i < enemyPrefabs.Count; i++)
		{
			var e = enemyPrefabs[i];
			if (e == null || e.prefab == null || e.weight <= 0f) continue;
			cumulative += e.weight;
			if (r <= cumulative) return e;
		}

		for (int i = 0; i < enemyPrefabs.Count; i++)
		{
			if (enemyPrefabs[i]?.prefab != null) return enemyPrefabs[i];
		}
		return null;
	}

	void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.green;
		if (spawnPoints != null)
		{
			for (int i = 0; i < spawnPoints.Count; i++)
			{
				var t = spawnPoints[i];
				if (t == null) continue;
				Gizmos.DrawWireSphere(t.position, 0.25f);
				Gizmos.DrawLine(t.position, t.position + t.forward * 0.75f);
			}
		}

		if (fallbackAreaRadius > 0f)
		{
			var c = fallbackAreaCenter != null ? fallbackAreaCenter.position : transform.position;
			Gizmos.color = new Color(0f, 0.5f, 1f, 0.35f);
			Gizmos.DrawWireSphere(c, fallbackAreaRadius);
		}
	}

	[DisallowMultipleComponent]
	sealed class SpawnedObjectTracker : MonoBehaviour
	{
		EnemySpawner owner;
		GameObject prefabKey;
		bool suppressDisableNotify;

		public void Configure(EnemySpawner spawner, GameObject prefab)
		{
			owner = spawner;
			prefabKey = prefab;
			if (owner != null && prefabKey != null && !owner.instanceToPrefab.ContainsKey(gameObject))
			{
				owner.instanceToPrefab[gameObject] = prefabKey;
			}
		}

		public void SuppressNextDisableNotify()
		{
			suppressDisableNotify = true;
		}

		void OnDisable()
		{
			if (suppressDisableNotify)
			{
				suppressDisableNotify = false;
				return;
			}
			if (owner != null)
			{
				owner.ReturnToPool(gameObject);
			}
		}

		void OnDestroy()
		{
			if (owner != null)
			{
				owner.activeInstances.Remove(gameObject);
				owner.instanceToPrefab.Remove(gameObject);
			}
		}
	}
}
