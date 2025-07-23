using System.Collections.Generic;
using UnityEngine;
using HyrphusQ.Events;
using LatteGames;
using Cinemachine;
using Sirenix.OdinInspector;

public class PvPWinnerCamera : MonoBehaviour
{
    const float Y_THRESHOLD = -10;
    [SerializeField] float smoothTime = 1f;
    [SerializeField] float fatalityCamTime = 1f;
    [SerializeField] float fatalityTimeScale = 1f;
    [SerializeField] Vector3 offset;
    [SerializeField] Vector3 lookOffset;
    [SerializeField] float fatalityDistance;
    [SerializeField] LayerMask viewObstacleMask;
    [SerializeField] CinemachineBlendDefinition fatalityCamBlend;
    [SerializeField] List<Vector3> fatalityCamPoses;// vector3 {camdistance, camHeight, camAngle}
    [SerializeField] CinemachineVirtualCamera winnerVCam;
    [SerializeField] PBLevelController levelController;
    CinemachineBrain cinemachineBrain;
    CinemachineBlendDefinition originalBlend;
    private PvPSlowMotion slowMotion;
    Transform target;
    Vector3 smoothPos;
    List<Competitor> winners;
    Competitor winner = null;
    Vector3 lastAttackerPos;
    Vector3 lastDmgReceiverPos;
    Vector3 fatalityCamPos;
    Vector3 fatalityForcusPos;
    int dmgEventFrame;
    int lastSurvivorFrame;
    bool isPLayFatality => lastSurvivorFrame != 0 && lastSurvivorFrame == dmgEventFrame;

    [SerializeField, BoxGroup("Data")] private PVPGameOverConfigSO m_PVPGameOverConfigSO;

    private void Awake()
    {
        lastSurvivorFrame = dmgEventFrame = 0;
        GameEventHandler.AddActionEvent(PBLevelEventCode.OnLevelEnded, HandleRoundCompleted);
        GameEventHandler.AddActionEvent(RobotStatusEventCode.OnRobotDamaged, HandleRobotTakeDamage);
    }

    private void Start()
    {
        cinemachineBrain = ObjectFindCache<CinemachineBrain>.Get();
        slowMotion = ObjectFindCache<PvPSlowMotion>.Get();
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(PBLevelEventCode.OnLevelEnded, HandleRoundCompleted);
        GameEventHandler.RemoveActionEvent(RobotStatusEventCode.OnRobotDamaged, HandleRobotTakeDamage);
    }

    void HandleRoundCompleted(object[] parameters)
    {
        if (parameters[0] is not List<Competitor> competitors) return;
        winners = competitors;

        if (competitors == null || competitors.Count <= 0) return;
        foreach (var competitor in competitors)
        {
            if (competitor.PersonalInfo.isLocal == true)
            {
                winner = competitor;
            }
        }
        if (winner == null)
        {
            winner = competitors[0];
        }
        if ( parameters[1] is int survivorCount && survivorCount == 1)
        {
            lastSurvivorFrame = Time.frameCount;
            InitFatalityCam();
        }
        SetTarget();
    }

    void HandleRobotTakeDamage(object[] parameters)
    {
        if (parameters[0] is not PBRobot pbRobot ||
            isPLayFatality) return;
        if (parameters[3] is PBPart pbPart && pbPart.RobotChassis != null)
        {
            lastAttackerPos = pbPart.RobotChassis.Robot.GetTargetPoint();
        }
        else if (parameters[3] is ActiveSkillCaster skillCaster && skillCaster.mRobot != null)
        {
            lastAttackerPos = skillCaster.mRobot.GetTargetPoint();
        }
        else if (parameters[3] is DroneBehavior drone && drone.owner != null)
        {
            lastAttackerPos = drone.owner.GetTargetPoint();
        }
        else
        {
            return;
        }
        lastDmgReceiverPos = pbRobot.GetTargetPoint();
        dmgEventFrame = Time.frameCount;
        InitFatalityCam();
    }

