using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerStateMachine : MonoBehaviour
{
    private Player player;
    private SkillModeManager skillModeManager;
    private PlayerInputHandler inputHandler;

    private List<IPlayerState> activeStates = new List<IPlayerState>();

    [Header("入力")]
    public float HorizontalInput => inputHandler.HorizontalInput;
    public Vector2 MoveInput => inputHandler.MoveInput;
    public float MoveInputStrength => inputHandler.MoveInputStrength;
    public bool JumpPressed => inputHandler.JumpPressed;
    public bool JumpHeld => inputHandler.JumpHeld;
    public bool JumpReleased => inputHandler.JumpReleased;
    public bool DashPressed => inputHandler.DashPressed;
    public bool IsCrouching => inputHandler.IsCrouching;
    public Vector2 ShootDirectionInput => inputHandler.ShootDirectionInput;
    public bool FireHeld => inputHandler.FireHeld;
    public bool SwitchModePressed => inputHandler.SwitchModePressed;
    public bool SkillPressed => inputHandler.SkillPressed;
    public float VerticalInput => inputHandler.VerticalInput;
    public PlayerInputHandler InputHandler => inputHandler;

    // 8方向入力の公開プロパティ
    public EightDirection CurrentMoveDirection => inputHandler.CurrentMoveDirection;

    public void Init(Player owner)
    {
        player = owner;
        skillModeManager = GetComponent<SkillModeManager>();
        inputHandler = new PlayerInputHandler();

        // 必要な初期ステートをここで登録
        ActivateState(new PlayerMoving());   // 常駐移動
        ActivateState(new NohMaskState());
        ActivateState(new NohMaskSkill());
    }

    public void StateSetting()
    {
        // スタン状態がアクティブかチェック
        var stunState = GetState<PlayerStunned>();
        if (stunState != null)
        {
            stunState.Update();
            return;
        }

        HandleInput();

        // アクティブステートの処理
        for (int i = 0; i < activeStates.Count; i++)
        {
            var state = activeStates[i];
            state.HandleInput();
            state.Update();
        }
    }

    void FixedUpdate()
    {
        Debug.Log("【ステートマシン】FixedUpdate 実行中...");

        for (int i = 0; i < activeStates.Count; i++)
        {
            activeStates[i].FixedUpdate();
        }
    }

    private void HandleInput()
    {
        // ジャンプ
        if (inputHandler.JumpPressed && player.IsGrounded() && !IsStateActive<PlayerJumping>())
        {
            ActivateState(new PlayerJumping());
        }

        // ダッシュ
        if (inputHandler.HorizontalInput != 0f && inputHandler.DashPressed && !IsStateActive<PlayerDash>())
        {
            ActivateState(new PlayerDash());
        }

        // しゃがみ
        if (inputHandler.IsCrouching && player.IsGrounded() && !IsStateActive<PlayerCrouching>())
        {
            ActivateState(new PlayerCrouching());
        }

        // しゃがみ解除
        if (!inputHandler.IsCrouching && IsStateActive<PlayerCrouching>())
        {
            DeactivateState(GetState<PlayerCrouching>());
        }

        // モード切替
        if (inputHandler.SwitchModePressed)
        {
            skillModeManager.SwitchMode();
        }

        // 梯子登り開始
        if (player.IsTouchingLadder && inputHandler.VerticalInput > 0.1f && !IsStateActive<PlayerClimbingLadder>())
        {
            ActivateState(new PlayerClimbingLadder());
        }
    }

    // 即時ステート有効化
    public void ActivateState(IPlayerState state)
    {
        var stateType = state.GetType();
        bool isDuplicate = activeStates.Exists(s => s.GetType() == stateType);

        if (!isDuplicate)
        {
            activeStates.Add(state);
            state.Init(player, this); // 即時Init
            Debug.Log($"[StateMachine] State activated immediately: {stateType.Name}");
        }
    }

    public void DeactivateState(IPlayerState state)
    {
        if (activeStates.Contains(state))
        {
            state.Remove();
            activeStates.Remove(state);
        }
    }

    public bool IsStateActive<T>() where T : IPlayerState
    {
        for (int i = 0; i < activeStates.Count; i++)
        {
            if (activeStates[i] is T) return true;
        }
        return false;
    }

    public T GetState<T>() where T : IPlayerState
    {
        for (int i = 0; i < activeStates.Count; i++)
        {
            if (activeStates[i] is T tState) return tState;
        }
        return default;
    }

    // 抽象基底クラスを持つIPlayerStateのリストから、
    // 指定した抽象基底クラス型（T）に該当する派生クラスのインスタンスを取得する
    public T GetStateByBaseClass<T>() where T : class, IPlayerState
    {
        return activeStates.OfType<T>().FirstOrDefault();
    }

    private void LateUpdate()
    {
        // 入力フラグのリセットはLateUpdateで安全に行う
        inputHandler.ResetInputFlags();
    }

    private void OnDisable()
    {
        inputHandler.Disable();
    }
}