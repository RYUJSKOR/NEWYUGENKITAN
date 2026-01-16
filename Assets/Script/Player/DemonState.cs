using UnityEngine;

public class DemonState : PlayerShooting
{
    [Header("ŠgUƒVƒ‡ƒbƒgİ’è")]
    public int pelletCount = 4; // ’e‚Ì”
    public float spreadAngle = 40f; // ŠgUŠp“xi}‚Å•ªŠ„j

    private Player player;

    public override void Init(Player player, PlayerStateMachine playerStateMachine)
    {
        this.player = player;
        base.Init(player, playerStateMachine);

        player.SetActiveMask(MaskType.Oni);

        shooting.SetBulletByName("DemonBullet");
        shooting.SetBulletSpeed(15f);
        SetShootInterval(0.15f);
        Debug.Log("DemonState ‰Šú‰»Š®—¹: DemonBullet g—p");
    }

    public override void Remove()
    {
        base.Remove();
        player.SetActiveMask(MaskType.None);
    }

    protected override void Fire()
    {
        float angleStep = spreadAngle / (pelletCount - 1);
        float startAngle = -spreadAngle / 2f;

        for (int i = 0; i < pelletCount; i++)
        {
            float angle = startAngle + (angleStep * i);
            Vector3 spreadDir = Quaternion.Euler(0, 0, angle) * shootingDirection.normalized;

            SE.Play("Player.DemonShoot");
            shooting.SetDirection(spreadDir);
            shooting.RequestShoot();
        }
    }
}
