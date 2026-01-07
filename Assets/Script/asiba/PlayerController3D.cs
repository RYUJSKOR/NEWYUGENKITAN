using UnityEngine;

/// <summary>
/// プレイヤーの入力を管理し、一方通行の足場を下に降りる処理を実行する。
/// 既存の移動システムとは独立して動作する。
/// </summary>
public class PlayerController3D : MonoBehaviour
{
    [Tooltip("すり抜け開始時に与える下向きの力（インパルス）")]
    [SerializeField]
    private float fallInitialForce = 10f;

    private Collider playerCollider;
    private Rigidbody rb;
    private int playerOriginalLayer; // プレイヤーの本来のレイヤー

    // PlayerStateMachineへの参照をここで保持します
    private PlayerStateMachine playerStateMachine;

    // 現在すり抜け処理の対象となっている足場
    private OneWayPlatform3D currentFallingPlatform = null;

    private const string DEBUG_KEY = "[OneWayPlatformDebug]";

    private void Start()
    {
        playerCollider = GetComponent<Collider>();
        rb = GetComponent<Rigidbody>();

        // 同じGameObjectにアタッチされているPlayerStateMachineコンポーネントを取得します
        playerStateMachine = GetComponent<Player>().playerStateMachine;

        // 最初にプレイヤーのレイヤーを記憶しておく
        playerOriginalLayer = gameObject.layer;

        if (playerCollider == null)
            Debug.LogError($"{DEBUG_KEY} (Player): PlayerにColliderがアタッチされていません！", this);
        if (rb == null)
            Debug.LogWarning($"{DEBUG_KEY} (Player): PlayerにRigidbodyがアタッチされていません。初速を与える処理が機能しません。", this);
        if (!CompareTag("Player"))
            Debug.LogWarning($"{DEBUG_KEY} (Player): Playerのタグが'Player'に設定されていません。", this);
        if (playerStateMachine == null)
            Debug.LogError($"{DEBUG_KEY} (Player): PlayerStateMachineコンポーネントが見つかりません！このスクリプトは動作しません。", this);
    }

    // PlayerController3D.cs の Updateメソッド

    private void Update()
    {
        // PlayerStateMachineへの参照がなければ、何もしません
        if (playerStateMachine == null) return;

        // PlayerStateMachineから直接しゃがみ（下入力）の状態を取得します
        bool isCrouchHeld = playerStateMachine.IsCrouching;

        // --- デバッグログ１：入力状態の確認 ---
        // しゃがみキーを押している間、コンソールに "入力検知: True" と表示され続けるか確認
        Debug.Log("入力検知: " + isCrouchHeld);

        // --- 下キーによるすり抜け処理 ---

        if (isCrouchHeld)
        {
            if (currentFallingPlatform != null) return;

            // --- デバッグログ２：Raycastの発射を確認 ---
            // しゃがみ時にコンソールに "Raycast発射試行" と表示されるか確認
            Debug.Log("Raycast発射試行");

            // Raycastを可視化して、Sceneビューで確認しやすくします
            Debug.DrawRay(transform.position, Vector3.down * 1.0f, Color.red);

            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 1.0f))
            {
                // --- デバッグログ３：Raycastが何かにヒットしたか確認 ---
                // Raycastがヒットした場合、対象オブジェクト名がコンソールに表示されるか確認
                Debug.Log("Raycastヒット！ 対象: " + hit.collider.name);

                OneWayPlatform3D platform = hit.collider.GetComponentInParent<OneWayPlatform3D>();
                if (platform != null)
                {
                    // --- デバッグログ４：すり抜け処理が実行されたか確認 ---
                    // このログが表示されれば、すり抜け処理自体は開始されています
                    Debug.Log("すり抜け処理を開始します！");

                    currentFallingPlatform = platform;
                    //currentFallingPlatform.IgnoreCollision(playerCollider);
                    gameObject.layer = LayerMask.NameToLayer("FallingPlayer"); //
                    if (rb != null)
                    {
                        rb.AddForce(Vector3.down * fallInitialForce, ForceMode.Impulse);
                    }
                }
                else
                {
                    // --- デバッグログ５：コンポーネントが見つからない場合 ---
                    // Raycastは当たったが、OneWayPlatform3Dが見つからない場合に表示されます
                    Debug.LogWarning("ヒットしたオブジェクト " + hit.collider.name + " に OneWayPlatform3D コンポーネントがありません！");
                }
            }
        }
        else
        {
            if (currentFallingPlatform != null)
            {
                //currentFallingPlatform.RestoreCollision(playerCollider);
                gameObject.layer = playerOriginalLayer;
                currentFallingPlatform = null;
            }
        }
    }
}