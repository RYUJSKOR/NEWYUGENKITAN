using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "NewBoneRainAttack", menuName = "Boss Attacks/Bone Rain Attack")]
public class BoneRainAttack : BossAttackPattern
{
    [Header("骨の雨 攻撃の設定")]
    public GameObject bonePrefab; // 降らせる骨のプレハブ
    public int numberOfBones = 30; // 降らせる骨の総数
    public float rainDuration = 5.0f; // 何秒間、雨を降らせ続けるか
    public float spawnHeight = 20f; // プレイヤーの頭上、どれくらいの高さから出現させるか
    public float spawnAreaWidth = 30f; // どのくらいの横幅の範囲に降らせるか

    public override void Execute(BossController boss)
    {
        if (boss.IsAttacking) return;

        boss.RunAttackCoroutine(AttackRoutine(boss));
    }

    private IEnumerator AttackRoutine(BossController boss)
    {
        // ▼▼▼ 修正 ▼▼▼
        // 攻撃開始と同時にフラグを立てる
        boss.SetAttackingState(true, false);

        try
        {
            Debug.Log("骨の雨、開始！");

            // (ここに、ボスが空に向かって咆哮するなどの予備動作アニメーションを入れると良い)
            yield return new WaitForSeconds(1.0f);

            float timeBetweenSpawns = rainDuration / numberOfBones;

            // 指定した回数分、骨を生成するループ
            for (int i = 0; i < numberOfBones; i++)
            {
                // プレイヤーの位置を基準に、横方向にランダムな位置を決定
                float randomX = Random.Range(-spawnAreaWidth / 2, spawnAreaWidth / 2);
                Vector3 spawnPosition = boss.playerTransform.position + new Vector3(randomX, spawnHeight, 0);

                // 骨のプレハブを生成
                if (bonePrefab != null)
                {
                    Instantiate(bonePrefab, spawnPosition, Random.rotation);
                }

                // 次の骨を生成するまで待機
                yield return new WaitForSeconds(timeBetweenSpawns);
            }

            // 最後の骨が落ちるまで少し待つ
            yield return new WaitForSeconds(2.0f);
        }
        finally
        {
            // 処理が中断されても、されなくても、必ず最後に呼ばれる
            boss.SetAttackingState(false, false);
            Debug.Log("骨の雨、終了！");
        }
        // ▲▲▲ ▲▲▲
    }
}