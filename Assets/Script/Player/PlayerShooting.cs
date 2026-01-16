using System; using UnityEngine;  public abstract class PlayerShooting : IPlayerState {     private Player player;     private PlayerStateMachine playerStateMachine;     protected Shooting shooting; // 弾の発射処理クラス     protected SEController SE;      protected float shootTimer = 0f;     protected float shootInterval = 0.15f;     public Vector3 shootingDirection = Vector3.zero;      Vector2 input;
    Vector3 direction;
    EightDirection shootDirEnum;      private bool wasGroundedLastFrame = true;      public virtual void Init(Player player, PlayerStateMachine playerStateMachine)     {         this.player = player;         this.playerStateMachine = playerStateMachine;         this.shooting = player.GetComponent<Shooting>();         SE = this.player.GetComponent<SEController>();         shootTimer = 0f;

        input = playerStateMachine.ShootDirectionInput;         direction = new Vector3(input.x, input.y, 0f);         shootDirEnum = playerStateMachine.InputHandler.GetEightDirection(input);          if (shootingDirection == Vector3.zero)         {             shootingDirection = new Vector3(1, 0, 0);         }     }      public void FixedUpdate()     {         // このステートではFixedUpdateで何もしない     }      public virtual void HandleInput()     {         HandleDirectionInput();     }      private void HandleDirectionInput()     {
        Vector2 input = playerStateMachine.ShootDirectionInput;
        Vector3 direction = new Vector3(input.x, input.y, 0f);

        // --- しゃがみ中なら上下入力を無視して左右のみ ---
        var crouchingState = playerStateMachine.GetState<PlayerCrouching>();
        if (crouchingState != null && crouchingState.GetCrouching())
        {
            // 左右入力があるならその方向、無ければ前回の方向を維持
            if (input.x > 0) direction = Vector3.right;
            else if (input.x < 0) direction = Vector3.left;
            else direction = shootingDirection; // 入力が無ければ前回の方向のまま
        }
        else
        {
            // 通常時の下撃ち制御
            if (player.IsGrounded() && shootDirEnum == EightDirection.Down)
            {
                direction.y = 0f; // 地上では真下撃ち禁止
            }
        }

        // 入力があれば shootingDirection 更新
        if (direction != Vector3.zero)
        {
            shootingDirection = direction.normalized;
        }
    }      public virtual void Update()     {         shootTimer += Time.deltaTime;         wasGroundedLastFrame = player.IsGrounded();          if (playerStateMachine.FireHeld && shootTimer >= shootInterval && !playerStateMachine.DashPressed)         {             Fire();             shootTimer = 0f;         }     }      public void SetShootInterval(float Interval)     {         shootInterval = Interval;     }      public float GetShootInterval() => shootInterval;       protected abstract void Fire();      public virtual void Remove() { }  }