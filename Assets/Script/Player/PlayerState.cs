using UnityEngine;

public interface IPlayerState
{
	void Init(Player player, PlayerStateMachine playerStateMachine);
	void Update();
    void FixedUpdate();
    void HandleInput();
	void Remove();
}
