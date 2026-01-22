using UnityEngine;

public class KarakasaGhost : TargetingEnemy
{
    private KarakasaGhostMovement karakasaMovement;
    private SEController SE;

    [SerializeField] private KarakasaMovementConfig movementConfig;

    new void Start()
    {
        base.Start();
        karakasaMovement = GetComponent<KarakasaGhostMovement>();
        if (karakasaMovement == null)
        {
            karakasaMovement = gameObject.AddComponent<KarakasaGhostMovement>();
        }

        karakasaMovement.Initialize(this);
        karakasaMovement.SetConfig(movementConfig);

        if (healthManager != null)
        {
            healthManager.OnDeath += OnDeath;
        }

        // �� --- ��������ǉ� ---
        // �Q�[���J�n���ɁA�C���X�y�N�^�[�Őݒ肳�ꂽ�������x���ړ��R���|�[�l���g�ɓ`����
        if (karakasaMovement != null)
        {
            karakasaMovement.SetSpeedModifier(currentSpeedModifier);
        }
        // �� --- �����܂Œǉ� ---

        SE = GetComponent<SEController>();
    }

    // �� --- ��������ǉ� ---
    /// <summary>
    /// �O������Ă΂�A���x�{�����ړ��R���|�[�l���g�ɓ`����
    /// </summary>
    public override void ApplySpeedModifier(float modifier)
    {
        base.ApplySpeedModifier(modifier); // ���N���X�̒l���X�V

        if (karakasaMovement != null)
        {
            karakasaMovement.SetSpeedModifier(currentSpeedModifier);
        }
    }
    // �� --- �����܂Œǉ� ---

    private void OnDeath()
    {
        // --- 1. 移動および物理演算の停止 ---
        if (karakasaMovement != null)
        {
            karakasaMovement.enabled = false;
        }

        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            // 死亡時に物理挙動が残らないよう、キネマティックに設定し速度をゼロにする
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
        }

        // --- 2. 外見と当たり判定の無効化 ---
        // AudioSourceの破棄を防ぐため、GameObjectは残したままレンダラーのみを非表示にする
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            r.enabled = false;
        }

        // プレイヤーの移動を妨げないよう、コリジョンを無効化する
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }

        // --- 3. SE再生と破棄予約 ---
        float soundDuration = 0.0f; // デフォルトの待機時間
        if (SE != null)
        {
            // SEを再生し、そのクリップの長さを待機時間として取得する
            soundDuration = SE.Play("Enemy.KarakasaDie");
        }

        // --- 4. エフェクト生成とアイテムドロップ処理 ---
        GameObject gomi = Instantiate(piecesPrefab, transform.position, Quaternion.identity);
        Destroy(gomi, GomiLifeTime);
        DropItem();

        // --- 5. SE再生完了後にオブジェクトを完全に破棄 ---
        // 音が途切れないよう、再生時間（soundDuration）分待機してから削除する
        Destroy(gameObject, soundDuration > 0 ? soundDuration : 0.1f);
    }
}