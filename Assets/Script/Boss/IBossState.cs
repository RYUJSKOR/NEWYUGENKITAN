

public interface IBossState
{
    // このステートに切り替わった時に一度だけ呼ばれる処理
    void Enter();

    // このステートである間、毎フレーム呼ばれる処理
    void Execute();

    // このステートから別のステートに切り替わる時に一度だけ呼ばれる処理
    void Exit();
}