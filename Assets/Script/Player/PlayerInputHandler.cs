using UnityEngine;

public enum EightDirection
{
    None,
    Up,
    Down,
    Left,
    Right,
    UpLeft,
    UpRight,
    DownLeft,
    DownRight
}
public class PlayerInputHandler
{
    private InputSystem_PlayerActions inputActions;

    private Vector2 moveInput;
    private Vector2 shootDirectionInput;
    private bool jumpPressed, jumpHeld, jumpReleased;
    private bool isCrouching;
    private bool dashPressed;
    private bool fireHeld;
    private bool switchModePressed;
    private bool skillPressed;

    [SerializeField] private float deadZone = 0.2f;

    public float HorizontalInput => moveInput.x;
    public Vector2 MoveInput => moveInput.magnitude < deadZone ? Vector2.zero : moveInput;
    public float MoveInputStrength => moveInput.magnitude < deadZone ? 0f : moveInput.magnitude;
    public float VerticalInput => moveInput.y;
    public bool JumpPressed => jumpPressed;
    public bool JumpHeld => jumpHeld;
    public bool JumpReleased => jumpReleased;
    public bool DashPressed => dashPressed;
    public bool IsCrouching => isCrouching;
    public Vector2 ShootDirectionInput => shootDirectionInput;
    public bool FireHeld => fireHeld;
    public bool SwitchModePressed => switchModePressed;
    public bool SkillPressed => skillPressed;
    public InputSystem_PlayerActions InputAction => inputActions;

    public EightDirection CurrentMoveDirection => GetEightDirection(moveInput);

    public PlayerInputHandler()
    {
        inputActions = new InputSystem_PlayerActions();

        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        inputActions.Player.Jump.started += ctx => jumpPressed = true;
        inputActions.Player.Jump.performed += ctx => jumpHeld = true;
        inputActions.Player.Jump.canceled += ctx => { jumpHeld = false; jumpReleased = true; };

        inputActions.Player.Crouch.performed += ctx => isCrouching = true;
        inputActions.Player.Crouch.canceled += ctx => isCrouching = false;

        inputActions.Player.Dash.performed += ctx => dashPressed = true;

        inputActions.Player.ShootDirection.performed += ctx => shootDirectionInput = ctx.ReadValue<Vector2>();
        inputActions.Player.ShootDirection.canceled += ctx => shootDirectionInput = Vector2.zero;

        inputActions.Player.Fire.performed += ctx => fireHeld = true;
        inputActions.Player.Fire.canceled += ctx => fireHeld = false;

        inputActions.Player.SwitchMode.performed += ctx => switchModePressed = true;
        inputActions.Player.Skill.performed += ctx => skillPressed = true;

        inputActions.Player.Enable();
    }

    public void ResetInputFlags()
    {
        jumpPressed = false;
        jumpReleased = false;
        dashPressed = false;
        switchModePressed = false;
        skillPressed = false;
    }

    public void Disable()
    {
        inputActions?.Player.Disable();
    }

    public void Enable()
    {
        inputActions?.Player.Enable();
    }

    public EightDirection GetEightDirection(Vector2 input)
    {
        if (input.magnitude < deadZone)
            return EightDirection.None;

        float angle = Mathf.Atan2(input.y, input.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360;

        if (angle >= 337.5f || angle < 22.5f)
            return EightDirection.Right;
        else if (angle >= 22.5f && angle < 67.5f)
            return EightDirection.UpRight;
        else if (angle >= 67.5f && angle < 112.5f)
            return EightDirection.Up;
        else if (angle >= 112.5f && angle < 157.5f)
            return EightDirection.UpLeft;
        else if (angle >= 157.5f && angle < 202.5f)
            return EightDirection.Left;
        else if (angle >= 202.5f && angle < 247.5f)
            return EightDirection.DownLeft;
        else if (angle >= 247.5f && angle < 292.5f)
            return EightDirection.Down;
        else // 292.5f ~ 337.5f
            return EightDirection.DownRight;
    }
}
