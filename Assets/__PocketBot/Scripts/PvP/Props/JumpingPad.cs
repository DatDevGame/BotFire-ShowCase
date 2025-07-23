using System.Collections;
using System.Collections.Generic;
using LatteGames;
using LatteGames.Template;
using Sirenix.OdinInspector;
using UnityEngine;

public class JumpingPad : UtilityProps
{
    private const float TweentyFrame = 20 / 60f;
    private const float SpringAnimDuration = AnimationDuration.TINY;
    private const float BlendShapeScale = 100;
    private const string SpringBlendShapeKey = "spring";

    [SerializeField]
    private float cooldownTime = 0.5f;
    [SerializeField]
    private Vector3 forceVector = Vector3.forward + Vector3.up;
    [SerializeField]
    private SkinnedMeshRenderer padRenderer;

    private float lastTimePushAnyRobot;
    private Coroutine springAnimCoroutine;
    private Dictionary<Rigidbody, float> lastTimePushRobotDict = new Dictionary<Rigidbody, float>();


    [PropertyRange(0, 1), ShowInInspector]
    public float Spring
    {
        get
        {
            return GetSpring();
        }
        set
        {
            SetSpring(value);
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        var pbPart = collider.GetComponent<PBPart>();
        if (collider == null || pbPart == null || pbPart.RobotBaseBody != collider.attachedRigidbody)
        {
            return;
        }
        PushTarget(pbPart.RobotChassis.Robot);
    }

    private void OnTriggerExit(Collider collider)
    {
        var pbPart = collider.GetComponent<PBPart>();
        if (collider == null || pbPart == null || pbPart.RobotBaseBody != collider.attachedRigidbody)
        {
            return;
        }
        RemoveEnterPointRobot(pbPart.RobotChassis.Robot);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, GetForceVector());
    }

    private IEnumerator PlaySpringAnim_CR()
    {
        yield return CommonCoroutine.LerpFactor(SpringAnimDuration, t =>
        {
            SetSpring(t);
        });
        yield return new WaitForSeconds(SpringAnimDuration / 2f);
        yield return CommonCoroutine.LerpFactor(SpringAnimDuration, t =>
        {
            SetSpring(1f - t);
        });
    }

    private void PlaySpringAnim()
    {
        if (springAnimCoroutine != null)
            StopCoroutine(springAnimCoroutine);
        springAnimCoroutine = StartCoroutine(PlaySpringAnim_CR());
    }

    public void PushTarget(PBRobot robot)
    {
        if (!IsAbleToPush(robot.ChassisInstance.RobotBaseBody))
            return;
        lastTimePushAnyRobot = Time.time;
        lastTimePushRobotDict.Set(robot.ChassisInstance.RobotBaseBody, Time.time);
        var carPhysics = robot.ChassisInstance.CarPhysics;
        carPhysics.LockBrakeTime = SpringAnimDuration * 2.5f;
        var targetRb = robot.ChassisInstance.RobotBaseBody;
        targetRb.AddForce(GetForceVector(), ForceMode.VelocityChange);
        targetRb.angularVelocity = Vector3.zero;
        PlaySpringAnim();
        AddEnterPointRobot(robot);
        SoundManager.Instance.PlaySFX(GeneralSFX.UIDropBox);
        //Debug.Log($"Push {targetRb} - {Time.time} - {Time.fixedTime}");
    }

    public bool IsAbleToPush(Rigidbody targetRb)
    {
        return Time.time - lastTimePushAnyRobot >= cooldownTime && Time.time - lastTimePushRobotDict.Get(targetRb) > TweentyFrame;
    }

    public void SetSpring(float spring)
    {
        padRenderer.SetBlendShapeWeight(padRenderer.sharedMesh.GetBlendShapeIndex(SpringBlendShapeKey), spring * BlendShapeScale);
    }

    public float GetSpring()
    {
        return padRenderer.GetBlendShapeWeight(padRenderer.sharedMesh.GetBlendShapeIndex(SpringBlendShapeKey)) / BlendShapeScale;
    }

    public Vector3 GetForceDirection()
    {
        return transform.TransformDirection(forceVector.normalized);
    }

    public Vector3 GetForceVector()
    {
        return transform.TransformDirection(forceVector.normalized) * forceVector.magnitude;
    }
}