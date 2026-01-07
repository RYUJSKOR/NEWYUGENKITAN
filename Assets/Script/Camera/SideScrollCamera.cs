using System; using System.Collections.Generic; using UnityEngine;  [System.Serializable] public class FollowThresholdZone {     public float xMin;     public float xMax;     public float thresholdY; }  public class SideScrollCamera : MonoBehaviour {
    public Transform player;

    [Header("追従速度")]
    public float smoothSpeedX = 5f;
    public float smoothSpeedY = 2f;

    [Header("カメラオフセットと制限")]
    public Vector3 offset = new Vector3(0f, 2f, -10f);
    public Vector3 rotationOffset = Vector3.zero;
    public float minX = 0f, maxX = 30f;

    [Header("デフォルトしきい値")]
    public float defaultThresholdY = 5f;
    public float deadZoneY = 0.5f;

    [Header("X範囲ごとのしきい値設定")]
    public List<FollowThresholdZone> thresholdZones = new List<FollowThresholdZone>();

    private float followYBlend = 0f; // 0 = 固定, 1 = 完全追従
    public float followYBlendSpeed = 2f; // 補間スピード

    private float targetOffsetX;   // プレイヤー基準の目標オフセットX
    public float flipSmoothSpeed = 1.5f;

    private bool isFollowingY = false;

    private float currentY;
    private float initialY;

    private float velocityX = 0f;
    private float velocityY = 0f;

    private float currentThresholdY;

    void Start()
    {
        if (!player) return;

        targetOffsetX = offset.x; // 初期値を保存
        initialY = player.position.y + offset.y;
        currentY = initialY;

        currentThresholdY = GetThresholdYForX(player.position.x);
    }

    void LateUpdate()
    {
        if (!player) return;

        Vector3 playerPos = player.position;

        // --- offset.x 補間 ---
        offset.x = Mathf.Lerp(offset.x, targetOffsetX, Time.deltaTime * flipSmoothSpeed);

        // --- 追従しきい値を補間 ---
        float targetThresholdY = GetThresholdYForX(playerPos.x);
        currentThresholdY = Mathf.Lerp(currentThresholdY, targetThresholdY, Time.deltaTime * 3f);

        // --- 追従したいかの判定 ---
        bool shouldFollowY =
            (!isFollowingY && playerPos.y >= currentThresholdY + deadZoneY) ||
            (isFollowingY && playerPos.y > currentThresholdY);

        // --- Y追従モードをスムーズに補間 ---
        float targetBlend = shouldFollowY ? 1f : 0f;
        followYBlend = Mathf.Lerp(followYBlend, targetBlend, Time.deltaTime * followYBlendSpeed);

        // --- 実際の目標Yを補間して求める ---
        float targetY_Follow = playerPos.y + offset.y;
        float targetY_Fixed = initialY;
        float blendedTargetY = Mathf.Lerp(targetY_Fixed, targetY_Follow, followYBlend);

        // --- スムーズ追従 ---
        float targetX = playerPos.x + offset.x;
        float smoothX = Mathf.SmoothDamp(transform.position.x, targetX, ref velocityX, 1f / smoothSpeedX);
        currentY = Mathf.SmoothDamp(currentY, blendedTargetY, ref velocityY, 1f / smoothSpeedY);

        Vector3 newPos = new Vector3(smoothX, currentY, offset.z);
        newPos.x = Mathf.Clamp(newPos.x, minX, maxX);

        transform.position = newPos;
        transform.rotation = Quaternion.Euler(rotationOffset);

        // --- 状態更新（最後に）---
        isFollowingY = shouldFollowY;
    }

    float GetThresholdYForX(float x)
    {
        FollowThresholdZone bestZone = null;
        float smallestRange = float.MaxValue;

        foreach (var zone in thresholdZones)
        {
            if (x >= zone.xMin && x <= zone.xMax)
            {
                float range = zone.xMax - zone.xMin;
                if (range < smallestRange)
                {
                    smallestRange = range;
                    bestZone = zone;
                }
            }
        }

        return bestZone != null ? bestZone.thresholdY : defaultThresholdY;
    }

    private void OnValidate()
    {
        if (!player) { return; }

        initialY = offset.y;
    }

    public void FlipOffsetX()
    {
        targetOffsetX = -targetOffsetX; // 目標値だけ反転
    }
}