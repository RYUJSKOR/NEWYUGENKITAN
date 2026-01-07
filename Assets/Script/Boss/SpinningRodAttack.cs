using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "NewSpinningRodAttack", menuName = "Boss Attacks/Spinning Rod Attack")]
public class SpinningRodAttack : BossAttackPattern
{
    [Header("回転ロッド攻撃の設定")]
    public GameObject rodPrefab;
    public float[] attackHeights = { 1.0f, 3.0f, 5.0f };
    public int attackCount = 3;
    public float delayBetweenAttacks = 1.5f;
    public float travelSpeed = 20f;
    public float rotationSpeed = 360f;
    public float screenEdgeOffset = 5.0f;
    public float initialDelay = 1.0f;

    private int lastHeightIndex = -1;

    public override void Execute(BossController boss)
    {
        if (boss.IsAttacking) return;

        boss.RunAttackCoroutine(AttackRoutine(boss));
    }

    private IEnumerator AttackRoutine(BossController boss)
    {
        // ▼▼▼ 修正 ▼▼▼
        boss.SetAttackingState(true, false);

        try
        {
            yield return new WaitForSeconds(initialDelay);

            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("Main Cameraが見つかりません！");
                yield break;
            }

            for (int i = 0; i < attackCount; i++)
            {
                // --- 1. 攻撃位置の計算 ---
                int newHeightIndex = Random.Range(0, attackHeights.Length);
                while (attackHeights.Length > 1 && newHeightIndex == lastHeightIndex)
                {
                    newHeightIndex = Random.Range(0, attackHeights.Length);
                }
                lastHeightIndex = newHeightIndex;
                float currentAttackHeight = attackHeights[newHeightIndex];

                float distanceToPlayer = Vector3.Dot(boss.playerTransform.position - mainCamera.transform.position, mainCamera.transform.forward);
                Vector3 leftEdge = mainCamera.ViewportToWorldPoint(new Vector3(0, 0.5f, distanceToPlayer));
                Vector3 rightEdge = mainCamera.ViewportToWorldPoint(new Vector3(1, 0.5f, distanceToPlayer));

                Vector3 startPos, endPos;
                bool startsFromLeft = (Random.value > 0.5f);

                if (startsFromLeft)
                {
                    startPos = leftEdge - Vector3.right * screenEdgeOffset;
                    endPos = rightEdge + Vector3.right * screenEdgeOffset;
                }
                else
                {
                    startPos = rightEdge + Vector3.right * screenEdgeOffset;
                    endPos = leftEdge - Vector3.right * screenEdgeOffset;
                }
                startPos.y = endPos.y = currentAttackHeight;

                // --- 2. 片サイドから攻撃オブジェクトを生成 ---
                boss.StartCoroutine(LaunchSingleRod(rodPrefab, startPos, endPos, travelSpeed, rotationSpeed));

                // --- 3. 次の攻撃までの待機 ---
                yield return new WaitForSeconds(delayBetweenAttacks);
            }

            // --- 4. 攻撃終了待機 ---
            float journeyLength = Vector3.Distance(mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0)), mainCamera.ViewportToWorldPoint(new Vector3(1, 0, 0))) + screenEdgeOffset * 2;
            yield return new WaitForSeconds(journeyLength / travelSpeed);
        }
        finally
        {
            // --- 5. 攻撃終了 ---
            boss.SetAttackingState(false, false);
        }
        // ▲▲▲ ▲▲▲
    }

    private IEnumerator LaunchSingleRod(GameObject prefab, Vector3 startPos, Vector3 endPos, float speed, float rotSpeed)
    {
        if (prefab == null) yield break;

        GameObject rodInstance = Instantiate(prefab, startPos, Quaternion.identity);
        float journeyDuration = Vector3.Distance(startPos, endPos) / speed;
        float timer = 0f;

        while (timer < journeyDuration)
        {
            if (rodInstance == null) yield break;

            rodInstance.transform.position = Vector3.Lerp(startPos, endPos, timer / journeyDuration);
            rodInstance.transform.Rotate(Vector3.forward, rotSpeed * Time.deltaTime);

            timer += Time.deltaTime;
            yield return null;
        }

        if (rodInstance != null)
        {
            Destroy(rodInstance);
        }
    }
}