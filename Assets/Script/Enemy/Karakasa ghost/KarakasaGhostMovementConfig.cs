using UnityEngine;

[CreateAssetMenu(fileName = "KarakasaMovementConfig", menuName = "Enemy/KarakasaMovementConfig")]
public class KarakasaMovementConfig : ScriptableObject
{
    public float bounceUpForce = 10f;
    public float floatFallSpeed = 1f;
    public float swingSpeed = 0.5f;
    public float swingRange = 0.2f;
    public float jumpForce = 7f;
    public float jumpInterval = 2f;
    public float crouchDuration = 0.4f;
    public float crouchScaleY = 0.5f;
    public float groundedIgnoreTime = 0.4f;
}
