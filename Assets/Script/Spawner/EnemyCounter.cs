using UnityEngine;
using System.Collections.Generic;
using System;

public class EnemyCounter : MonoBehaviour
{
    public static EnemyCounter Instance { get; private set; }

    private HashSet<GameObject> enemiesInView = new HashSet<GameObject>();
    [SerializeField] private int enemyCountInView; // シリアライズしてインスペクターに表示
    public int EnemyCountInView => enemyCountInView;

    public event Action<EnemyBase> OnEnemyEntered;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // DontDestroyOnLoad(gameObject); // 必要であれば
    }

    private void OnTriggerEnter(Collider other)
    {
        EnemyBase enemy = other.GetComponent<EnemyBase>();
        if (enemy != null && !enemiesInView.Contains(other.gameObject))
        {
            enemiesInView.Add(other.gameObject);
            UpdateEnemyCountInInspector();

            // 敵が視界内に入った瞬間通知
            OnEnemyEntered?.Invoke(enemy);

            Debug.Log($"カメラ視野内に入った敵: {other.gameObject.name}, 現在の敵数: {EnemyCountInView}");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (enemiesInView.Contains(other.gameObject))
        {
            enemiesInView.Remove(other.gameObject);
            UpdateEnemyCountInInspector();
            Debug.Log($"カメラ視野外に出た敵: {other.gameObject.name}, 現在の敵数: {EnemyCountInView}");
        }
    }

    // Destroyされた敵をカウントから削除するためのメソッド
    public void RemoveEnemy(GameObject enemy)
    {
        if (enemiesInView.Contains(enemy))
        {
            enemiesInView.Remove(enemy);
            UpdateEnemyCountInInspector();
            Debug.Log($"敵を削除しました (Destroy): {enemy.name}, 現在の敵数: {EnemyCountInView}");
        }
    }

    private void UpdateEnemyCountInInspector()
    {
        enemyCountInView = enemiesInView.Count;
    }

    public int GetEnemyCountInView() => EnemyCountInView;
    public HashSet<GameObject> GetEnemiesInView() => enemiesInView;
}