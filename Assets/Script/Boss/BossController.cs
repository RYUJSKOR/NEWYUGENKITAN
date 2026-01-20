using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

// アニメーションのenum定義
public enum ArmAnimationState
{
    Default,
    Fist,
    OpenHand
}

public class BossController : MonoBehaviour
{
    #region 変数宣言
    [Header("ターゲット")]
    public Transform playerTransform;
    [Header("ボスパーツの設定")]
    public GameObject bodyObject;
    public GameObject leftArmObject;
    public GameObject rightArmObject;
    public Transform leftArmRestPosition;
    public Transform rightArmRestPosition;
    [Header("待機モーション設定")]
    public float idleFloatSpeed = 1.5f;
    public float idleFloatHorizontalRadius = 0.3f;
    public float idleFloatVerticalRadius = 0.15f;
    [Header("攻撃設定")]
    public float groundLevelY = 0f;
    [Header("フェーズ設定")]
    public List<BossPhase> phases = new List<BossPhase>();
    [Header("スタン設定")]
    public float stunDuration = 10.0f;
    [Header("演出用設定")]
    public GameObject deathModelObject;
    public GameObject bodyMeshObject;
    public GameObject transitionModelObject;
    public Transform escapePoint;
    public GameObject stageExitTriggerPrefab;
    public Transform stageExitTriggerSpawnPoint;
    public GameObject ladderPrefab;
    public Transform ladderSpawnPoint;
    [Header("撃破エフェクト設定")]
    public float initialExplosionDelay = 0.5f;
    public float timeBetweenSmallExplosions = 0.2f;
    public GameObject smallExplosionPrefab;
    public int numberOfSmallExplosions = 10;
    public float finalExplosionDelay = 1.5f;
    public GameObject finalExplosionPrefab;

    [Header("マテリアルカラー設定")]
    [Tooltip("色を変更するシェーダーのプロパティ名")]
    public string colorPropertyName = "_MainColor";

    [Tooltip("フェーズの色変更から除外するRenderer（仮面など）")]
    public List<Renderer> excludeFromColorChange = new List<Renderer>();

    private BossStateMachine stateMachine;
    private List<BossWeakPoint> weakPoints = new List<BossWeakPoint>();
    private int destroyedWeakPointsCount = 0;
    public CharacterHealthManager bodyHealthManager;
    private int currentPhaseIndex = 0;
    private int currentAttackPatternIndex = 0;
    private Coroutine activeAttackCoroutine = null;
    private BossAttackPattern runningAttackPattern = null;
    private List<Transform> explosionPoints = new List<Transform>();
    private List<MeshFilter> bodyMeshFilters = new List<MeshFilter>();

    private Animator animator;
    private Animator leftArmAnimator;
    private Animator rightArmAnimator;

    public BossWeakPoint leftArmWeakPoint;
    public BossWeakPoint rightArmWeakPoint;
    public BossPartFlash leftArmFlash;
    public BossPartFlash rightArmFlash;
    public BossPartFlash bodyFlash;

    private Renderer[] bodyRenderers;
    private Renderer[] leftArmRenderers;
    private Renderer[] rightArmRenderers;
    private MaterialPropertyBlock propBlock;

    public event Action OnBossDefeated;
    public event Action OnDeathSequenceStart;

    public bool IsAttacking { get; private set; } = false;
    public bool IsLeftArmAttacking { get; private set; } = false;
    public bool IsRightArmAttacking { get; private set; } = false;
    #endregion

    #region 有効と無効時
    private void OnEnable()
    {
        if (BossGameManager.Instance != null)
        {
            BossGameManager.Instance.RegisterBoss(this);
        }

        if (bodyHealthManager != null)
        {
            bodyHealthManager.OnDamageTaken += OnBodyDamaged;
        }

        leftArmWeakPoint.OnDamaged += OnWeakPointDamaged;
        rightArmWeakPoint.OnDamaged += OnWeakPointDamaged;
    }

    private void OnDisable()
    {
        if (BossGameManager.Instance != null)
        {
            BossGameManager.Instance.UnregisterBoss();
        }
        foreach (var wp in weakPoints)
        {
            if (wp != null)
                wp.OnDestroyed -= OnWeakPointDestroyed;
        }
        if (bodyHealthManager != null)
        {
            bodyHealthManager.OnDeath -= HandleBossDeath;
            bodyHealthManager.OnDamageTaken -= CheckForPhaseTransition;
        }

        if (bodyHealthManager != null)
        {
            bodyHealthManager.OnDamageTaken -= OnBodyDamaged;
        }

        leftArmWeakPoint.OnDamaged -= OnWeakPointDamaged;
        rightArmWeakPoint.OnDamaged -= OnWeakPointDamaged;
    }

