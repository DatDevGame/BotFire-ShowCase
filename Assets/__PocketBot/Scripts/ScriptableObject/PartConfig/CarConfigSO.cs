using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "CarConfigSO", menuName = "PocketBots/PartConfig/CarConfigSO")]
public class CarConfigSO : ScriptableObject
{
    [BoxGroup("Gravity"), SerializeField] private float gravityFactor = 1.35f;
    [BoxGroup("Accelerate"), SerializeField] private float baseTopSpeed = 24f;
    [BoxGroup("Accelerate")] public float accelerateSpeed = 2f;
    [BoxGroup("Accelerate")] public AnimationCurve powerCurve;
    [BoxGroup("Steering")] public float maxSteerRotation = 50;
    [BoxGroup("Steering")] public float minAngleToRotateCar = 70;
    [BoxGroup("Suspension"), SerializeField] private float springDamper = 20;
    [BoxGroup("Suspension"), SerializeField] private float springStrength = 500;
    [BoxGroup("Braking")] public AnimationCurve brakingCurve;
    [BoxGroup("Anti Sliding")] public AnimationCurve antiSlidingForceCurve;
    [SerializeField] private AnimationCurve suspensionRestDistMultiplierCurve;

    public float GravityFactor { get => gravityFactor * MovementGlobalConfigs.gravityScale; }
    public float BaseTopSpeed { get => baseTopSpeed;}
    public float SpringDamper { get => springDamper * MovementGlobalConfigs.gravityScale; }
    public float SpringStrength { get => springStrength * MovementGlobalConfigs.gravityScale; }
    public AnimationCurve SuspensionRestDistMultiplierCurve => suspensionRestDistMultiplierCurve;
}