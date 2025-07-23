using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

[WindowMenuItem("PartSO/Transformers/Astroblast/ConfigSOs")]
[CreateAssetMenu(fileName = "AstroblastLazerGunConfigSO", menuName = "PocketBots/Transformers/Astroblast/AstroblastLazerGunConfigSO")]
public class AstroblastLazerGunConfigSO : ScriptableObject
{
    [SerializeField]
    private float chargingEnergyPhase1Time = 0.5f;
    [SerializeField]
    private float chargingEnergyPhase2Time = 0.5f;
    [MinValue("@chargingEnergyPhase1Time + chargingEnergyPhase2Time")]
    [SerializeField]
    private float reloadTime = 3f;
    [SerializeField]
    private float missileLifetime = 10f;
    [SerializeField]
    private float missileForceSpeed = 25f;
    [SerializeField]
    private bool isExplosionForce;
    [SerializeField]
    private float explosiveDamageRange = 2f;
    [SerializeField]
    private float explosiveForce = 25f;
    [SerializeField]
    private float explosiveUpwardsModifier = 2.5f;
    [SerializeField]
    private AnimationCurve forceImpactCurve;
    [SerializeField]
    private float knockBackForce = 5f;
    [SerializeField]
    private bool isEnableAutoAim = true;
    [SerializeField, ShowIf("isEnableAutoAim")]
    private float autoAimMaxRange = 30f;
    [SerializeField, ShowIf("isEnableAutoAim"), Range(0f, 360f)]
    private float autoAimMaxAngle = 90f;
    [SerializeField, ShowIf("isEnableAutoAim")]
    private float autoAimRotateSpeed = 360f;
    [SerializeField, ShowIf("isEnableAutoAim")]
    private RangeFloatValue minMaxAngleRange;
    [SerializeField, ShowIf("isEnableAutoAim")]
    private bool isAimToGround = true;
    [SerializeField, ShowIf("isAimToGround")]
    private float offsetFromGround = 0.1f;

    [TitleGroup("Animation")]
    [SerializeField]
    private float originEnergyOrbScale = 0f;
    [SerializeField]
    private float targetEnergyOrbScale = 2f;
    [SerializeField]
    private float scaleUpEnergyOrbDuration = 0.25f;
    [SerializeField]
    private Ease scaleUpEnergyOrbEase = Ease.Linear;
    [SerializeField]
    private float scaleDownEnergyOrbDuration = 0.125f;
    [SerializeField]
    private Ease scaleDownEnergyOrbEase = Ease.OutQuad;
    [SerializeField]
    private float recoilStrength = 0.1f;
    [SerializeField]
    private float recoilAnimDuration = 0.1f;
    [SerializeField]
    private int recoilAnimVibrato = 10;
    [SerializeField]
    private float recoilAnimElasticity = 1f;
    [SerializeField]
    private Ease recoilAnimEase = Ease.Linear;
    [SerializeField]
    private bool isEnableCamImpulse = true;
    [SerializeField, ShowIf("isEnableCamImpulse")]
    private float camImpulseDuration = 0.2f;
    [SerializeField, ShowIf("isEnableCamImpulse")]
    private float camImpulseStrength = 1f;

    public float ChargingEnergyPhase1Time => chargingEnergyPhase1Time;
    public float ChargingEnergyPhase2Time => chargingEnergyPhase2Time;
    public float ReloadTime => reloadTime;
    public float MissileLifetime => missileLifetime;
    public float MissileForceSpeed => missileForceSpeed;
    public bool IsExplosionForce => isExplosionForce;
    public float ExplosiveDamageRange => explosiveDamageRange;
    public float ExplosiveForce => explosiveForce;
    public float ExplosiveUpwardsModifier => explosiveUpwardsModifier;
    public AnimationCurve ForceImpactCurve => forceImpactCurve;
    public float KnockBackForce => knockBackForce;
    public bool IsEnableAutoAim => isEnableAutoAim;
    public float AutoAimMaxRange => autoAimMaxRange;
    public float AutoAimMaxAngle => autoAimMaxAngle;
    public float AutoAimRotateSpeed => autoAimRotateSpeed;
    public RangeFloatValue MinMaxAngleRange => minMaxAngleRange;
    public bool IsAimToGround => isAimToGround;
    public float OffsetFromGround => offsetFromGround;

    // Animation
    public float OriginEnergyOrbScale => originEnergyOrbScale;
    public float TargetEnergyOrbScale => targetEnergyOrbScale;
    public float ScaleUpEnergyOrbDuration => scaleUpEnergyOrbDuration;
    public Ease ScaleUpEnergyOrbEase => scaleUpEnergyOrbEase;
    public float ScaleDownEnergyOrbDuration => scaleDownEnergyOrbDuration;
    public Ease ScaleDownEnergyOrbEase => scaleDownEnergyOrbEase;
    public float RecoilStrength => recoilStrength;
    public float RecoilAnimDuration => recoilAnimDuration;
    public int RecoilAnimVibrato => recoilAnimVibrato;
    public float RecoilAnimElasticity => recoilAnimElasticity;
    public Ease RecoilAnimEase => recoilAnimEase;
    public bool IsEnableCamImpulse => isEnableCamImpulse;
    public float CamImpulseDuration => camImpulseDuration;
    public float CamImpulseStrength => camImpulseStrength;
}