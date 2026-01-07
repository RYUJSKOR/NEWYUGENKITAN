using System;
using UnityEngine;

[RequireComponent(typeof(EnemyBase))]
public class EnemyDeathNotifier : MonoBehaviour
{
    public event Action<EnemyBase> OnEnemyDied;

    private EnemyBase enemyBase;
    private CharacterHealthManager healthManager;

    private void Awake()
    {
        enemyBase = GetComponent<EnemyBase>();
        healthManager = GetComponent<CharacterHealthManager>();

        if (healthManager != null)
        {
            // 敵の死亡時にイベントを転送
            healthManager.OnDeath += HandleEnemyDeath;
        }
        else
        {
            Debug.LogWarning($"{name} に CharacterHealthManager が見つかりません。");
        }
    }

    private void HandleEnemyDeath()
    {
        OnEnemyDied?.Invoke(enemyBase);
        Debug.Log($"[EnemyDeathNotifier] {enemyBase.name} が死亡しました。イベント発行。");
    }

    private void OnDestroy()
    {
        if (healthManager != null)
        {
            healthManager.OnDeath -= HandleEnemyDeath;
        }
    }
}