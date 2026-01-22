using UnityEngine;

public class OrotiPhaseController : MonoBehaviour
{
    [SerializeField] private int attackCountPerPhase = 6;

    public bool IsAttackPhase { get; private set; } = true;
    private int attackCount;

    public void OnAttackExecuted()
    {
        attackCount++;

        if (attackCount >= attackCountPerPhase)
        {
            IsAttackPhase = !IsAttackPhase;
            attackCount = 0;
        }
    }
}
