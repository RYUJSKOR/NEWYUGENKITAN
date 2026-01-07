using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlayerHPViewer : MonoBehaviour
{
	// =================================================
	// 1. 必要な参照
	// =================================================
	[Header("参照")]
	[SerializeField] private CharacterHealthManager healthManager;
	private GameData _gameData;
	private GameManager _gameManager;

	// =================================================
	// 2. UI設定 (漢数字)
	// =================================================
	[Header("漢数字UI")]
	[SerializeField] private Image kanjiImage;
	[Tooltip("HPに対応するスプライト (Index 0 = HP0, Index 10 = HP10)")]
	[SerializeField] private List<Sprite> kanjiSprites = new List<Sprite>();

	// =================================================
	// 3. プレハブ設定 (生成・削除方式)
	// =================================================
	[Header("体力段階別プレハブ")]
	[Tooltip("生成する親の位置（空欄ならこのオブジェクト直下）")]
	[SerializeField] private Transform spawnPoint;

	[Tooltip("体力 10～7 のプレハブ")]
	[SerializeField] private GameObject prefabHigh;
	[Tooltip("体力 6～4 のプレハブ")]
	[SerializeField] private GameObject prefabMid;
	[Tooltip("体力 3～1 のプレハブ")]
	[SerializeField] private GameObject prefabLow;
	[Tooltip("体力 0 のプレハブ")]
	[SerializeField] private GameObject prefabZero;

	// 現在生成されているインスタンス
	private GameObject currentInstance;
	// 現在表示しているプレハブの種類（再生成の判定用）
	private GameObject currentPrefabSource;

	// =================================================
	// 4. アニメーション速度設定 (自動計算)
	// =================================================
	[Header("アニメーション速度設定")]
	[Tooltip("HP 10 (MAX) の時のアニメーション速度")]
	[SerializeField] private float maxHpSpeed = 1.0f; // 例: 1.0 (通常)

	[Tooltip("HP 1 (瀕死) の時のアニメーション速度")]
	[SerializeField] private float minHpSpeed = 0.2f; // 例: 0.2 (スロー)

	// 内部変数
	private int cachedHP = -1;
	private bool isDeadSceneLoaded = false;
	private const int MAX_HP_CONST = 10; // 計算基準となる最大HP

	// =================================================
	// 初期化 & 更新処理
	// =================================================

	private void Start()
	{
		// 参照の自動取得
		if (healthManager == null)
		{
			var player = FindObjectOfType<Player>();
			if (player != null) healthManager = player.GetComponent<CharacterHealthManager>();
		}

		_gameData = FindObjectOfType<GameData>();
		_gameManager = FindObjectOfType<GameManager>();

		if (healthManager != null)
		{
			healthManager.OnDeath += OnPlayerDeath;
		}

		// SpawnPointが未設定なら自分自身を親にする
		if (spawnPoint == null) spawnPoint = this.transform;

		// 初回更新
		UpdateUI(forceUpdate: true);
	}

	private void Update()
	{
		if (healthManager == null) return;
		UpdateUI(forceUpdate: false);
	}

	// =================================================
	// メイン更新ロジック
	// =================================================

	private void UpdateUI(bool forceUpdate)
	{
		float hpRaw = healthManager.GetHealth();
		int currentHP = Mathf.Clamp(Mathf.FloorToInt(hpRaw), 0, MAX_HP_CONST);

		if (!forceUpdate && currentHP == cachedHP) return;
		cachedHP = currentHP;

		// 1. 漢数字画像の更新
		UpdateKanjiImage(currentHP);

		// 2. プレハブの生成切り替え & 速度更新
		UpdateStatePrefab(currentHP);
	}

	private void UpdateKanjiImage(int hp)
	{
		if (kanjiImage == null) return;

		if (hp >= 0 && hp < kanjiSprites.Count)
		{
			kanjiImage.sprite = kanjiSprites[hp];
			kanjiImage.gameObject.SetActive(true);
		}
		else
		{
			kanjiImage.gameObject.SetActive(false);
		}
	}

	// プレハブの切り替えと速度計算
	private void UpdateStatePrefab(int hp)
	{
		// --- A. 表示すべきプレハブを決定 ---
		GameObject targetPrefab = null;

		if (hp >= 7) targetPrefab = prefabHigh;  // 10-7
		else if (hp >= 4) targetPrefab = prefabMid;   // 6-4
		else if (hp >= 1) targetPrefab = prefabLow;   // 3-1
		else targetPrefab = prefabZero;  // 0

		// --- B. プレハブの種類が変わった場合のみ再生成 ---
		if (targetPrefab != currentPrefabSource)
		{
			// 古いものを削除
			if (currentInstance != null)
			{
				Destroy(currentInstance);
			}

			currentPrefabSource = targetPrefab;

			// 新しいものを生成
			if (targetPrefab != null)
			{
				currentInstance = Instantiate(targetPrefab, spawnPoint);
				// 位置や回転のリセット（必要に応じて）
				currentInstance.transform.localPosition = Vector3.zero;
				currentInstance.transform.localRotation = Quaternion.identity;
				currentInstance.transform.localScale = targetPrefab.transform.localScale;
			}
		}

		// --- C. 速度の計算と適用 ---
		if (currentInstance != null)
		{
			Animator anim = currentInstance.GetComponent<Animator>();
			if (anim != null)
			{
				float speed = CalculateAnimSpeed(hp);
				anim.speed = speed;
			}
		}
	}

	// HPに応じた速度を計算する関数
	private float CalculateAnimSpeed(int hp)
	{
		// HPが0の場合は速度0（または停止）にするなら 0f を返す
		if (hp <= 0) return 0f;

		// HPが10以上なら最大速度
		if (hp >= MAX_HP_CONST) return maxHpSpeed;

		// HPが1なら最小速度
		if (hp <= 1) return minHpSpeed;

		// その間（9～2）を線形補間で計算
		// t は 0.0(HP1) ～ 1.0(HP10) の割合
		float t = (float)(hp - 1) / (MAX_HP_CONST - 1);

		// Lerpで速度を決定
		return Mathf.Lerp(minHpSpeed, maxHpSpeed, t);
	}

	// =================================================
	// その他
	// =================================================

	private void OnPlayerDeath()
	{
		if (isDeadSceneLoaded) return;
		isDeadSceneLoaded = true;

		if (_gameData != null) _gameData.savePlayerHP(0);

		var fader = FindObjectOfType<GameOverFader>();
		if (fader != null) fader.Play();
		else if (_gameManager != null) _gameManager.ChangeOverScene();
	}

	public void SaveCurrentHP()
	{
		if (_gameData != null && healthManager != null)
		{
			int hp = Mathf.Clamp(Mathf.FloorToInt(healthManager.GetHealth()), 0, 10);
			_gameData.savePlayerHP(hp);
		}
	}

	public static string FormatHP(float currentHP, float MaxHP)
	{
		int cur = Mathf.Max(0, Mathf.FloorToInt(currentHP));
		int max = 10;
		return $"{cur}/{max}";
	}
}