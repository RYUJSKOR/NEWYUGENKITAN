/// <summary>
/// 首用のダメージ中継ヘルスマネージャー
/// Playerの攻撃はここを叩き、実体はRootに転送される
/// </summary>
public class OrotiNeckHealthRelay : CharacterHealthManager
{
	private OrotiController oroti;

	protected override void Awake()
	{
		// CharacterHealthManager の Awake を呼ばない
		// （maxHealth 初期化などを無効化するため）
		oroti = GetComponentInParent<OrotiController>();
	}

	public override void ApplyDamage(float value, bool bypassInvincibility = false)
	{
		if (oroti == null) return;

		// 完全中継（無敵・HP計算はRootに集約）
		oroti.ApplyDamageToBoss(value);
	}

	// 以下は念のため無効化
	public override void Recovery(float value) { }
	public override void ResetHealth() { }
	public override void ActivateInvincibility(float duration) { }
	public override void InstantKill() { }
}