    void InitFatalityCam()
    {
        if (!isPLayFatality)
        {
            return;
        }
        fatalityForcusPos = 0.5f * (lastAttackerPos + lastDmgReceiverPos);
        fatalityCamPos = (lastAttackerPos - lastDmgReceiverPos).sqrMagnitude < fatalityDistance * fatalityDistance ? 
            FindBestFatalityCamPos(lastAttackerPos, lastDmgReceiverPos):
            FindBestFatalityCamPos(lastDmgReceiverPos, lastAttackerPos);
        cinemachineBrain.m_DefaultBlend = fatalityCamBlend;
        slowMotion?.StartSloMoFatality(fatalityTimeScale);

        Vector3 FindBestFatalityCamPos(Vector3 first, Vector3 second)
        {
            Vector3 bestCamPos;
            Vector3 posConfig;
            Vector3 angleZeroDir = first - second;
            angleZeroDir.y = 0;
            angleZeroDir = angleZeroDir.normalized;
            float camProjectionDistance;
            for (int i = 0, length = fatalityCamPoses.Count; i < length; i++)
            {
                posConfig = fatalityCamPoses[i];
                camProjectionDistance = Mathf.Sqrt(posConfig.x * posConfig.x - posConfig.y * posConfig.y);
                bestCamPos = first + Quaternion.Euler(0, - posConfig.z, 0) * angleZeroDir * camProjectionDistance + posConfig.y * Vector3.up;
                if (!Physics.SphereCast(bestCamPos, 0.3f, first - bestCamPos, out _, (first - bestCamPos).magnitude - 2, viewObstacleMask.value) &&
                    !Physics.SphereCast(bestCamPos, 0.3f, second - bestCamPos, out _, (second - bestCamPos).magnitude - 2, viewObstacleMask.value))
                {
                    return bestCamPos;
                }
            }
            for (int i = 0, length = fatalityCamPoses.Count; i < length; i++)
            {
                posConfig = fatalityCamPoses[i];
                camProjectionDistance = Mathf.Sqrt(posConfig.x * posConfig.x - posConfig.y * posConfig.y);
                bestCamPos = second - Quaternion.Euler(0, - posConfig.z, 0) * angleZeroDir * camProjectionDistance + posConfig.y * Vector3.up;
                if (!Physics.SphereCast(bestCamPos, 0.3f, first - bestCamPos, out _, (first - bestCamPos).magnitude - 2, viewObstacleMask.value) &&
                    !Physics.SphereCast(bestCamPos, 0.3f, second - bestCamPos, out _, (second - bestCamPos).magnitude - 2, viewObstacleMask.value))
                {
                    return bestCamPos;
                }
            }
            posConfig = fatalityCamPoses?[0]??new Vector3(20, 10, 25);
            camProjectionDistance = Mathf.Sqrt(posConfig.x * posConfig.x - posConfig.y * posConfig.y);
            bestCamPos = first + Quaternion.Euler(0, - posConfig.z, 0) * angleZeroDir * camProjectionDistance + posConfig.y * Vector3.up;
            return bestCamPos;
        }
    }

///**
    void OnDrawGizmos()
    {
        if (!isPLayFatality) return;
        Color original = Gizmos.color;

        Vector3 first = lastAttackerPos;
        Vector3 second = lastDmgReceiverPos;
        Vector3 bestCamPos;
        Vector3 posConfig;
        Vector3 angleZeroDir = first - second;
        angleZeroDir.y = 0;
        angleZeroDir = angleZeroDir.normalized;
        float camProjectionDistance;
        for (int i = 0, length = fatalityCamPoses.Count; i < length; i++)
        {
            posConfig = fatalityCamPoses[i];
            camProjectionDistance = Mathf.Sqrt(posConfig.x * posConfig.x - posConfig.y * posConfig.y);
            bestCamPos = first + Quaternion.Euler(0, - posConfig.z, 0) * angleZeroDir * camProjectionDistance + posConfig.y * Vector3.up;
            if (!Physics.SphereCast(bestCamPos, 0.3f, first - bestCamPos, out _, (first - bestCamPos).magnitude - 2, viewObstacleMask.value) &&
                !Physics.SphereCast(bestCamPos, 0.3f, second - bestCamPos, out _, (second - bestCamPos).magnitude - 2, viewObstacleMask.value))
            {
                Gizmos.color = Color.green;
            }
            else
            {
                Gizmos.color = Color.red;
            }
            Gizmos.DrawLine(bestCamPos, first);
            Gizmos.DrawLine(bestCamPos, second);
        }
        for (int i = 0, length = fatalityCamPoses.Count; i < length; i++)
        {
            posConfig = fatalityCamPoses[i];
            camProjectionDistance = Mathf.Sqrt(posConfig.x * posConfig.x - posConfig.y * posConfig.y);
            bestCamPos = second - Quaternion.Euler(0, - posConfig.z, 0) * angleZeroDir * camProjectionDistance + posConfig.y * Vector3.up;
            if (!Physics.SphereCast(bestCamPos, 0.3f, first - bestCamPos, out _, (first - bestCamPos).magnitude - 2, viewObstacleMask.value) &&
                !Physics.SphereCast(bestCamPos, 0.3f, second - bestCamPos, out _, (second - bestCamPos).magnitude - 2, viewObstacleMask.value))
            {
                Gizmos.color = Color.green;
            }
            else
            {
                Gizmos.color = Color.red;
            }
            Gizmos.DrawLine(bestCamPos, first);
            Gizmos.DrawLine(bestCamPos, second);
        }

        Gizmos.color = original;
    }
//**/

