using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

[WindowMenuItem("PartSO/Transformers", assetFolderPath: "Assets/__PocketBot/RobotParts/ScriptableObjects/Transformers", mode: WindowMenuItemAttribute.Mode.Multiple, sortByName: true)]
[CreateAssetMenu(fileName = "BodyTransformerConfigSO", menuName = "PocketBots/Transformers/BodyTransformerConfigSO")]
public class BodyTransformerConfigSO : ScriptableObject
{
    [SerializeField]
    private BodyTransformer.State m_DefaultState = BodyTransformer.State.Idle;
    [SerializeField]
    private AnimatorUpdateMode m_UpdateMode = AnimatorUpdateMode.Normal;
    [SerializeField]
    private bool m_IsAbleToTransform = true;
    [SerializeField]
    private int m_MaxSlowMotionTimes = 1;
    [SerializeField]
    private int m_MaxCinematicCameraFocusTimes = 1;
    [SerializeField]
    private float m_IdleToAttackTransformationDuration = 2f;
    [SerializeField]
    private float m_AttackToIdleTransformationDuration = 2f;
    [SerializeField]
    private float m_IdleTransformationRange = 30f;
    [SerializeField]
    private float m_AttackTransformationRange = 30f;
    [SerializeField]
    private float m_IdleToAttackTimeThreshold = 0.25f;
    [SerializeField]
    private float m_AttackToIdleTimeThreshold = 0.25f;
    [SerializeField, Range(0f, 1f)]
    private float m_AttackStateSpeed = 0.75f;
    [SerializeField, Range(0f, 1f)]
    private float m_ExplodeForceMultiplier = 1f;
    [SerializeField]
    private float m_TimeScale = 0.1f;
    [SerializeField]
    private float m_JumpStrength = 2f;
    [SerializeField]
    private float m_MaxJumpStrength = 20f;
    [SerializeField, Range(0f, 90f)]
    private float m_JumpAngle = 30f;
    [SerializeField]
    private int m_WeaponPreviewCycleTimes = 2;
    [SerializeField]
    private Vector3 m_IdleCenterOfMassOffset = Vector3.zero;
    [SerializeField]
    private Vector3 m_AttackCenterOfMassOffset = Vector3.zero;
    [SerializeField]
    private AnimationClip m_IdleAnimClip;
    [SerializeField]
    private AnimationClip m_AttackAnimClip;
    [SerializeField]
    private AnimationClip m_IdleToAttackAnimClip;
    [SerializeField]
    private AnimationClip m_AttackToIdleAnimClip;

    public BodyTransformer.State defaultState => m_DefaultState;
    public AnimatorUpdateMode updateMode => m_UpdateMode;
    public bool isAbleToTransform => m_IsAbleToTransform;
    public int maxSlowMotionTimes => m_MaxSlowMotionTimes;
    public int maxCinematicCameraFocusTimes => m_MaxCinematicCameraFocusTimes;
    public float idleToAttackTransformationDuration => m_IdleToAttackTransformationDuration;
    public float attackToIdleTransformationDuration => m_AttackToIdleTransformationDuration;
    public float idleTransformationRange => m_IdleTransformationRange;
    public float attackTransformationRange => m_AttackTransformationRange;
    public float idleToAttackTimeThreshold => m_IdleToAttackTimeThreshold;
    public float attackToIdleTimeThreshold => m_AttackToIdleTimeThreshold;
    public float attackStateSpeed => m_AttackStateSpeed;
    public float explodeForceMultiplier => m_ExplodeForceMultiplier;
    public float idleToAttackAnimClipLength => m_IdleToAttackAnimClip.length;
    public float attackToIdleAnimClipLength => m_AttackToIdleAnimClip.length;
    public float timeScale => m_TimeScale;
    public float jumpStrength => m_JumpStrength;
    public float maxJumpStrength => m_MaxJumpStrength;
    public float jumpAngle => m_JumpAngle;
    public int weaponPreviewCycleTimes => m_WeaponPreviewCycleTimes;
    public Vector3 idleCenterOfMassOffset => m_IdleCenterOfMassOffset;
    public Vector3 attackCenterOfMassOffset => m_AttackCenterOfMassOffset;
    public AnimationClip idleAnimClip => m_IdleAnimClip;
    public AnimationClip attackAnimClip => m_AttackAnimClip;
}