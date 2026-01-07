using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    public bool IsGrounded { get; private set; } = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ground")) IsGrounded = true;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Ground")) IsGrounded = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Ground")) IsGrounded = false;
    }
}
