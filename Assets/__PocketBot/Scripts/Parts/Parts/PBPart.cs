using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PBPart : MonoBehaviour, IDamagable, IAttackable
{
    public event Action<PartDamagedEventData> OnPartReceiveDamage = delegate { };
    public event Action<PartDamagedEventData> OnPartCauseDamage = delegate { };

    public virtual float DamageMultiplier { get; set; } = 1f;
    public virtual float Resistance { get; set; } = 0;
    public virtual PBPartSO PartSO { get; set; }
    public virtual IPartStats ManualRobotStats { get; set; }

    public virtual Rigidbody RobotBaseBody { get; }
    public virtual PBChassis RobotChassis { get; }

    CommonGun commonGun;

    protected virtual void Awake()
    {
        commonGun = GetComponent<CommonGun>();
    }

    public virtual void ReceiveDamage(IAttackable attacker, float forceTaken)
    {
        if (attacker is MonoBehaviour mono && IsSameTeamLayer(mono.gameObject, gameObject))
            return;
        var takenDamage = attacker.GetDamage() * (1 - Resistance);
        if (takenDamage <= 0f)
            return;
        if (attacker is ConnectedPartTwoPhase connectedPartTwoPhase)
        {
            IPhaseTwoAttackable phaseTwoAttackable = connectedPartTwoPhase.GetIPhaseTwoAttackable();
            if (phaseTwoAttackable != null)
            {
                if (phaseTwoAttackable.IsActive())
                    takenDamage = takenDamage * phaseTwoAttackable.GetPercentDamage();
            }
        }

        if (attacker is ScorpionBehavior scorpion)
        {
            if (scorpion != null)
                takenDamage = scorpion.MainRobot.MaxHealth * scorpion.GetDamage() / 100;
        }

        var damagedEventData = new PartDamagedEventData()
        {
            receiver = this,
            attacker = attacker,
            damageTaken = takenDamage,
            forceTaken = forceTaken
        };
        OnPartReceiveDamage(damagedEventData);
        if (attacker is PBPart attackerPartNotice)
            attackerPartNotice.OnPartCauseDamage(damagedEventData);
        // LGDebug.Log($"{name} {RobotChassis} received damage from {attacker} - {damagedEventData.damageTaken} - {damagedEventData.forceTaken} - {Time.time}");
    }
    private bool IsSameTeamLayer(GameObject a, GameObject b)
    {
        string layerA = LayerMask.LayerToName(a.layer);
        string layerB = LayerMask.LayerToName(b.layer);

        // Compare by prefix: e.g., "PlayerPart", "EnemyPart"
        return GetTeamPrefix(layerA) == GetTeamPrefix(layerB);
    }

    private string GetTeamPrefix(string layerName)
    {
        if (layerName.StartsWith("PlayerPart"))
            return "PlayerPart";
        if (layerName.StartsWith("EnemyPart"))
            return "EnemyPart";
        return "Unknown";
    }

    public virtual float GetDamage()
    {
        float rawAttack;
        float attackMultiplier = RobotChassis == null ? 1f : RobotChassis.Robot.AtkMultiplier;
        if (ManualRobotStats != null)
        {
            rawAttack = ManualRobotStats.GetAttack().value * attackMultiplier;
        }
        else if (PartSO != null)
        {
            rawAttack = PartSO.GetAttack().value * attackMultiplier;
        }
        else
            rawAttack = 0;
        float perHitRatio = PartSO != null ? PartSO.Stats.DamagePerHitRatio : 1;
        var result = rawAttack * perHitRatio * DamageMultiplier;
        if (commonGun != null)
        {
            result /= commonGun.BurstCount * commonGun.SpreadCount;
        }
        return result;
    }

    public class PartDamagedEventData
    {
        public PBPart receiver;
        public IAttackable attacker;
        public float damageTaken;
        public float forceTaken;
    }
}