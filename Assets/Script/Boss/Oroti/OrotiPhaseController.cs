using UnityEngine;

public class OrotiPhaseController : MonoBehaviour
{
	[SerializeField] private int attackCountPerPhase = 6;
	[SerializeField] private float restTime = 3f;

	public bool IsAttackPhase { get; private set; } = true;

	private int attackCount;
	private float restTimer;

	private void Update()
	{
		if (IsAttackPhase) return;

		restTimer -= Time.deltaTime;
		if (restTimer <= 0f)
		{
			IsAttackPhase = true;
			attackCount = 0;
		}
	}

	public void OnAttackExecuted()
	{
		attackCount++;
		Debug.Log($"Attack Count : {attackCount}");
		if (attackCount >= attackCountPerPhase)
		{
			IsAttackPhase = false;
			restTimer = restTime;
		}
	}
}
