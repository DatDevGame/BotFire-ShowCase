using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

[WindowMenuItem("PartSO/Transformers/Astroblast/ConfigSOs")]
[CreateAssetMenu(fileName = "RocketArtilleryConfigSO", menuName = "PocketBots/Transformers/Astroblast/RocketArtilleryConfigSO")]
public class RocketArtilleryConfigSO : ScriptableObject
{
    [SerializeField]
    private float reloadTime = 0.1f;
    [SerializeField]
    private float fireRate = 0.1f;
    [SerializeField]
    private float gravityScale = 2.5f;
    [SerializeField]
    private float launchForce = 5f;
    [SerializeField]
    private float searchRange = 10f;
    [SerializeField, Range(0f, 360f)]
    private float searchAngle = 360f;
    [SerializeField]
    private float chasingSpeed = 5f;
    [SerializeField]
    private float maxSpeed = 20f;
    [SerializeField]
    private float rocketLifetime = 10f;
    [SerializeField]
    private AnimationCurve distanceToSpeedCurve;

    [TitleGroup("Animation")]
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

    [TitleGroup("Debug")]
    [SerializeField]
    private bool drawGizmos = true;
    [SerializeField]
    private bool drawSearchZone = false;
    [SerializeField]
    private Color searchZoneColor;
    [SerializeField]
    private Color lineColor;

    public float ReloadTime => reloadTime;
    public float FireRate => fireRate;
    public float GravityScale => gravityScale;
    public float LaunchForce => launchForce;
    public float SearchRange => searchRange;
    public float SearchAngle => searchAngle;
    public float ChasingSpeed => chasingSpeed;
    public float MaxSpeed => maxSpeed;
    public float RocketLifetime => rocketLifetime;
    public AnimationCurve DistanceToSpeedCurve => distanceToSpeedCurve;

    // Animation
    public float RecoilStrength => recoilStrength;
    public float RecoilAnimDuration => recoilAnimDuration;
    public int RecoilAnimVibrato => recoilAnimVibrato;
    public float RecoilAnimElasticity => recoilAnimElasticity;
    public Ease RecoilAnimEase => recoilAnimEase;

    // Debug
    public bool DrawGizmos => drawGizmos;
    public bool DrawSearchZone => drawSearchZone;
    public Color SearchZoneColor => searchZoneColor;
    public Color LineColor => lineColor;
}