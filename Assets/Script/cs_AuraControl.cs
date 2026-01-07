/*
 * 作成：23CU03330 橋本大和
 * 用途：パーティクルシステム。
 *       敵に向けて伸びていくビーム型パーティクル
 */

using System;
using UnityEngine;

public class cs_AuraControl : MonoBehaviour
{
    [Header("ターゲット")]
    public Transform target;

    [Header("伸びる速度")]
    public float expandSpeed = 30f;

    [Header("到達後にビームを止めるまでの時間")]
    public float stopDelay = 0.5f;

    [Header("爆発エフェクト（Prefab）")]
    public GameObject explosionEffect;

    [Header("爆発エフェクトの削除遅延")]
    public float explosionDestroyDelay = 1f;

    [Header("ビームの開始位置（Player）")]
    public Transform player;

    // 到達した瞬間に通知するコールバック
    public Action onHit;

    private ParticleSystem ps;
    private float currentHeight = 0f;
    private float targetDistance = 0f;

    private bool reached = false;
    private bool exploded = false;

    private float stopTimer = 0f;

    private GameObject spawnedExplosion;
    private ParticleSystem ExplosionEffect;

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
    }

    void Update()
    {
        bool hasTarget = target != null;

        // Player がいる場合は常に Player 中心へ固定
        if (player != null)
        {
            transform.position = player.position;
        }

        // target が生きてる時だけ方向を計算
        if (hasTarget)
        {
            Vector3 dir = target.position - transform.position;
            targetDistance = dir.magnitude;

            Quaternion look = Quaternion.LookRotation(dir);
            Quaternion fix = Quaternion.Euler(90, 0, 0);
            transform.rotation = look * fix;
        }

        // 到達後は target が死んでも止め処理が動く！
        if (reached)
        {
            stopTimer += Time.deltaTime;

            if (stopTimer >= stopDelay)
            {
                StopBeamEmission();
                StopExplosionEffect();
                return;
            }
        }

        // target が死んだ後はこれ以上伸びない
        if (!hasTarget) return;

        if (!reached)
        {
            currentHeight += expandSpeed * Time.deltaTime;

            if (currentHeight >= targetDistance)
            {
                currentHeight = targetDistance;
                reached = true;

                onHit?.Invoke();

                if (!exploded)
                {
                    exploded = true;
                    SpawnExplosion();
                }
            }
        }
        else
        {
            // ★ 到達後：常に target までの距離に合わせる
            currentHeight = targetDistance;
        }

        var shape = ps.shape;
        shape.scale = new Vector3(shape.scale.x, currentHeight, shape.scale.z);
        shape.position = new Vector3(0, currentHeight / 2f, 0);
    }


    // ビームの停止
    private void StopBeamEmission()
    {
        Debug.Log("cs_AuraControl StopBeamEmission called!");
        var emission = ps.emission;
        emission.enabled = false;

        ParticleSystem[] beamChildren = GetComponentsInChildren<ParticleSystem>();
        foreach (var c in beamChildren)
        {
            c.Stop();
        }

        Destroy(gameObject, explosionDestroyDelay);
    }

    // 爆発エフェクト生成
    private void SpawnExplosion()
    {
        if (explosionEffect == null) return;

        spawnedExplosion = Instantiate(explosionEffect, target.position, Quaternion.identity);
        ExplosionEffect = spawnedExplosion.GetComponent<ParticleSystem>();
    }

    // 爆発エフェクトの停止 & 遅延削除
    private void StopExplosionEffect()
    {

        Debug.Log("cs_AuraControl StopExplosionEffect called!");
        if (spawnedExplosion == null) return;

        ParticleSystem[] explosionChildren = spawnedExplosion.GetComponentsInChildren<ParticleSystem>();
        foreach (var c in explosionChildren)
        {
            c.Stop();
        }

        Destroy(spawnedExplosion, explosionDestroyDelay);
    }
}
