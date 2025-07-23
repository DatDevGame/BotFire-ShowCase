using System;
using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using UnityEngine;

public class PvPSlowMotion : MonoBehaviour
{
    // [SerializeField]
    // private AnimationCurveVariable m_TimeScaleCurveVariable;

    // private float m_OriginTimeScale;
    // private bool m_IsTransformationSlowMotion;
    // private bool m_FatalitySlowMotion;
    // private Coroutine m_SlowMotionCoroutine;

    private void Awake()
    {
        // ObjectFindCache<PvPSlowMotion>.Add(this);
        // GameEventHandler.AddActionEvent(PBPvPEventCode.OnShakeCamera, StartSloMo);
    }

    private void OnDestroy()
    {
        // ObjectFindCache<PvPSlowMotion>.Remove(this);
        // StopSloMo();

        // GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnShakeCamera, StartSloMo);
    }

    private bool IsAbleToSloMo(PBChassis receiverChassis, PBChassis attackerChassis, float force)
    {
        // bool isValid = false;
        // var receiverVelLength = receiverChassis.RobotBaseBody.velocity.magnitude;
        // var receiverVelDotProduct = Vector3.Dot(Vector3.up, receiverChassis.RobotBaseBody.velocity.normalized);
        // bool isReceiverValid = receiverVelLength >= 5f && receiverVelDotProduct > 0.35f;
        // var attackerVelLength = attackerChassis.RobotBaseBody.velocity.magnitude;
        // var attackerVelDotProduct = Vector3.Dot(Vector3.up, attackerChassis.RobotBaseBody.velocity.normalized);
        // var isAttackerValid = attackerVelLength >= 5f && attackerVelDotProduct > 0.35f;
        // if (isReceiverValid || isAttackerValid)
        // {
        //     isValid = true;
        // }
        //LGDebug.Log($"IsAbleToSloMo {isValid} - {force} - {receiverChassis.RobotBaseBody.velocity.magnitude} {receiverChassis.RobotBaseBody.velocity} ({Vector3.Dot(receiverChassis.RobotBaseBody.velocity.normalized, Vector3.up)}) - {attackerChassis.RobotBaseBody.velocity.magnitude} {attackerChassis.RobotBaseBody.velocity} ({Vector3.Dot(attackerChassis.RobotBaseBody.velocity.normalized, Vector3.up)})");
        return false;
    }

    private void StartSloMo(object[] parameters)
    {
        // if (m_IsTransformationSlowMotion || m_FatalitySlowMotion)
        //     return;
        // if (parameters[0] is not PBChassis receiverChassis) return;
        // if (parameters[1] is not PBChassis attackerChassis) return;
        // if (parameters[2] is not float force) return;
        // if (parameters[3] is not bool isIgnoreCondition) return;
        // if (m_SlowMotionCoroutine != null)
        // {
        //     StopCoroutine(m_SlowMotionCoroutine);
        //     StopSloMo();
        // }
        // m_OriginTimeScale = Time.timeScale;
        // var animCurve = m_TimeScaleCurveVariable.value;
        // var duration = m_TimeScaleCurveVariable.value.keys[^1].time;
        // m_SlowMotionCoroutine = StartCoroutine(StartSloMo_CR(receiverChassis, attackerChassis, force, isIgnoreCondition, duration, t =>
        // {
        //     var timeScale = animCurve.Evaluate(t * duration);
        //     Time.timeScale = timeScale;
        // }));
    }

    private IEnumerator StartSloMo_CR(PBChassis receiverChassis, PBChassis attackerChassis, float force, bool isIgnoreCondition, float duration, Action<float> callback)
    {
        yield break;
        // yield return new WaitForSeconds(0.1f);
        // if (!isIgnoreCondition && !IsAbleToSloMo(receiverChassis, attackerChassis, force))
        // {
        //     m_SlowMotionCoroutine = null;
        //     yield break;
        // }
        // // var robots = PBFightingStage.Instance?.GetAllRobots();
        // // if (robots != null)
        // // {
        // //     foreach (var robot in robots)
        // //     {
        // //         foreach (var rb in robot.ChassisInstance.Rigidbodies)
        // //         {
        // //             if (rb != null)
        // //                 rb.interpolation = RigidbodyInterpolation.Interpolate;
        // //         }
        // //     }
        // // }
        // yield return new WaitForSeconds(0.1f);
        // float t = 0.0f;
        // callback(t / duration);
        // while (t < duration)
        // {
        //     yield return null;
        //     t += Time.deltaTime;
        //     callback(t / duration);
        // }
        // callback(1);
        // StopSloMo();
    }

    public void StopSloMo()
    {
        // if (m_SlowMotionCoroutine != null)
        // {
        //     StopCoroutine(m_SlowMotionCoroutine);
        //     m_SlowMotionCoroutine = null;
        //     Time.timeScale = m_OriginTimeScale;
        //     // var robots = PBFightingStage.Instance?.GetAllRobots();
        //     // if (robots != null)
        //     // {
        //     //     foreach (var robot in robots)
        //     //     {
        //     //         foreach (var rb in robot.ChassisInstance.Rigidbodies)
        //     //         {
        //     //             if (rb != null)
        //     //                 rb.interpolation = RigidbodyInterpolation.None;
        //     //         }
        //     //     }
        //     // }
        // }
    }

    public void StartSloMoTransformer(float timeScale)
    {
        // StopSloMo();
        // m_OriginTimeScale = Time.timeScale;
        // Time.timeScale = timeScale;
        // Time.fixedDeltaTime *= timeScale;
        // m_IsTransformationSlowMotion = true;
    }

    public void StopSloMoTransformer(float timeScale)
    {
        // Time.timeScale = m_OriginTimeScale;
        // Time.fixedDeltaTime /= timeScale;
        // m_IsTransformationSlowMotion = false;
    }

    public void StartSloMoFatality(float timeScale)
    {
        // StopSloMo();
        // m_OriginTimeScale = Time.timeScale;
        // Time.timeScale = timeScale;
        // Time.fixedDeltaTime *= timeScale;
        // m_FatalitySlowMotion = true;
    }

    public void StopSloMoFatality(float timeScale)
    {
        // Time.timeScale = m_OriginTimeScale;
        // Time.fixedDeltaTime /= timeScale;
        // m_FatalitySlowMotion = false;
    }
}