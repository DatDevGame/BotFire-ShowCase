using HyrphusQ.Events;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;

using UnityEngine;

[CreateAssetMenu(fileName = "ActiveSkillSO_CallDrone", menuName = "PocketBots/ActiveSkillSO/CallDrone")]
public class ActiveSkillCallDroneSO : ActiveSkillSO<ActiveSkillCallDroneSO, ActiveSkillCallDroneCaster>
{
    [SerializeField] private float _droneDuration;
    [SerializeField] private float _droneSpeed;
    [SerializeField] private float _droneFlyHeight;
    [SerializeField] private int _droneAmount;
    [SerializeField] private float _operatingRangeMin;
    [SerializeField] private float _operatingRangeMax;
    [SerializeField] private float _appearRange;
    [SerializeField] private float _attackRange;
    [SerializeField, Range(0, 1)] private float _droneHpPercentDmg;
    [SerializeField] private float _droneDmgPerHitRatio;
    [SerializeField] private DroneBehavior _droneTemplate;
    [SerializeField] private Material _playerMat;
    [SerializeField] private Material _enemyMat;


    public float droneDuration => _droneDuration;
    public float droneSpeed => _droneSpeed;
    public float droneFlyHeight => _droneFlyHeight;
    public int droneAmount => _droneAmount;
    public float operatingRangeMin => _operatingRangeMin;
    public float operatingRangeMax => _operatingRangeMax;
    public float appearRange => _appearRange;
    public float attackRange => _attackRange;
    public float droneHpPercentDmg => _droneHpPercentDmg;
    public float droneDmgPerHitRatio => _droneDmgPerHitRatio;
    public DroneBehavior droneTemplate => _droneTemplate;
    public Material playerMat => _playerMat;
    public Material enemyMat => _enemyMat;

    public override float activeDuration => _droneDuration;

}

public class ActiveSkillCallDroneCaster : ActiveSkillCaster<ActiveSkillCallDroneSO>
{
    private enum State
    {
        Following,
        Attacking,
    }

    private const float APPPEAR_HEIGHT = 20f;
    private static Dictionary<Type, List<DroneBehavior>> _dronePool = new();

    private float _droneRemainDuration;
    private State _state;
    private PBRobot _closestRobot;
    private PBRobot _target;
    private RadarDetector _radarDetector;
    private List<DroneBehavior> _drones = new();

    #region Firebase Event
    [ShowInInspector] protected HashSet<int> m_TargetID;
    #endregion 

    public override float remainingActiveTime => _droneRemainDuration;

    private static DroneBehavior GetDrone(DroneBehavior template)
    {
        DroneBehavior result;
        Type type = template.GetType();
        if (_dronePool.TryGetValue(type, out var _drones))
        {
            while (_drones.Count > 0)
            {
                result = _drones[_drones.Count - 1];
                _drones.RemoveAt(_drones.Count - 1);
                if (result)
                {
                    return result;
                }
            }
        }
        return Instantiate(template);
    }

    private static void ReturnDrone(DroneBehavior drone)
    {
        Type type = drone.GetType();
        if (!_dronePool.TryGetValue(type, out var _drones))
        {
            _drones = new();
            _dronePool[type] = _drones;
        }
        drone.gameObject.SetActive(false);
        _drones.Add(drone);
    }

    public override void Initialize(ActiveSkillCallDroneSO activeSkillSO, PBRobot robot)
    {
        m_TargetID = new HashSet<int>();

        base.Initialize(activeSkillSO, robot);
        _radarDetector = new RadarDetector();
        _radarDetector.Initialize(m_Robot.AIBotController, m_ActiveSkillSO.operatingRangeMin, 360f);
    }

