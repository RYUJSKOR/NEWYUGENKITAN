using UnityEngine;

/// <summary>
/// 動く足場に乗ったプレイヤーを、足場の子オブジェクトにして追従させるスクリプト。
/// </summary>
public class MovingPlatform : MonoBehaviour
{
    /// <summary>
    /// オブジェクトがこのコライダーに乗り始めた時に呼ばれる
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        // 接触したのがプレイヤーなら
        if (collision.gameObject.CompareTag("Player"))
        {
            // プレイヤーをこの足場オブジェクトの子にする
            collision.transform.SetParent(this.transform);
            Debug.Log("プレイヤーが足場に乗りました。親子関係を設定。");
        }
    }

    /// <summary>
    /// オブジェクトがこのコライダーから離れた時に呼ばれる
    /// </summary>
    private void OnCollisionExit(Collision collision)
    {
        // 離れたのがプレイヤーなら
        if (collision.gameObject.CompareTag("Player"))
        {
            // 親子関係を解除する (nullを設定するとトップ階層に戻る)
            collision.transform.SetParent(null);
            Debug.Log("プレイヤーが足場から降りました。親子関係を解除。");
        }
    }
}