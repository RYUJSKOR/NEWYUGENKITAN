using UnityEngine;

// EnemyBaseを継承した、敵本体クラス
public class FallEnemy : EnemyBase
{
    // 自身の移動コンポーネントを保持
    private FallEnemyMovement movement;

    // 基底クラスのStartをオーバーライド（または隠蔽）
    new protected void Start()
    {
        base.Start(); // EnemyBaseのStart()の処理を呼び出す

        // 自分にアタッチされているFallEnemyMovementコンポーネントを取得
        movement = GetComponent<FallEnemyMovement>();

        // ゲーム開始時に、インスペクターで設定された初期速度を移動コンポーネントに伝える
        if (movement != null)
        {
            movement.SetSpeedModifier(currentSpeedModifier);
        }

        // TODO: ここで体力管理などの初期化を行う
        // if (healthManager != null)
        // {
        //     healthManager.OnDeath += OnDeath;
        // }
    }

    /// <summary>
    /// 必殺技などで外部から呼ばれ、速度倍率を移動コンポーネントに伝える
    /// </summary>
    public override void ApplySpeedModifier(float modifier)
    {
        base.ApplySpeedModifier(modifier); // 基底クラスの値を更新

        if (movement != null)
        {
            movement.SetSpeedModifier(currentSpeedModifier);
        }
    }

    // 敵が倒されたときの処理（必要に応じて実装）
    // private void OnDeath()
    // {
    //     Explode(); // 例えば爆発四散する
    //     Destroy(gameObject);
    // }
}