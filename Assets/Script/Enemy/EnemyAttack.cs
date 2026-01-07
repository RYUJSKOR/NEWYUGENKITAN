using UnityEngine;

// 攻撃コンポーネントは必ずShootingコンポーネントを必要とする
[RequireComponent(typeof(Shooting))]
public abstract class EnemyAttack : MonoBehaviour
{
    protected Shooting shooting;

    protected virtual void Awake()
    {
        shooting = GetComponent<Shooting>();
    }

    /// <summary>
    /// 外部のConfigアセットを使って攻撃の初期設定を行います。
    /// </summary>
    /// <param name="config">攻撃設定用のScriptableObject</param>
    public abstract void Initialize(ScriptableObject config);

    /// <summary>
    /// 攻撃を実行します。
    /// </summary>
    public abstract void PerformAttack();

    public abstract void SetTarget(GameObject target);
}