using UnityEngine;

[CreateAssetMenu(fileName = "BlueBoundMovementConfig", menuName = "Enemy/BlueBoundMovementConfig")]
public class BlueBoundMovementConfig : ScriptableObject
{
    public float jumpForce = 10f;              // ジャンプの上方向の力
    public float moveForce = 3f;               // 横方向の移動力
    public float jumpInterval = 1.5f;          // ジャンプまでの間隔
    public float crouchDuration = 0.4f;        // しゃがみ（溜め）時間
    public float crouchScaleY = 0.5f;          // しゃがみ時のスケール
    public float wallDetectionDistance = 1f;   // 壁との接近距離
}
