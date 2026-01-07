using UnityEngine;
using System.Collections;

public class BlueEnemy : EnemyBase
{
    private Collider enemyCollider;
    private Rigidbody rb;
    private BlueEnemyMoving moving;

    [SerializeField] private float reviveTime = 5f;
    [SerializeField] private Vector3 squashedScale = new Vector3(1f, 0.3f, 1f); // 潰れたサイズ
    [SerializeField] private string enemyLayer = "Enemy";
    [SerializeField] private string invincibleLayer = "InvincibleEnemy";

    private Vector3 originalScale;
    private int defaultLayer;

    new void Start()
    {
        healthManager = GetComponent<CharacterHealthManager>();
        enemyCollider = GetComponent<Collider>();
        rb = GetComponent<Rigidbody>();
        moving = GetComponent<BlueEnemyMoving>();

        originalScale = transform.localScale;
        defaultLayer = LayerMask.NameToLayer(enemyLayer);

        if (healthManager != null)
        {
            healthManager.OnDeath += OnEnemyDeath;
        }
    }

    private void OnEnemyDeath()
    {
        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        float originalHeight = transform.localScale.y;
        float squashedHeight = squashedScale.y;
        float heightDifference = (originalHeight - squashedHeight) * 0.5f;

        // 潰れる
        transform.localScale = squashedScale;
        transform.position -= new Vector3(0f, heightDifference, 0f);

        // レイヤー変更して衝突無効化
        gameObject.layer = LayerMask.NameToLayer(invincibleLayer);

        // 無力化
        enemyCollider.enabled = false;
        if (rb != null) rb.isKinematic = true;
        if (moving != null) moving.enabled = false;

        // 震える前まで待つ
        float shakeDuration = reviveTime / 2.5f;
        float waitBeforeShake = reviveTime - shakeDuration;
        if (waitBeforeShake > 0f)
            yield return new WaitForSeconds(waitBeforeShake);

        // 震える演出
        float elapsed = 0f;
        float shakeStrength = 0.05f;
        Vector3 basePosition = transform.position;

        while (elapsed < shakeDuration)
        {
            float offsetX = Random.Range(-shakeStrength, shakeStrength);
            float offsetY = Random.Range(-shakeStrength, shakeStrength);
            transform.position = basePosition + new Vector3(offsetX, offsetY, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 元の位置・サイズに戻す
        transform.position = basePosition + new Vector3(0f, heightDifference, 0f);
        transform.localScale = originalScale;

        // レイヤー戻す
        gameObject.layer = defaultLayer;

        // 復活
        enemyCollider.enabled = true;
        if (rb != null) rb.isKinematic = false;
        if (moving != null) moving.enabled = true;

        healthManager.Recovery(1f); // HP回復
    }

    private void OnDestroy()
    {
        // メモリーリーク回避
        if (healthManager != null)
        {
            healthManager.OnDeath -= OnEnemyDeath;
        }
    }
}
