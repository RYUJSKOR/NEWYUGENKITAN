using System.Collections.Generic;
using System;
using UnityEngine;

public class SkillModeManager : MonoBehaviour
{
    private PlayerStateMachine stateMachine;

    private SkillType currentType = SkillType.Noh;

    private Vector3 lastShootingDir = Vector3.right;

    float inheritedGauge;

    bool switchMode = true;

    private readonly Dictionary<SkillType, Type> shootingMap = new()
    {
        { SkillType.Noh, typeof(NohMaskState) },
        { SkillType.Demon, typeof(DemonState) },
        { SkillType.Fox, typeof(FoxState) }
    };

    private readonly Dictionary<SkillType, Type> skillMap = new()
    {
        { SkillType.Noh, typeof(NohMaskSkill) },
        { SkillType.Demon, typeof(DemonSkill) },
        { SkillType.Fox, typeof(FoxSkill) }
    };

    private void Start()
    {
        stateMachine = GameObject.Find("Player").GetComponent<PlayerStateMachine>();

        if (stateMachine == null)
        {
            Debug.Log("NULL" + stateMachine);
        }
        else
        {
            Debug.Log("éÊìæ" + stateMachine);

        }

    }

    public void SwitchMode()
    {
        if (!switchMode) { return; }

        inheritedGauge = 0f;
        var currentSkill = stateMachine.GetStateByBaseClass<BulletSkill>();
        if (currentSkill != null)
        {
            inheritedGauge = currentSkill.GetGauge();
        }

        var currentShoot = stateMachine.GetStateByBaseClass<PlayerShooting>();
        if (currentShoot != null)
        {
            lastShootingDir = currentShoot.shootingDirection;
        }

        RemoveStates(currentType);

        currentType = (SkillType)(((int)currentType + 1) % Enum.GetValues(typeof(SkillType)).Length);

        AddStates(currentType);

        Debug.Log($"[SkillModeManager] Switched to: {currentType}");
    }

    private void RemoveStates(SkillType type)
    {
        var shoot = stateMachine.GetStateByBaseClass<PlayerShooting>();
        var skill = stateMachine.GetStateByBaseClass<BulletSkill>();

        if (shoot != null) stateMachine.DeactivateState(shoot);
        if (skill != null) stateMachine.DeactivateState(skill);
    }

    private void AddStates(SkillType type)
    {
        var shoot = (IPlayerState)Activator.CreateInstance(shootingMap[type]);
        var skill = (BulletSkill)Activator.CreateInstance(skillMap[type]);

        skill.SetGauge(inheritedGauge); // ÉQÅ[ÉWà¯Ç´åpÇ¨

        if (shoot is PlayerShooting ps)
        {
            ps.shootingDirection = lastShootingDir;
        }

        stateMachine.ActivateState(shoot);
        stateMachine.ActivateState(skill);
    }

    public void SetSwitchMode(bool switchmode) { switchMode = switchmode; }
}