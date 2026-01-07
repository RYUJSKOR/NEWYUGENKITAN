using System.Collections;
using UnityEngine;

public class MovingWall : MonoBehaviour
{
    [SerializeField]
    [Tooltip("初期位置からの終点の相対位置（移動量）")]
    private Vector3 endPointOffset;

    [SerializeField]
    [Tooltip("壁の移動速度")]
    private float moveSpeed = 2.0f;

    [SerializeField]
    [Tooltip("各地点で待機する時間（秒）")]
    private float waitTime = 1.0f;

    [SerializeField]
    private Transform PlayerTransform;
    [SerializeField]
    private float distance;

    public bool Isdistance;

    // ゲーム中に実際に移動する目標地点（ワールド座標）
    private Vector3 startPosition;
    private Vector3 endPosition;
    private Vector3 nextTarget;

    // ゲーム開始時に一度だけ呼ばれる
    void Start()
    {
        // ゲーム開始時の位置を「始点」として記憶する
        startPosition = transform.position;
        // 「終点」を、始点＋オフセット（移動量）で計算する
        endPosition = startPosition + endPointOffset;

        // 最初の目標を「終点」に設定
        nextTarget = endPosition;

        // 移動処理を開始
        StartCoroutine(Move());
    }

    void Update()
    {
        // PlayerTransform が設定されているか確認
        if (PlayerTransform == null) return;

        // プレイヤーとの距離が指定範囲内かどうかを計算し、フィールド変数に代入
        // これにより、Inspectorでリアルタイムに bool の状態を確認できる
        Isdistance = Vector3.Distance(transform.position, PlayerTransform.position) <= distance;
    }

    // 壁を動かす処理（内部ロジックはほぼ同じ）
    private IEnumerator Move()
    {
        yield return new WaitUntil(() => Vector3.Distance(transform.position, PlayerTransform.position) <= distance);

        while (true)
        {
            while (Vector3.Distance(transform.position, nextTarget) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, nextTarget, moveSpeed * Time.deltaTime);
                yield return null;
            }

            transform.position = nextTarget;
            yield return new WaitForSeconds(waitTime);

            // 目標地点を切り替える
            if (nextTarget == endPosition)
            {
                nextTarget = startPosition;
            }
            else
            {
                nextTarget = endPosition;
            }
        }
    }

    // エディタ上でギズモを描画する
    private void OnDrawGizmos()
    {
        // ギズモの始点と終点を計算
        // Application.isPlayingはゲーム実行中かどうかを判定する
        // 実行中ならStart()で記憶した座標を、編集中なら現在のオブジェクトの位置を基準にする
        Vector3 gizmoStart = Application.isPlaying ? startPosition : transform.position;
        Vector3 gizmoEnd = gizmoStart + endPointOffset;

        // ギズモを描画
        Gizmos.color = Color.green; // 色を緑に変更
        Gizmos.DrawLine(gizmoStart, gizmoEnd);
        Gizmos.DrawSphere(gizmoStart, 0.15f);
        Gizmos.DrawSphere(gizmoEnd, 0.15f);
    }
}