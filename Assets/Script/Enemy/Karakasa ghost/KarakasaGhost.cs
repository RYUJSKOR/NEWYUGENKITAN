using UnityEngine;

public class KarakasaGhost : TargetingEnemy
{
    private KarakasaGhostMovement karakasaMovement;

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
        // OnDeath���̖�����
        if (karakasaMovement != null)
        {
            karakasaMovement.enabled = false;
        }

        // �����I�ȓ������~�߂�
        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
        }

        GameObject gomi = Instantiate(piecesPrefab, transform.position, Quaternion.identity);
        Destroy(gomi, GomiLifeTime);
        Destroy(gameObject);
        DropItem();
    }
}