using UnityEngine;

public abstract class BaseState
{
    protected BlueBoundEnemyMovement enemy;
    protected float stateTimer;

    public BaseState(BlueBoundEnemyMovement enemy)
    {
        this.enemy = enemy;
    }

    public virtual void EnterState()
    {
        stateTimer = 0f;
    }

    public abstract void UpdateState();

    // ↓ --- このメソッドをまるごと追加 ---
    /// <summary>
    /// このステートにいる間、FixedUpdateのタイミングで呼ばれる処理
    /// </summary>
    public virtual void FixedUpdateState() { }
    // ↑ --- ここまで追加 ---

    public virtual void ExitState() { }
}