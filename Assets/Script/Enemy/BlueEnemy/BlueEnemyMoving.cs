using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BlueEnemyMoving : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float groundCheckDistance = 1f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody rb;
    private Vector3 moveDirection = Vector3.left;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (!IsGroundAhead())
        {
            Flip(); // 崖の先に地面がなければ反転
        }

        rb.linearVelocity = new Vector3(moveDirection.x * moveSpeed, rb.linearVelocity.y, 0f);
    }

    private bool IsGroundAhead()
    {
        Vector3 origin = transform.position + moveDirection * 0.5f;
        origin.y += 0.1f;
        return Physics.Raycast(origin, Vector3.down, groundCheckDistance, groundLayer);
    }

    private void Flip()
    {
        moveDirection = -moveDirection;
        transform.Rotate(0f, 180f, 0f); 
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Groundレイヤーとぶつかったら反転
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            // 壁に正面からぶつかったときだけ反転
            if (Vector3.Dot(collision.contacts[0].normal, moveDirection) < -0.5f)
            {
                Flip();
            }
        }
    }
}
