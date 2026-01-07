using System;
using UnityEngine;

public class BossWeakPoint : MonoBehaviour
{
    public event Action<BossWeakPoint> OnDestroyed;

    public event Action<BossWeakPoint> OnDamaged;

    private CharacterHealthManager healthManager;

    public bool IsGrounded { get; private set; }

    private void Awake()
    {
        healthManager = GetComponent<CharacterHealthManager>();
    }

    private void OnEnable()
    {
        if (healthManager != null)
        {
            healthManager.OnDeath += HandleDeath;
            healthManager.OnDamageTaken += HandleDamage;
        }
    }

    private void OnDisable()
    {
        if (healthManager != null)
        {
            healthManager.OnDeath -= HandleDeath;
            healthManager.OnDamageTaken -= HandleDamage; 
        }
    }

    private void HandleDeath()
    {
        OnDestroyed?.Invoke(this);
        gameObject.SetActive(false);
    }

    private void HandleDamage()
    {
        OnDamaged?.Invoke(this);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            IsGrounded = true;
        }
    }

    public void ResetGroundedFlag()
    {
        IsGrounded = false;
    }
}
