using UnityEngine;

public class NohMaskState : PlayerShooting
{
    private Player player;

    public override void Init(Player player, PlayerStateMachine playerStateMachine)
    {
        this.player = player;

        base.Init(player, playerStateMachine);

        player.SetActiveMask(MaskType.Noh);

        shooting.SetBulletByName("NohMaskBullet");
        shooting.SetBulletSpeed(15f);
        SetShootInterval(0.15f);
        Debug.Log("NohMaskState èâä˙âªäÆóπ: PlayerBullet égóp");
    }

    public override void Remove()
    {
        base.Remove();
        player.SetActiveMask(MaskType.None);
    }

    protected override void Fire()
    {
        shooting.SetDirection(shootingDirection);
        shooting.RequestShoot();
        SE.Play("Player.NohMaskShoot");
    }
}
