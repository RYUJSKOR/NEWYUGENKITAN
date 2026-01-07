using UnityEngine;

public class DestroyAfterAnimation : MonoBehaviour
{
    // アニメーションイベントから呼び出すための公開メソッド
    public void DestroySelf()
    {
        Destroy(gameObject);
    }
}