    #endregion

    #region 初期化
    void Start()
    {
        propBlock = new MaterialPropertyBlock();

        stateMachine = GetComponent<BossStateMachine>();
        animator = GetComponent<Animator>();

        if (leftArmObject != null)
        {
            leftArmAnimator = leftArmObject.GetComponentInChildren<Animator>();
            // GetFilteredRenderers を使う
            leftArmRenderers = GetFilteredRenderers(leftArmObject);
        }
        if (rightArmObject != null)
        {
            rightArmAnimator = rightArmObject.GetComponentInChildren<Animator>();
            // GetFilteredRenderers を使う
            rightArmRenderers = GetFilteredRenderers(rightArmObject);
        }

        if (bodyObject != null)
        {
            bodyHealthManager = bodyObject.GetComponent<CharacterHealthManager>();
            bodyMeshFilters = bodyObject.GetComponentsInChildren<MeshFilter>().ToList();
        }

        // GetFilteredRenderers を使う
        if (bodyMeshObject != null)
        {
            bodyRenderers = GetFilteredRenderers(bodyMeshObject);
        }
        else if (bodyObject != null) // フォールバック
        {
            bodyRenderers = GetFilteredRenderers(bodyObject);
            if (bodyRenderers != null && bodyRenderers.Length > 0)
                Debug.LogWarning("BossController: bodyMeshObjectが設定されていません。bodyObjectからRendererを検索しました。");
        }

        GetComponentsInChildren(true, weakPoints);

        if (BossGameManager.Instance != null && BossGameManager.Instance.HasSavedData)
        {
            currentPhaseIndex = BossGameManager.Instance.SavedBossPhase;
            Debug.Log("保存されたボスのフェーズを復元しました: " + currentPhaseIndex);
            if (bodyHealthManager != null)
            {
                bodyHealthManager.SetHealth(BossGameManager.Instance.SavedBossHealth);
                Debug.Log("保存されたボスの体力を復元しました: " + bodyHealthManager.GetHealth());
            }
        }

        foreach (var wp in weakPoints)
        {
            wp.OnDestroyed += OnWeakPointDestroyed;
        }
        if (bodyHealthManager != null)
        {
            bodyHealthManager.OnDeath += HandleBossDeath;
            bodyHealthManager.OnDamageTaken += CheckForPhaseTransition;
        }

        foreach (Transform child in GetComponentsInChildren<Transform>())
        {
            if (child.name.Contains("ExplosionPoint"))
            {
                explosionPoints.Add(child);
            }
        }

        InitializeBoss();

        UpdatePhaseColor();
    }

    /// <summary>
    /// GameObjectからRendererを取得し、除外リストに基づいてフィルタリングする
    /// </summary>
    private Renderer[] GetFilteredRenderers(GameObject targetObject)
    {
        if (targetObject == null)
        {
            return new Renderer[0]; // 空の配列を返す
        }

        // 1. まず全ての子Rendererを取得
        List<Renderer> allRenderers = targetObject.GetComponentsInChildren<Renderer>().ToList();

        // 2. 除外リストが設定されていれば、それらの要素をリストから削除
        if (excludeFromColorChange != null && excludeFromColorChange.Count > 0)
        {
            // LINQのExceptを使用して、除外リストに含まれるものを除外する
            return allRenderers.Except(excludeFromColorChange).ToArray();
        }
        else
        {
            // 除外リストが空なら、そのまま全て返す
            return allRenderers.ToArray();
        }
    }

    private void InitializeBoss()
    {
        bodyObject.GetComponent<BoxCollider>().enabled = false;
        stateMachine.ChangeState(new BossIdleState(stateMachine, this));
        UpdateHealthGate();
    }

    #endregion