    void OnWinnerHealthChange(Competitor.HealthChangedEventData data)
    {
        if (winner.IsDead)
        {
            var aliveCompetitors = levelController.Competitors.FindAll((item) => !item.IsDead);
            if (aliveCompetitors.Count > 0)
            {
                winner = aliveCompetitors[0];
                SetTarget();
            }
        }
    }

    Coroutine setTargetCoroutine;
    void SetTarget()
    {
        if (winner != null)
        {
            winner.OnHealthChanged -= OnWinnerHealthChange;
        }
        winner.OnHealthChanged += OnWinnerHealthChange;
        var robot = winner as PBRobot;
        if (setTargetCoroutine != null)
        {
            StopCoroutine(setTargetCoroutine);
        }
        setTargetCoroutine = StartCoroutine(CommonCoroutine.Delay(fatalityCamTime, true, () => 
        {
            target = robot.ChassisInstanceTransform;
            if (isPLayFatality)
            {
                slowMotion?.StopSloMoFatality(fatalityTimeScale);
                cinemachineBrain.m_DefaultBlend = originalBlend;
            }

            if (cinemachineBrain != null)
            {
                cinemachineBrain.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.EaseInOut;
                cinemachineBrain.m_DefaultBlend.m_Time = m_PVPGameOverConfigSO.TimeMoveWinCamDuration;
            }
        }));
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            if (isPLayFatality)
            {
                winnerVCam.transform.rotation = Quaternion.LookRotation(fatalityForcusPos - fatalityCamPos);
                winnerVCam.transform.position = fatalityCamPos;
                winnerVCam.enabled = true;
            }
            else
            {
                winnerVCam.enabled = false;
            }
            return;
        }
        if (winnerVCam.transform.position.y < Y_THRESHOLD)
        {
            return;
        }
        Vector3 camPos = target.position + target.right * offset.x + Vector3.up * offset.y + target.forward * offset.z;
        winnerVCam.transform.position = Vector3.SmoothDamp(winnerVCam.transform.position, camPos, ref smoothPos, smoothTime);
        var lookRot = Quaternion.LookRotation(target.position + lookOffset - winnerVCam.transform.position);
        winnerVCam.transform.rotation = lookRot;
        winnerVCam.enabled = true;
    }
}
