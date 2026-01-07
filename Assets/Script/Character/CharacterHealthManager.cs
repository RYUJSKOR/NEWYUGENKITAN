using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class CharacterHealthManager : MonoBehaviour
{
    [SerializeField] private float Health;
    [SerializeField] private float invincibleDuration = 1.0f;

    private float maxHealth;
    private float invincibleTimer = 0f;
    private float accumulatedDamage = 0f;
    private bool isProcessingDamage = false;

    [SerializeField, TagSelector] private List<string> DesignationTag;

    /// <summary>
    /// HPの下限値。この値よりHPが下がらなくなります。0以下の場合は無効です。
    /// </summary>
    public float HealthGate { get; set; } = 0f;

    public bool IsInvincible => invincibleTimer > 0;

    public event Action OnDeath;
    public event Action OnDamageTaken;

    private void Awake()
    {
        maxHealth = Health;
    }

    private void Update()
    {
        if (invincibleTimer > 0)
        {
            invincibleTimer -= Time.deltaTime;
        }
    }

    public void ApplyDamage(float value, bool bypassInvincibility = false)
    {
        if (IsInvincible && !bypassInvincibility)
        {
            return;
        }
        accumulatedDamage += value;
        if (!isProcessingDamage)
        {
            StartCoroutine(ProcessDamage());
        }
    }

    private IEnumerator ProcessDamage()
    {
        isProcessingDamage = true;
        yield return null;

        if (accumulatedDamage > 0)
        {
            // ヘルスゲートのロジック
            float newHealth = Health - accumulatedDamage;

            // ゲートが設定されており、計算後のHPがゲートを下回る場合
            if (HealthGate > 0 && newHealth < HealthGate)
            {
                // HPをゲートの値で固定する
                Health = HealthGate;
                Debug.Log($"<color=orange>[HealthGate] ダメージが上限に達したため、HPを {Health} でストップしました。</color>");
            }
            else
            {
                // 通常通りダメージを適用
                Health = newHealth;
            }

            OnDamageTaken?.Invoke();
            accumulatedDamage = 0f;

            if (Health <= 0)
            {
                Health = 0;
                Die();
            }
            else if (invincibleDuration > 0)
            {
                invincibleTimer = invincibleDuration;
            }
        }
        isProcessingDamage = false;
    }

    private void Die()
    {
        OnDeath?.Invoke();
    }


    public void Recovery(float value)
    {
        Health += value;
        if (Health > maxHealth)
        {
            Health = maxHealth;
        }
    }

    public void ResetHealth()
    {
        Health = maxHealth;
    }

    public void ActivateInvincibility(float duration)
    {
        invincibleTimer = duration;
    }

    public void InstantKill()
    {
        ApplyDamage(Health, true);
    }

    public bool IsDead() { return Health <= 0; }

    public bool IsTakingDamage() { return IsInvincible; }

    private void OnCollisionEnter(Collision collision)
    {
        if (DesignationTag.Contains(collision.gameObject.tag))
        {
            ApplyDamage(1);
        }
    }

    public float GetInvincibleDuration() { return invincibleDuration; }

    public float GetHealth() { return Health; }

    private void OnDisable()
    {
        isProcessingDamage = false;
        accumulatedDamage = 0f;
    }

    public float GetMaxHealth() { return maxHealth; }

    public void SetHealth(float value)
    {
        Health = value;
    }
}