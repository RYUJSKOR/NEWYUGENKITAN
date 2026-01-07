using UnityEngine;

public class SpriteRotator : MonoBehaviour
{
    private float rotationSpeed;
    private Vector3 rotationDirection;

    public void SetRotation(float speed, Vector3 direction)
    {
        rotationSpeed = speed;
        rotationDirection = direction;
    }

    void Update()
    {
        // •ÏX“_: Time.deltaTime -> Time.unscaledDeltaTime
        // Time.timeScale‚ª0‚Å‚à‰ñ“]‚·‚é‚æ‚¤‚É‚·‚é
        transform.Rotate(rotationDirection, rotationSpeed * Time.unscaledDeltaTime);
    }
}