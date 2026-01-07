using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 【抽象クラス】プレイヤーのスキルの基本処理をまとめた基底クラス。
/// 各スキル（例: FoxSkill, FireSkill）はこのクラスを継承して実装する。
/// </summary>
public abstract class BulletSkill : IPlayerState
{
    // ==============================
    // 参照・依存関係
    // ==============================

    protected Player player;                          // プレイヤー本体
    protected PlayerStateMachine playerStateMachine;  // ステート管理
    protected SkillModeManager skillModeManager;      // スキルモードの切り替え管理
    protected PlayerShooting playerShooting;          // 弾の発射制御

    private GaugeUIManager gaugeUIManager;            // UI上のゲージ表示を管理
    private SkillCutInManager skillCutInManager;      // 暗転演出（カットイン）を制御

    // ==============================
    // スキル共通パラメータ
    // ==============================

    protected float SkillTime = 0;                    // スキル使用時間（継続スキル用）
    private float gauge = 0f;                         // スキルゲージ現在値
    private const float maxGauge = 5f;                // ゲージの最大値
    private const float gaugeIncreasePerSecond = 0.1f;// 自動蓄積スピード
    private const float EnemyDestroyUpGauge = 0.05f;  // 敵撃破によるゲージ上昇量

    private HashSet<EnemyDeathNotifier> registeredNotifiers = new HashSet<EnemyDeathNotifier>();

    // ==============================
    // スキル特性
    // ==============================

    public abstract SkillType SkillType { get; }      // スキルタイプ（enumで識別）
    protected bool isActive = false;                  // スキル発動中フラグ
    protected virtual float maxDuration => 5f;        // スキルの継続時間（継承先で上書き可）

    // ==============================
    // 初期化処理
    // ==============================

    public virtual void Init(Player player, PlayerStateMachine playerStateMachine)
    {
        this.player = player;
        this.playerStateMachine = playerStateMachine;

        // 必要なマネージャー類を取得
        gaugeUIManager = GameObject.Find("gaugeManager").GetComponent<GaugeUIManager>();
        skillModeManager = GameObject.Find("Player").GetComponent<SkillModeManager>();
        skillCutInManager = GameObject.FindFirstObjectByType<SkillCutInManager>(); // カットイン演出を取得

        // 敵登録システムにイベント登録
        if (EnemyCounter.Instance != null)
        {
            // 画面内の敵を初期登録
            foreach (var enemyObj in EnemyCounter.Instance.GetEnemiesInView())
            {
                RegisterEnemy(enemyObj.GetComponent<EnemyBase>());
            }

            // 新しく出現した敵も監視対象にする
            EnemyCounter.Instance.OnEnemyEntered += RegisterEnemy;
        }

        // ゲージUIを初期化
        gaugeUIManager.Init(this);
    }

    // ==============================
    // 毎フレーム更新
    // ==============================

    public virtual void Update()
    {
        // ゲージを自然回復
        IncreaseGauge(Time.deltaTime);

        // デバッグ用：Pキーで即満タン
        if (Input.GetKeyDown(KeyCode.P))
        {
            gauge = maxGauge;
        }
    }

    public void FixedUpdate() { } // 物理更新は未使用（継承先で必要なら使用）

    // ==============================
    // 入力処理（スキル発動チェック）
    // ==============================

    public virtual void HandleInput()
    {
        ShootSkill();
    }

    public virtual void Remove() { }

    // ==============================
    // ゲージ蓄積処理
    // ==============================

    private void IncreaseGauge(float deltaTime)
    {
        gauge += gaugeIncreasePerSecond * deltaTime;
        if (gauge > maxGauge)
            gauge = maxGauge;
    }

    // ==============================
    // スキル発動処理
    // ==============================

    private void ShootSkill()
    {
        // ゲージ満タン時に発動
        if (playerStateMachine.SkillPressed && gauge == maxGauge)
        {
            gauge -= maxGauge;

            // カットイン演出開始（暗転＋スロー演出）
            skillCutInManager?.PlaySkillCutIn(() =>
            {
                // 演出完了後にスキルを発動
                if (IsInstantSkill())
                    Skill();     // 即時発動型（例：弾を即発射）
                else
                    BeginSkill(); // 継続型（例：一定時間強化）
            });
        }
    }

    // ==============================
    // スキルのタイプ判定・開始・終了
    // ==============================

    /// <summary>
    /// 即時スキル（1回で発動して終わる）なら true。
    /// 継続スキルなら false にして BeginSkill / EndSkill で制御。
    /// </summary>
    protected virtual bool IsInstantSkill() => true;

    protected virtual void BeginSkill()
    {
        isActive = true;
        Skill(); // スキル実行
        Debug.Log("Duration Skill Started");
    }

    protected virtual void EndSkill()
    {
        isActive = false;
        OnSkillEnd(); // スキル終了処理
        Debug.Log("Duration Skill Ended");
    }

    // ==============================
    // 敵撃破時ゲージ上昇
    // ==============================

    private void OnEnemyDefeated(EnemyBase enemy)
    {
        gauge += EnemyDestroyUpGauge;
        if (gauge > maxGauge)
            gauge = maxGauge;

        Debug.Log($"BulletSkill: {enemy.name} 撃破 → ゲージ {gauge:F2}");
    }

    // ==============================
    // 敵の死亡イベント登録
    // ==============================

    private void RegisterEnemy(EnemyBase enemy)
    {
        if (enemy == null) return;

        var notifier = enemy.GetComponent<EnemyDeathNotifier>();
        if (notifier != null && !registeredNotifiers.Contains(notifier))
        {
            notifier.OnEnemyDied += OnEnemyDefeated;
            registeredNotifiers.Add(notifier);
            Debug.Log($"BulletSkill: {enemy.name} を監視対象に登録");
        }
    }

    // ==============================
    // スキル本体（継承先で実装）
    // ==============================

    /// <summary> スキルのメイン処理（継承先でオーバーライド） </summary>
    protected virtual void Skill() { }

    /// <summary> スキル終了時の処理（継承先でオーバーライド） </summary>
    protected virtual void OnSkillEnd() { }

    /// <summary> サブスキル（派生技など） </summary>
    protected virtual void SubSkill() { }

    // ==============================
    // 外部アクセス用
    // ==============================

    public float GetGauge() => gauge;
    public void SetGauge(float value) { gauge = value; }
    public void SetPlayerShooting(PlayerShooting shooting) => playerShooting = shooting;
}