    #region バインド関数
    private void HandleBossDeath()
    {
        Debug.Log("ボス本体の体力が0になりました！ゲームクリア！");
        IsAttacking = true;

        OnDeathSequenceStart?.Invoke();

        if (bodyObject != null)
        {
            bodyObject.SetActive(true);
            var bodyCollider = bodyObject.GetComponent<Collider>();
            if (bodyCollider != null) bodyCollider.enabled = false;
        }
        stateMachine.ChangeState(new BossDeathState(this));

        StartCoroutine(DeathAnimationSequence());
    }

    private IEnumerator DeathAnimationSequence()
    {
        if (bodyMeshObject != null) bodyMeshObject.SetActive(false);
        if (leftArmObject != null) leftArmObject.SetActive(false);
        if (rightArmObject != null) rightArmObject.SetActive(false);


        if (deathModelObject != null)
        {
            deathModelObject.SetActive(true);
            Animator deathAnimator = deathModelObject.GetComponent<Animator>();
            if (deathAnimator != null)
            {
                deathAnimator.SetTrigger("Death");
            }
        }

        float deathAnimationLength = 7f;
        yield return new WaitForSeconds(deathAnimationLength);

        if (CameraShakeManager.instance != null)
        {
            CameraShakeManager.instance.TriggerShake(0.5f, 0.4f);
        }

        OnBossDefeated?.Invoke();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.BossClear();
            gameObject.SetActive(false);
        }
    }


    private void OnWeakPointDamaged(BossWeakPoint weakPoint)
    {
        // (前回の修正) 現在のフェーズの色を取得
        Color currentPhaseColor = phases[currentPhaseIndex].phaseColor;

        if (weakPoint == leftArmWeakPoint)
        {
            leftArmFlash.FlashRed(currentPhaseColor);
        }
        else if (weakPoint == rightArmWeakPoint)
        {
            rightArmFlash.FlashRed(currentPhaseColor);
        }
    }

    private void OnBodyDamaged()
    {
        if (bodyFlash != null)
        {
            // (前回の修正) 現在のフェーズの色を取得して渡す
            Color currentPhaseColor = phases[currentPhaseIndex].phaseColor;
            bodyFlash.FlashRed(currentPhaseColor);
        }
    }
    #endregion

    #region その他のメソッド

    public void SetArmAnimation(bool isLeftArm, ArmAnimationState state)
    {
        Animator targetAnimator = isLeftArm ? leftArmAnimator : rightArmAnimator;
        if (targetAnimator != null)
        {
            targetAnimator.SetInteger("HandState", (int)state);
        }
    }

    void OnWeakPointDestroyed(BossWeakPoint destroyedWp)
    {
        destroyedWeakPointsCount++;

        if (activeAttackCoroutine != null)
        {
            runningAttackPattern?.Cleanup(this);
            StopCoroutine(activeAttackCoroutine);
            activeAttackCoroutine = null;
            runningAttackPattern = null;
            IsAttacking = false;
            IsLeftArmAttacking = false;
            IsRightArmAttacking = false;
        }

        SetArmAnimation(true, ArmAnimationState.Default);
        SetArmAnimation(false, ArmAnimationState.Default);

        if (destroyedWeakPointsCount >= weakPoints.Count)
        {
            StartStun();
        }
        else
        {
            if (destroyedWp == leftArmWeakPoint)
            {
                animator.SetTrigger("LeftArmDestroyed");
            }
            else if (destroyedWp == rightArmWeakPoint)
            {
                animator.SetTrigger("RightArmDestroyed");
            }

            stateMachine.ChangeState(new BossIdleState(stateMachine, this));
        }
    }

    private void StartStun()
    {
        animator.ResetTrigger("LeftArmDestroyed");
        animator.ResetTrigger("RightArmDestroyed");
        animator.SetTrigger("Stun");

        if (bodyObject != null)
        {
            bodyObject.SetActive(true);
            SetBodyBoxCollidersEnabled(true);
        }
        stateMachine.ChangeState(new BossStunState(stateMachine, this, stunDuration));
    }

    public void EndStunAndRegenerateArms()
    {
        animator.SetTrigger("Recover");

        if (bodyObject != null)
        {
            SetBodyBoxCollidersEnabled(false);
        }
        ResetArm(leftArmObject, leftArmRestPosition);
        ResetArm(rightArmObject, rightArmRestPosition);

        SetArmAnimation(true, ArmAnimationState.Default);
        SetArmAnimation(false, ArmAnimationState.Default);

        destroyedWeakPointsCount = 0;
        ResetAllFlash();
        stateMachine.ChangeState(new BossIdleState(stateMachine, this));
    }

    public void ResetAllFlash()
    {
        if (bodyFlash != null && leftArmFlash != null && rightArmFlash != null)
        {
            // (前回の修正) 現在のフェーズの色（リセット先の色）を取得
            Color currentPhaseColor = phases[currentPhaseIndex].phaseColor;

            bodyFlash.ResetFlash(currentPhaseColor);
            leftArmFlash.ResetFlash(currentPhaseColor);
            rightArmFlash.ResetFlash(currentPhaseColor);
        }
        else
        {
            Debug.LogError("アームかボディのフラッシュが設定できていません。");
        }
    }

    private void ResetArm(GameObject armObject, Transform restPosition)
    {
        if (armObject == null) return;
        Rigidbody armRb = armObject.GetComponent<Rigidbody>();
        if (armRb != null)
        {
            armRb.linearVelocity = Vector3.zero;
            armRb.angularVelocity = Vector3.zero;
        }
        armObject.transform.position = restPosition.position;
        armObject.SetActive(true);
        CharacterHealthManager armHealth = armObject.GetComponent<CharacterHealthManager>();
        if (armHealth != null)
        {
            armHealth.ResetHealth();
        }
    }

    public void ExecuteNextAttack()
    {
        if (IsAttacking || phases.Count <= currentPhaseIndex) return;

        var currentPhaseAttackPatterns = phases[currentPhaseIndex].attackPatterns;
        if (currentPhaseAttackPatterns.Count == 0) return;

        BossAttackPattern currentAttack = currentPhaseAttackPatterns[currentAttackPatternIndex];

        runningAttackPattern = currentAttack;
        currentAttack.Execute(this);

        currentAttackPatternIndex = (currentAttackPatternIndex + 1) % currentPhaseAttackPatterns.Count;
    }

    public void SetAttackingState(bool isAttacking, bool isLeftArm)
    {
        this.IsAttacking = isAttacking;
        if (isLeftArm) this.IsLeftArmAttacking = isAttacking;
        else this.IsRightArmAttacking = isAttacking;

        if (!isAttacking)
        {
            if (!IsLeftArmAttacking && !IsRightArmAttacking)
            {
                runningAttackPattern = null;
            }
        }
    }

    public void SetBothArmsAttacking(bool isAttacking)
    {
        this.IsAttacking = isAttacking;
        this.IsLeftArmAttacking = isAttacking;
        this.IsRightArmAttacking = isAttacking;
    }

    public void RunAttackCoroutine(IEnumerator coroutine)
    {
        activeAttackCoroutine = StartCoroutine(coroutine);
    }

    private void CheckForPhaseTransition()
    {
        if (currentPhaseIndex >= phases.Count - 1 || bodyObject.activeSelf == false) return;

        float maxHealth = bodyHealthManager.GetMaxHealth();
        float currentHealth = bodyHealthManager.GetHealth();
        float threshold = phases[currentPhaseIndex].transitionHealthThreshold;

        const float tolerance = 0.001f;
        if ((currentHealth / maxHealth) <= (threshold + tolerance))
        {
            TransitionToNextPhase();
        }
    }

    private void TransitionToNextPhase()
    {
        currentPhaseIndex++;
        currentAttackPatternIndex = 0;
        stateMachine.ChangeState(new BossPhaseTransitionState(stateMachine, this));

        UpdatePhaseColor();
        UpdateHealthGate();
    }

    /// <summary>
    /// 現在のフェーズに基づいてボスのマテリアルの色を更新する
    /// </summary>
    private void UpdatePhaseColor()
    {
        if (phases != null && phases.Count > currentPhaseIndex)
        {
            Color newColor = phases[currentPhaseIndex].phaseColor;
            Debug.Log($"[BossController] フェーズ {currentPhaseIndex} の色 {newColor} に変更します。");

            if (propBlock == null) propBlock = new MaterialPropertyBlock();

            propBlock.SetColor(colorPropertyName, newColor);

            // ▼▼▼ 各Renderer配列をループして適用するように変更 ▼▼▼

            // Body Renderers
            if (bodyRenderers != null && bodyRenderers.Length > 0)
            {
                foreach (var rend in bodyRenderers)
                {
                    if (rend != null) rend.SetPropertyBlock(propBlock);
                }
            }

            // Left Arm Renderers
            if (leftArmRenderers != null && leftArmRenderers.Length > 0)
            {
                foreach (var rend in leftArmRenderers)
                {
                    if (rend != null) rend.SetPropertyBlock(propBlock);
                }
            }

            // Right Arm Renderers
            if (rightArmRenderers != null && rightArmRenderers.Length > 0)
            {
                foreach (var rend in rightArmRenderers)
                {
                    if (rend != null) rend.SetPropertyBlock(propBlock);
                }
            }
            // ▲▲▲ ▲▲▲
        }
        else
        {
            Debug.LogWarning($"[BossController] Phase {currentPhaseIndex} の設定が phases リストに存在しません。");
        }
    }

    private void UpdateHealthGate()
    {
        if (bodyHealthManager == null) return;

        if (currentPhaseIndex < phases.Count - 1)
        {
            float threshold = phases[currentPhaseIndex].transitionHealthThreshold;
            float gateValue = bodyHealthManager.GetMaxHealth() * threshold;

            bodyHealthManager.HealthGate = gateValue;
            Debug.Log($"<color=cyan>[BossController] 次のヘルスゲートをHP: {gateValue} ({(threshold * 100)}%) に設定しました。</color>");
        }
        else
        {
            bodyHealthManager.HealthGate = 0f;
            Debug.Log("<color=cyan>[BossController] 最終フェーズのため、ヘルスゲートを解除しました。</color>");
        }
    }

    public void BeginPhaseTransition(float duration)
    {
        if (bodyObject != null)
        {
            bodyObject.SetActive(true);
            var bodyHealth = bodyObject.GetComponent<CharacterHealthManager>();
            if (bodyHealth != null)
            {
                bodyHealth.ActivateInvincibility(duration);
            }
        }
    }

    /// <summary>
    /// KOKOGA 問題になっているところ
    /// </summary>
    public void SpawnPhaseExitObjects()
    {
        if (stageExitTriggerPrefab != null && stageExitTriggerSpawnPoint != null)
        {
            GameObject triggerInstance = Instantiate(stageExitTriggerPrefab, stageExitTriggerSpawnPoint.position, stageExitTriggerSpawnPoint.rotation);
            StageExitTrigger triggerScript = triggerInstance.GetComponent<StageExitTrigger>();
            if (phases.Count > currentPhaseIndex)
            {
                string sceneToLoad = phases[currentPhaseIndex].nextSceneName;
                if (triggerScript != null && !string.IsNullOrEmpty(sceneToLoad))
                {
                    //triggerScript.nextSceneName = sceneToLoad;
                }
            }
        }
        if (ladderPrefab != null && ladderSpawnPoint != null)
        {
            Instantiate(ladderPrefab, ladderSpawnPoint.position, ladderSpawnPoint.rotation);
        }
    }

    public int GetCurrentPhase()
    {
        return currentPhaseIndex;
    }

    public float GetBodyHealth()
    {
        if (bodyHealthManager != null)
        {
            return bodyHealthManager.GetHealth();
        }
        return 0;
    }
    public float GetCurrentPhaseAttackInterval()
    {
        if (phases.Count > currentPhaseIndex)
        {
            return phases[currentPhaseIndex].attackInterval;
        }
        return 5.0f; // 安全のためのデフォルト値
    }

    public void SetBodyBoxCollidersEnabled(bool enabled)
    {
        if (bodyObject == null) return;

        BoxCollider[] colliders = bodyObject.GetComponentsInChildren<BoxCollider>(true);
        foreach (var collider in colliders)
        {
            collider.enabled = enabled;
        }
    }

    public void SetAnimationTrigger(string triggerName)
    {
        if (animator != null)
        {
            animator.SetTrigger(triggerName);
        }
    }


    public void ExecuteSpecificAttack(BossAttackPattern attackPattern)
    {
        if (IsAttacking || attackPattern == null) return;

        runningAttackPattern = attackPattern;
        attackPattern.Execute(this);
    }

    public void PlayTransitionAnimation(string triggerName)
    {
        if (bodyMeshObject != null)
        {
            bodyMeshObject.SetActive(false);
        }

        if (transitionModelObject != null)
        {
            transitionModelObject.SetActive(true);
            Animator transitionAnimator = transitionModelObject.GetComponent<Animator>();
            if (transitionAnimator != null)
            {
                transitionAnimator.SetTrigger(triggerName);
            }
        }
    }
    #endregion
}