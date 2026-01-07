using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DemonAnimation : MonoBehaviour
{
    [Header("半透明球（本体）")]
    [SerializeField] private GameObject spherePrefab;
    [SerializeField] private GameObject spherePrefab2;

    [Header("爆発エフェクト")]
    [SerializeField] private ParticleSystem explosionEffect;
    [SerializeField] private ParticleSystem smallexplosionEffect;

    [Header("ジャンプ軌道エフェクト（横線）")]
    [SerializeField] private GameObject trailPrefab;
    [SerializeField] private float trailInterval = 0.08f;

    // --- 改良用設定 ---
    [SerializeField] private float pathRecordInterval = 0.02f; // 軌道記録間隔
    [SerializeField] private int maxPathCount = 60;            // 履歴保存上限
    [SerializeField] private float backDistance = 0.4f;        // プレイヤー後方に出す距離

    private float trailTimer = 0f;
    public bool isSpecialMove = false;

    // List ではなく Queue を使う（Enqueue / Dequeue を使うため）
    private Queue<Vector3> jumpPath = new Queue<Vector3>();

    private GameObject sphereInstance;
    private GameObject sphereInstance2;    // 従来の球
    private Camera mainCam;

    private void Awake()
    {
        // カメラを自動取得
        mainCam = Camera.main;
    }

    /// <summary>
    /// カメラに spherePrefab を配置（自動呼び出し）
    /// </summary>
    private void ShowSphereOnCamera()
    {
        if (spherePrefab == null || sphereInstance != null || mainCam == null)
            return;

        // カメラの正面に作成
        Vector3 pos = mainCam.transform.position + mainCam.transform.forward * 0.5f;

        sphereInstance = Instantiate(
            spherePrefab,
            pos,
            mainCam.transform.rotation,   // カメラと同じ向き
            mainCam.transform             // カメラの子にする → 自動追従
        );
    }

    /// <summary>
    /// spherePrefab2 のみ従来通り radius で生成
    /// さらに spherePrefab（巨大エフェクト）をカメラに表示
    /// </summary>
    public void PlayExplosionThenShowSphere(Transform parent, Vector3 position, float radius, bool followParent)
    {
        // カメラに spherePrefab を表示
        ShowSphereOnCamera();

        // spherePrefab2 は今までどおり
        if (spherePrefab2 != null && sphereInstance2 == null)
        {
            sphereInstance2 = Instantiate(
                spherePrefab2,
                position,
                Quaternion.identity,
                followParent ? parent : null
            );

            float diameter = radius * 2f;
            sphereInstance2.transform.localScale = new Vector3(diameter, diameter, diameter);
        }

        // 爆発エフェクト
        if (explosionEffect != null)
        {
            ParticleSystem explosionInstance = Instantiate(
                explosionEffect,
                position,
                Quaternion.identity,
                followParent ? parent : null
            );
            explosionInstance.Play();
            Destroy(explosionInstance.gameObject, explosionInstance.main.duration);
        }
    }

    /// <summary>
    /// spherePrefab2 のみ radius 更新
    /// </summary>
    public void UpdateVisual(Vector3 position, float radius)
    {
        if (sphereInstance2 == null) return;

        float diameter = radius * 2f;
        sphereInstance2.transform.localScale = new Vector3(diameter, diameter, diameter);
    }

    /// <summary>
    /// 全部削除
    /// </summary>
    public void Hide()
    {
        if (sphereInstance != null)
        {
            Destroy(sphereInstance);
            sphereInstance = null;
        }

        if (sphereInstance2 != null)
        {
            Destroy(sphereInstance2);
            sphereInstance2 = null;
        }
    }

    /// <summary>
    /// ランダム小爆発
    /// </summary>
    public void SpawnRandomExplosion(Vector3 center, float radius)
    {
        if (smallexplosionEffect == null) return;

        Vector3 randomPos = center + Random.insideUnitSphere * radius;
        randomPos.y = center.y;

        ParticleSystem fx = Instantiate(smallexplosionEffect, randomPos, Quaternion.identity);
        fx.Play();
        Destroy(fx.gameObject, fx.main.duration);
    }

    /// <summary>
    /// ジャンプ開始時に呼ぶ
    /// </summary>
    public void OnJumpStart()
    {
        jumpPath.Clear();
        trailTimer = 0f;
    }

    public void UpdateJumpTrail(bool isJumping, Transform player)
    {
        if (!isJumping) return;
        if (!isSpecialMove) return; // ← 必殺技中のみ

        // ---- トレイル生成間隔チェック ----
        trailTimer += Time.deltaTime;
        if (trailTimer < trailInterval) return;
        trailTimer = 0f;

        // 足元位置取得
        Vector3 spawnPos = GetFootPosition(player);

        // 生成
        if (trailPrefab != null)
        {
            GameObject obj = Instantiate(trailPrefab, spawnPos, Quaternion.identity);
            Destroy(obj, 0.3f);
        }
    }

    private Vector3 GetFootPosition(Transform player)
    {
        float footOffset = 0f;

        CapsuleCollider col = player.GetComponent<CapsuleCollider>();
        if (col != null)
        {
            footOffset = col.height / 2f - col.radius;
        }

        return player.position + Vector3.down * footOffset;
    }
}