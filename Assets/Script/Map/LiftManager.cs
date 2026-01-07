using UnityEngine;

public class LiftManager : MonoBehaviour
{

    [Header("X軸の移動範囲")]
    public bool moveOnX = true;   // trueの場合、X軸で移動する
    public float leftX = -5f;     // 左端のX座標
    public float rightX = 5f;     // 右端のX座標

    [Header("Y軸の移動範囲")]
    public bool moveOnY = true;   // trueの場合、Y軸で移動する
    public float topY = 5f;       // 上端のY座標
    public float bottomY = 0f;    // 下端のY座標

    [Header("移動速度")]
    public float speed = 2f;      // 移動速度

    private bool movingRight = true;
    private bool movingUp = true;

    private void FixedUpdate()
    {
        // 現在の座標を取得
        Vector3 pos = transform.position;

        // X軸（左右）の移動処理
        if (moveOnX)
        {
            if (movingRight)
            {
                pos.x += speed * Time.deltaTime;
                if (pos.x >= rightX)
                    movingRight = false;
            }
            else
            {
                pos.x -= speed * Time.deltaTime;
                if (pos.x <= leftX)
                    movingRight = true;
            }
        }

        // Y軸（上下）の移動処理
        if (moveOnY)
        {
            if (movingUp)
            {
                pos.y += speed * Time.deltaTime;
                if (pos.y >= topY)
                    movingUp = false;
            }
            else
            {
                pos.y -= speed * Time.deltaTime;
                if (pos.y <= bottomY)
                    movingUp = true;
            }
        }

        // 座標を更新
        transform.position = pos;
    }

    // プレイヤーが乗ったら子オブジェクトにして一緒に移動
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(transform);
        }
    }

    // プレイヤーが降りたら親子関係を解除
    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(null);
        }
    }
}