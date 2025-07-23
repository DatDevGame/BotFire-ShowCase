using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Animations;

public class AstroblastLazerGunAutoAim : MonoBehaviour
{
    // I suppose vector of fire point and gun align
    [ShowInInspector]
    private Quaternion originLocalRotation = Quaternion.Euler(new Vector3(89.980217f, 0.000345573353f, 0)); // Dirty set origin rotation
    [ShowInInspector]
    private Quaternion currentLocalRotation = Quaternion.identity;
    [ShowInInspector]
    private Quaternion desiredLocalRotation = Quaternion.identity;
    [ShowInInspector]
    private Transform firePointTransform;
    [ShowInInspector]
    private Transform gunTransform;
    private AstroblastLazerGunBehaviour lazerGun;
    [ShowInInspector]
    private AstroblastLazerGunConfigSO configSO;
    [ShowInInspector]
    private RadarDetector radarDetector;

    [ShowInInspector, BoxGroup("Info")]
    private float DesiredLocalAngleX => (desiredLocalRotation.eulerAngles.y > 0f && desiredLocalRotation.eulerAngles.z > 0f) ? 180f - desiredLocalRotation.eulerAngles.x : desiredLocalRotation.eulerAngles.x;
    [ShowInInspector, BoxGroup("Info")]
    private float SignedAngle => CalcSignedAngle(AimTarget);
    [ShowInInspector, BoxGroup("Info")]
    private Transform AimTarget { get; set; }

    private IEnumerator Start()
    {
        currentLocalRotation = originLocalRotation;
        // Delay 1 frame before robots is spawned completely
        yield return null;
        radarDetector = new RadarDetector();
        radarDetector.Initialize(lazerGun.PbPart.RobotChassis.Robot.AIBotController, configSO.AutoAimMaxRange, configSO.AutoAimMaxAngle);
        radarDetector.IsEnabled = radarDetector.BotController != null;
    }

    private void Update()
    {
        // Save rotation before internal animation update
        currentLocalRotation = gunTransform.localRotation;
    }

    private void LateUpdate()
    {
        if (radarDetector != null)
        {
            var robotDistancePairs = radarDetector.ScanAllRobotsInDetectArea();
            if (robotDistancePairs.Count > 0)
            {
                var closestPair = robotDistancePairs.OrderBy(pair => Mathf.Abs(CalcSignedAngle(pair.Item1.ChassisInstanceTransform))).First();
                AimTarget = closestPair.Item1.ChassisInstanceTransform;
            }
            else
            {
                AimTarget = null;
            }
        }

        Quaternion desiredRotation = originLocalRotation;
        if (AimTarget != null && firePointTransform != null && gunTransform != null)
        {
            float angle = CalcSignedAngle(AimTarget);
            desiredRotation = gunTransform.localRotation * Quaternion.Euler(Vector3.right * angle);
            desiredLocalRotation = desiredRotation;
        }
        var localRotation = Quaternion.RotateTowards(currentLocalRotation, desiredRotation, configSO.AutoAimRotateSpeed * Time.deltaTime);
        var localEulerAngleX = localRotation.eulerAngles.y > 0f && localRotation.eulerAngles.z > 0f ? 180f - localRotation.eulerAngles.x : localRotation.eulerAngles.x;
        localEulerAngleX = Mathf.Clamp(localEulerAngleX, configSO.MinMaxAngleRange.minValue, configSO.MinMaxAngleRange.maxValue);
        localRotation = Quaternion.Euler(Vector3.right * localEulerAngleX);
        gunTransform.localRotation = localRotation;
    }

    private void OnDrawGizmos()
    {
        if (AimTarget == null || firePointTransform == null || gunTransform == null || radarDetector == null || !radarDetector.DrawGizmos)
            return;
        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(AimTarget.position, 0.5f);
        Vector3 closestPoint = CalcClosestPoint(AimTarget);
        Gizmos.DrawWireSphere(closestPoint, 0.5f);
        Gizmos.DrawRay(firePointTransform.position, firePointTransform.forward * 5f);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(firePointTransform.position, closestPoint);
    }

    private Vector3 CalcClosestPoint(Transform target)
    {
        if (target == null)
            return default;
        var plane = new Plane(firePointTransform.right, firePointTransform.position);
        var closestPoint = plane.ClosestPointOnPlane(target.position);
        if (configSO.IsAimToGround && Physics.Raycast(closestPoint, Vector3.down, out RaycastHit hit, float.MaxValue, radarDetector.BotController.CarPhysics.RaycastMask, QueryTriggerInteraction.Collide))
        {
            closestPoint.y = hit.point.y + configSO.OffsetFromGround;
        }
        return closestPoint;
    }

    private float CalcSignedAngle(Transform target)
    {
        if (target == null || firePointTransform == null || gunTransform == null)
            return float.NegativeInfinity;
        Vector3 closestPoint = CalcClosestPoint(AimTarget);
        return Vector3.SignedAngle(firePointTransform.position - gunTransform.position, closestPoint - gunTransform.position, firePointTransform.right);
    }

    public void SetAimTarget(Transform aimTarget)
    {
        AimTarget = aimTarget;
    }

    public void Initialize(AstroblastLazerGunBehaviour lazerGun)
    {
        this.lazerGun = lazerGun;
        this.configSO = lazerGun.ConfigSO;
        firePointTransform = lazerGun.FirePointTransform;
        gunTransform = lazerGun.LazerGunTransform;
    }
}