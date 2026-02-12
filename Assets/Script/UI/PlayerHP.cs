using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlayerHP : MonoBehaviour
{
    // =================================================
    // 1. 必要な参照
    // =================================================
    [Header("参照")]
    [SerializeField] private CharacterHealthManager healthManager;
    private GameData _gameData;
    private GameManager _gameManager;

    // =================================================
    // 2. UI設定 (削除済み)
    // =================================================
    // [削除済み] 漢字関連

    // =================================================
    // 3. プレハブ設定 (10個の個別配置方式)
    // =================================================
    [Header("体力段階別プレハブ")]

    // ★修正: 1つの親ではなく、10個の配置場所を登録するリストに変更
    [Tooltip("炎を配置する場所を10個登録してください (Index 0 = HP1, Index 9 = HP10)")]
    [SerializeField] private List<Transform> spawnPoints = new List<Transform>();

    [Tooltip("体力 10～7 のプレハブ")]
    [SerializeField] private GameObject prefabHigh;
    [Tooltip("体力 6～4 のプレハブ")]
    [SerializeField] private GameObject prefabMid;
    [Tooltip("体力 3～1 のプレハブ")]
    [SerializeField] private GameObject prefabLow;
    [Tooltip("体力 0 のプレハブ")]
    [SerializeField] private GameObject prefabZero;

    // 生成された10個の炎インスタンスを管理するリスト
    private List<GameObject> currentInstances = new List<GameObject>();

    // 現在表示しているプレハブの種類（再生成の判定用）
    private GameObject currentPrefabSource;

    // =================================================
    // 4. アニメーション速度設定 (元のまま維持)
    // =================================================
    [Header("アニメーション速度設定")]
    [Tooltip("HP 10 (MAX) の時のアニメーション速度")]
    [SerializeField] private float maxHpSpeed = 1.0f;

    [Tooltip("HP 1 (瀕死) の時のアニメーション速度")]
    [SerializeField] private float minHpSpeed = 0.2f;

    // 内部変数
    private int cachedHP = -1;
    private bool isDeadSceneLoaded = false;
    private const int MAX_HP_CONST = 10;

    // =================================================
    // 初期化 & 更新処理
    // =================================================

    private void Start()
    {
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

        // ★修正: spawnPointsが空だった場合の処理は削除（必ずInspectorで設定してもらう前提）

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

        // プレハブの生成切り替え & 速度更新
        UpdateStatePrefab(currentHP);
    }

    // プレハブの切り替えと速度計算
    private void UpdateStatePrefab(int hp)
    {
        // --- A. 表示すべきプレハブを決定 (元のロジック維持) ---
        GameObject targetPrefab = null;

        if (hp >= 7) targetPrefab = prefabHigh;       // 10-7
        else if (hp >= 4) targetPrefab = prefabMid;   // 6-4
        else if (hp >= 1) targetPrefab = prefabLow;   // 3-1
        else targetPrefab = prefabZero;               // 0

        // --- B. プレハブの種類が変わった場合のみ再生成 (★10個の指定場所に生成) ---
        if (targetPrefab != currentPrefabSource)
        {
            // 1. 既存の炎をすべて削除
            foreach (var obj in currentInstances)
            {
                if (obj != null) Destroy(obj);
            }
            currentInstances.Clear();

            currentPrefabSource = targetPrefab;

            // 2. 指定された10個の場所に、それぞれプレハブを生成
            if (targetPrefab != null)
            {
                for (int i = 0; i < MAX_HP_CONST; i++)
                {
                    // スポーン地点が登録されているか確認
                    if (i < spawnPoints.Count && spawnPoints[i] != null)
                    {
                        // spawnPoints[i] を親にして生成
                        GameObject newInstance = Instantiate(targetPrefab, spawnPoints[i]);

                        // 位置を親（スポーン地点）の中心に合わせる
                        newInstance.transform.localPosition = Vector3.zero;
                        newInstance.transform.localRotation = Quaternion.identity;
                        newInstance.transform.localScale = targetPrefab.transform.localScale;

                        currentInstances.Add(newInstance);
                    }
                    else
                    {
                        // スポーン地点が足りない場合はnullを入れておく（エラー防止）
                        currentInstances.Add(null);
                    }
                }
            }
        }

        // --- C. 体力に応じて表示・非表示 & 速度適用 ---
        float speed = CalculateAnimSpeed(hp);

        for (int i = 0; i < currentInstances.Count; i++)
        {
            if (currentInstances[i] == null) continue;

            // 現在のHPよりインデックスが小さければ表示
            bool isActive = i < hp;
            currentInstances[i].SetActive(isActive);

            if (isActive)
            {
                Animator anim = currentInstances[i].GetComponent<Animator>();
                if (anim != null)
                {
                    anim.speed = speed;
                }
            }
        }
    }

    // HPに応じた速度を計算する関数 (元のロジック維持)
    private float CalculateAnimSpeed(int hp)
    {
        if (hp <= 0) return 0f;
        if (hp >= MAX_HP_CONST) return maxHpSpeed;
        if (hp <= 1) return minHpSpeed;

        float t = (float)(hp - 1) / (MAX_HP_CONST - 1);
        return Mathf.Lerp(minHpSpeed, maxHpSpeed, t);
    }

    // =================================================
    // その他 (元のロジック維持)
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

    // 外部参照エラー回避用
    public void StopCountHP()
    {
        SaveCurrentHP();
    }
}