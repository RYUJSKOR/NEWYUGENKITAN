using UnityEngine;

public class LoopSpawner : MonoBehaviour
{
	[Header("スポーン設定")]
	[SerializeField] private bool CanSpawn = true;
	[SerializeField] public GameObject SpawnObject;
	[SerializeField] private float SpawnInterval = 2.0f;
	[SerializeField] public GameObject TargetObject;
	[SerializeField] private float SpawnDistance = 10.0f;

	[Tooltip("これ以上ターゲットが近いとスポーンしない距離")]
	[SerializeField] private float MinSpawnDistance = 2.0f;
	[SerializeField] private bool IsWatchEnemy = true;

	[Header("エフェクト設定")]
	[Tooltip("スポーン前に表示するエフェクトのプレハブ")]
	[SerializeField] private GameObject preSpawnEffectPrefab;
	[Tooltip("スポーンの何秒前にエフェクトを表示するか")]
	[SerializeField] private float preSpawnEffectTime = 1.0f;

	private float spawnTimer;
	private bool isTargetNeeded = false;
	private GameObject lastSpawnedObject;
	private Transform targetr;

	// 生成したエフェクトを一時的に保持するための変数
	private GameObject spawnedEffectInstance;

	void Start()
	{
		if (SpawnObject == null)
		{
			Debug.LogError("スポーンするオブジェクト(SpawnObject)が設定されていません。", this);
			CanSpawn = false;
			return;
		}
		if (SpawnObject.GetComponent<EnemyBase>() == null)
		{
			Debug.LogError("スポーンするオブジェクトが敵(EnemyBase)ではありません。", this);
			CanSpawn = false;
			return;
		}
		if (SpawnObject.GetComponent<TargetingEnemy>() != null)
		{
			isTargetNeeded = true;
			if (TargetObject == null)
			{
				Debug.LogError("ターゲットが必要な敵ですが、ターゲットオブジェクト(TargetObject)が設定されていません。", this);
				CanSpawn = false;
				return;
			}
		}

		if (TargetObject != null)
		{
			targetr = TargetObject.GetComponent<Transform>();
		}
	}

	void Update()
	{
		if (!CanSpawn || SpawnObject == null)
		{
			return;
		}

		if (targetr == null) return;

		// ターゲットの距離を計算する
		float distance = Vector2.Distance(transform.position, targetr.position);

		// ターゲットが遠すぎる場合はスポーン処理を行わない
		if (distance >= SpawnDistance)
		{
			return;
		}

		// ターゲットが近すぎる場合
		if (distance < MinSpawnDistance)
		{
			// ★変更点: 予兆エフェクトがまだ表示されていない場合のみ、処理を中断する
			if (spawnedEffectInstance == null)
			{
				// 近すぎる場合はスポーン処理を中断し、タイマーもリセットする
				// これにより、プレイヤーが一度離れた時に即スポーンするのを防ぐ
				spawnTimer = 0f;

				// (この時点で spawnedEffectInstance は null なので、
				//  元のコードにあったエフェクト削除処理は実質不要)

				return;
			}
			// (予兆エフェクトが表示されている場合は、このif文を無視して処理を続行する)
		}

		// (以降のロジックは変更なし)
		if (IsWatchEnemy == true)
		{
			if (lastSpawnedObject != null)
			{
				return;
			}
			SpawnEnemy();
		}
		else
		{
			SpawnEnemy();
		}
	}

	private void SpawnEnemy()
	{
		spawnTimer += Time.deltaTime;

		// エフェクトをスポーンするタイミングか判定
		if (preSpawnEffectPrefab != null && spawnTimer >= SpawnInterval - preSpawnEffectTime && spawnedEffectInstance == null)
		{
			// エフェクトをスポーンし、そのインスタンスを記憶しておく
			spawnedEffectInstance = Instantiate(preSpawnEffectPrefab, transform.position, Quaternion.identity);
		}

		if (spawnTimer >= SpawnInterval)
		{
			// もし表示されているエフェクトがあれば、それを削除する
			if (spawnedEffectInstance != null)
			{
				Destroy(spawnedEffectInstance, 0.3f);
				spawnedEffectInstance = null; // 消したことを記録
			}

			// オブジェクトをスポーン
			GameObject spawnedObject = Instantiate(SpawnObject, transform.position, Quaternion.identity);

			lastSpawnedObject = spawnedObject;

			if (isTargetNeeded)
			{
				TargetingEnemy targetingEnemy = spawnedObject.GetComponent<TargetingEnemy>();
				if (targetingEnemy != null)
				{
					targetingEnemy.Target = TargetObject;
				}
			}

			spawnTimer = 0.0f;
		}
	}
}