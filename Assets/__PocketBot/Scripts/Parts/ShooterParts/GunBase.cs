using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using LatteGames.Template;
using Sirenix.OdinInspector;
using UnityEngine;

public class GunBase : PartBehaviour
{
    [Header("Base Gun Configs")]
    [SerializeField] public float reload = 1f;
    [SerializeField] public float aimRange = 20f;
    [SerializeField] public float aimAngle = 20f;
    [SerializeField] public float rotateSpeed = 100f;
    [SerializeField] protected bool isPlayHitSound = false;
    [SerializeField, ShowIf("isPlayHitSound")] protected SoundID muzzleSoundID;
    [Header("Base Bullet Configs")]
    [SerializeField] public float bulletSpeed = 10f;
    [Header("Base Gun Refs")]
    [SerializeField] protected Transform shootingPoint;

    protected AimController aimController;
    [HideInInspector] public int hitLayerMask = 0;
    [HideInInspector] public int hitLayerMaskPartOnly = 0;
    [HideInInspector] public int hitLayerMaskWallOnly = 0;

    public AimController AimController => aimController;

    protected virtual void Awake()
    {
        aimController = GetComponentInParent<AimController>();
        aimController.RegisterGun(this);
        GameEventHandler.AddActionEvent(PBLevelEventCode.OnLevelEnded, OnLevelEnded);
    }

    protected virtual void OnEnable()
    {
        hitLayerMask = LayerMask.GetMask("Ground", "Wall", gameObject.layer == LayerMask.NameToLayer("PlayerPart") ? "EnemyPart" : "PlayerPart");
        hitLayerMaskWallOnly = LayerMask.GetMask("Ground", "Wall");
        hitLayerMaskPartOnly = LayerMask.GetMask(gameObject.layer == LayerMask.NameToLayer("PlayerPart") ? "EnemyPart" : "PlayerPart");
    }

    protected virtual void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(PBLevelEventCode.OnLevelEnded, OnLevelEnded);
    }

    protected virtual void OnLevelEnded()
    {
        StopAllCoroutines();
        enabled = false;
    }

    public virtual bool CanShootTarget(AimController target)
    {
        if (target == null) return false;
        var diff = shootingPoint.position.DiffOnPlane(target.transform.position);
        return diff.sqrMagnitude <= aimRange * aimRange
            && Vector3.Angle(shootingPoint.forward, diff) <= aimAngle / 2;
    }

    protected virtual void FixedUpdate()
    {
        if (aimController != null)
        {
            if (aimController.aimTarget != null && aimController.isFacingToTarget)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(transform.position.DiffOnPlane(aimController.aimTarget.transform.position)), rotateSpeed * Time.fixedDeltaTime);
            }
            else
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, aimController.transform.rotation, rotateSpeed * Time.fixedDeltaTime);
            }
        }
    }

    protected virtual void OnDrawGizmosSelected()
    {
        if (shootingPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(shootingPoint.position, shootingPoint.position + shootingPoint.forward * aimRange);
            Gizmos.DrawLine(shootingPoint.position, shootingPoint.position + Quaternion.AngleAxis(aimAngle / 2, Vector3.up) * shootingPoint.forward * aimRange);
            Gizmos.DrawLine(shootingPoint.position, shootingPoint.position + Quaternion.AngleAxis(-aimAngle / 2, Vector3.up) * shootingPoint.forward * aimRange);
        }
    }

    protected void PlayMuzzleSound()
    {
        if (isPlayHitSound)
        {
            SoundManager.Instance.PlaySFX_3D_Pitch(muzzleSoundID, shootingPoint.position, true);
        }
    }
}
