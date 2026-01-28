using UnityEngine;

public class FoxState : PlayerShooting
{
    private Player player;

    public override void Init(Player player, PlayerStateMachine playerStateMachine)
    {
        this.player = player;
        base.Init(player, playerStateMachine);

        player.SetActiveMask(MaskType.Fox);

        shooting.SetBulletByName("FoxBullet");
        shooting.SetBulletSpeed(13f);
        SetShootInterval(0.15f);
        Debug.Log("FoxState èâä˙âªäÆóπ: FoxBullet égóp");
    }

    public override void Remove()
    {
        base.Remove();
        player.SetActiveMask(MaskType.None);
    }

    protected override void Fire()
    {
        shooting.SetDirection(shootingDirection);
        SE.Play("Player.FoxShoot");
        shooting.RequestShoot();
    }
}
