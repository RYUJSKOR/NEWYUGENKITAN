using UnityEngine;

public class EnemySoundController : MonoBehaviour
{
    [Header("Audio Sources")]
    [Tooltip("ループ音源（登場時の音など）")]
    [SerializeField] private AudioSource loopSource;
    [Tooltip("単発効果音（攻撃、死亡音など）")]
    [SerializeField] private AudioSource oneShotSource;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip spawnSound;   // 登場時の音（ループまたはワンショット）
    [SerializeField] private AudioClip deathSound;   // 死亡時の音
    [SerializeField] private AudioClip attackSound;  // 攻撃時の音
    [SerializeField] private AudioClip damageSound;  // ダメージ受けた時の音
    [SerializeField] private AudioClip reviveSound;  // 復活時の音（BlueEnemy用）
    // ...その他必要な効果音を追加...

    void Start()
    {
        // 登場音が設定されていて、ループ音源があるなら再生
        if (loopSource != null && spawnSound != null && loopSource.loop)
        {
            loopSource.clip = spawnSound;
            loopSource.Play();
        }
        else if (oneShotSource != null && spawnSound != null)
        {
            oneShotSource.PlayOneShot(spawnSound);
        }
    }

    // --- 他のスクリプトから呼び出すためのメソッド群 ---

    public void PlayDeathSound()
    {
        // 死亡時はループ音を止める
        if (loopSource != null && loopSource.isPlaying)
        {
            loopSource.Stop();
        }
        PlaySound(deathSound);
    }

    public void PlayAttackSound()
    {
        PlaySound(attackSound);
    }

    public void PlayDamageSound()
    {
        PlaySound(damageSound);
    }

    public void PlayReviveSound()
    {
        PlaySound(reviveSound);
        // 必要ならループ音を再開
        if (loopSource != null && !loopSource.isPlaying && loopSource.loop)
        {
            loopSource.Play();
        }
    }

    // 実際に音を鳴らす内部処理
    private void PlaySound(AudioClip clip)
    {
        if (oneShotSource != null && clip != null)
        {
            oneShotSource.PlayOneShot(clip);
        }
    }
}