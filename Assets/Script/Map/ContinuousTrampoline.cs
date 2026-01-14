using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ContinuousTrampoline : MonoBehaviour
{
    private SEController SE;

    [Header("トランポリン設定")]
    [Tooltip("通常ジャンプより強いジャンプ力を設定")]
    public float trampolineForce = 28f; // 例: 通常ジャンプ(19)より高く

    [Tooltip("水平方向の速度を維持するか")]
    public bool preserveHorizontalVelocity = true;

    private void Awake()
    {
        SE = GetComponent<SEController>();
    }

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerStay(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;
        if (rb == null) return;

        SE?.Play("Map.Trampoline");

        // 水平方向を維持してジャンプ
        Vector3 v = rb.linearVelocity;
        if (preserveHorizontalVelocity)
            rb.linearVelocity = new Vector3(v.x, trampolineForce, v.z);
        else
            rb.linearVelocity = Vector3.up * trampolineForce;

        // PlayerJumping に通知
        var player = other.GetComponent<Player>();
        if (player != null)
        {
            var jumpingState = player.GetComponent<PlayerStateMachine>().GetState<PlayerJumping>();
            if (jumpingState != null)
            {
                jumpingState.OnTrampolineBounce(trampolineForce);
            }
        }
    }

}