    protected override void Update()
    {
        base.Update();
        if (_radarDetector != null && (skillState == SkillState.Active || (skillState == SkillState.Ready && !m_Robot.PersonalInfo.isLocal)))
        {
            _radarDetector.TryScanRobotInDetectArea(out _closestRobot);
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.D) && !Input.GetKey(KeyCode.LeftShift) && m_Robot.PersonalInfo.isLocal)
            {
                PerformSkill();
            }
            else if (Input.GetKeyDown(KeyCode.D) && Input.GetKey(KeyCode.LeftShift) && !m_Robot.PersonalInfo.isLocal)
            {
                PerformSkill();
            }
#endif
        }

        if (_drones.Count > 0)
        {
            _droneRemainDuration -= Time.deltaTime;
            if (_droneRemainDuration <= 0 || m_Robot.IsDead)
            {
                ClearDrones();
                _target = null;
                remainingCooldown = m_ActiveSkillSO.cooldown;
            }
            else
            {
                switch (_state)
                {
                    case State.Following:
                        if (_closestRobot != null &&
                            (_closestRobot.ChassisInstanceTransform.position - m_Robot.ChassisInstanceTransform.position).magnitude < m_ActiveSkillSO.operatingRangeMin)
                        {
                            ToAttackingState(_closestRobot);
                        }
                        break;
                    case State.Attacking:
                        if (!_target || _target.IsDead ||
                            (_target.ChassisInstanceTransform.position - m_Robot.ChassisInstanceTransform.position).magnitude > m_ActiveSkillSO.operatingRangeMax)
                        {
                            if (_closestRobot != null &&
                            (_closestRobot.ChassisInstanceTransform.position - m_Robot.ChassisInstanceTransform.position).magnitude < m_ActiveSkillSO.operatingRangeMin)
                            {
                                ToAttackingState(_closestRobot);
                            }
                            else
                            {
                                ToFollowingState();
                            }
                        }
                        break;
                }
            }
        }
    }

    public override void PerformSkill()
    {
        base.PerformSkill();
        ClearDrones();
        _droneRemainDuration = m_ActiveSkillSO.droneDuration;
        Vector3 circleOffset = -Vector3.right * m_ActiveSkillSO.appearRange;
        Quaternion angle = Quaternion.Euler(0, -360f / m_ActiveSkillSO.droneAmount, 0);
        Material material = m_Robot.PersonalInfo.isLocal ? m_ActiveSkillSO.playerMat : m_ActiveSkillSO.enemyMat;
        for (int i = 0; i < m_ActiveSkillSO.droneAmount; i++)
        {
            DroneBehavior newDrone = GetDrone(m_ActiveSkillSO.droneTemplate);
            newDrone.gameObject.transform.position = m_Robot.ChassisInstanceTransform.position + (APPPEAR_HEIGHT + m_ActiveSkillSO.droneFlyHeight) * Vector3.up + circleOffset;
            newDrone.gameObject.SetActive(true);
            newDrone.Init(m_Robot, material,
                m_ActiveSkillSO.droneHpPercentDmg, m_ActiveSkillSO.droneDmgPerHitRatio,
                m_ActiveSkillSO.droneSpeed, m_ActiveSkillSO.attackRange, m_ActiveSkillSO.droneFlyHeight);
            newDrone.Appear((APPPEAR_HEIGHT + m_ActiveSkillSO.droneFlyHeight) * Vector3.up + circleOffset, m_ActiveSkillSO.droneFlyHeight * Vector3.up + circleOffset);
            _drones.Add(newDrone);
            circleOffset = angle * circleOffset;

            #region Firebase Event
            newDrone.InitTargetID(m_TargetID, m_ActiveSkillSO);
            #endregion
        }
        ToFollowingState();

        //GameEventHandler.AddActionEvent(RobotStatusEventCode.OnRobotDamaged, OnRobotDamaged);
    }

    private void ToAttackingState(PBRobot target)
    {
        _target = target;
        _state = State.Attacking;
        foreach (var drone in _drones)
        {
            drone.AttackRobot(_target);
        }
    }

    private void ToFollowingState()
    {
        _target = null;
        _state = State.Following;
        foreach (var drone in _drones)
        {
            drone.FollowOwner();
        }
    }

    private void ClearDrones()
    {
        #region Firebase Event
        try
        {
            if(m_TargetID != null)
                m_TargetID.Clear();
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            Debug.LogError($"{LogErrorKeyGA.Key}-{ex}");
        }
        #endregion

        foreach (var drone in _drones)
        {
            Vector3 offsetFromOwner = drone.transform.position - m_Robot.ChassisInstanceTransform.position;
            drone.Disappear(offsetFromOwner, offsetFromOwner + APPPEAR_HEIGHT * Vector3.up, DroneDisappearCB);
        }
        _drones.Clear();
        //GameEventHandler.RemoveActionEvent(RobotStatusEventCode.OnRobotDamaged, OnRobotDamaged);
    }

    private void DroneDisappearCB(DroneBehavior drone)
    {
        ReturnDrone(drone);
    }

    private void OnRobotDamaged(params object[] objects)
    {
        if (objects.Length >= 4 && objects[3] is PBPart part && part.RobotChassis && part.RobotChassis.Robot &&
            part.RobotChassis.Robot.GetHashCode() == m_Robot.GetHashCode() && objects[0] is PBRobot target &&
            target.GetHashCode() != m_Robot.GetHashCode() && target.GetHashCode() != _target.GetHashCode() && !target.IsDead &&
            (target.ChassisInstanceTransform.position - m_Robot.ChassisInstanceTransform.position).magnitude <= m_ActiveSkillSO.operatingRangeMin)
        {
            ToAttackingState(target);
        }
    }

    public override bool IsAbleToPerformSkillForAI()
    {
        return IsAbleToPerformSkill() && _closestRobot && 
        (_closestRobot.ChassisInstanceTransform.position - m_Robot.ChassisInstanceTransform.position).magnitude * 0.9f <= m_ActiveSkillSO.operatingRangeMin;
    }
}