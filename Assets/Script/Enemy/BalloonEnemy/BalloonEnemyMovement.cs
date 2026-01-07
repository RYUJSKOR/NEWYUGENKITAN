using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BalloonEnemyMovement : MonoBehaviour
{
    // [SerializeField]�̓f�o�b�O�p�Ɏc���Ă��ǂ��ł����AInitialize�Őݒ肳��邽�ߕK�{�ł͂���܂���B
    private Vector3 moveDirection = Vector3.forward;
    private float moveSpeed = 2f;
    private float floatStrength = 0.5f;
    private float floatSpeed = 1f;

    private Rigidbody rb;
    private Vector3 initialPosition;
    private bool isFrozen = false;

    public void Initialize(BalloonEnemyMovementConfig config)
    {
        moveDirection = config.moveDirection.normalized;
        moveSpeed = config.moveSpeed;
        floatStrength = config.floatStrength;
        floatSpeed = config.floatSpeed;
    }

    public void Freeze()
    {
        isFrozen = true;
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero; // linearVelocity�͌Â�API�Ȃ̂�velocity�𐄏�
        }
    }

    public void Resume()
    {
        isFrozen = false;
    }

    private void Awake() // Start��葁���Q�Ƃ��m�ۂ��邽��Awake�𐄏�
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        initialPosition = transform.position;
    }

    private void FixedUpdate()
    {
        if (isFrozen)
        {
            rb.linearVelocity = Vector3.zero;
            return;
        }

        // �ڕW�ƂȂ�Y���W��Sin�g�Ōv�Z
        float targetY = initialPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatStrength;

        // XZ���ʂ̈ړ����x
        Vector3 targetVelocityXZ = moveDirection * moveSpeed;

        // Y�����̑��x���A���݂�Y���W�ƖڕW��Y���W�̍�����v�Z
        // ����ɂ��A�I�u�W�F�N�g�͖ڕW��Y���W�ɒǏ]����悤�ɓ����܂�
        float yVelocity = (targetY - rb.position.y) / Time.fixedDeltaTime;

        // X, Y, Z�S�Ă̑��x���������āA�ŏI�I�ȑ��x�Ƃ���Rigidbody�ɐݒ�
        rb.linearVelocity = new Vector3(targetVelocityXZ.x, yVelocity, targetVelocityXZ.z);
    }
}