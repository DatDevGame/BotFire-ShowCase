using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using UnityEngine;

public class PvPFightingHapticController : MonoBehaviour
{
    [SerializeField]
    private float m_ForceThreshold = 20f;

    private void Awake()
    {
        GameEventHandler.AddActionEvent(RobotStatusEventCode.OnRobotDamaged, OnRobotDamaged);
    }

    private void OnDestroy()
    {
        GameEventHandler.AddActionEvent(RobotStatusEventCode.OnRobotDamaged, OnRobotDamaged);
    }

    private void OnRobotDamaged(object[] parameters)
    {
        // if (parameters == null || parameters.Length <= 0)
        //     return;
        // var damagedRobot = (PBRobot)parameters[0];
        // var damage = (float)parameters[1];
        // var force = (float)parameters[2];
        // var attacker = (IAttackable)parameters[3];

        // if (damage > 0f)
        // {
        //     if (LayerMask.LayerToName(damagedRobot.gameObject.layer) == "PlayerPart" ||
        //      (attacker is PBPart && LayerMask.LayerToName(((PBPart)attacker).gameObject.layer) == "PlayerPart"))
        //     {
        //         if (force >= m_ForceThreshold)
        //         {
        //             HapticManager.Instance.PlayFlashHaptic(HapticTypes.Failure);
        //         }
        //         else
        //         {
        //             HapticManager.Instance.PlayFlashHaptic(HapticTypes.MediumImpact);
        //         }
        //     }
        // }
    }
}