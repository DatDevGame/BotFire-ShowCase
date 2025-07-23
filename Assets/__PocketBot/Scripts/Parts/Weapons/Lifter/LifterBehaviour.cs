using System.Collections;
using UnityEngine;
using Sirenix.OdinInspector;
using HyrphusQ.Events;
using System.Linq;
using System.Collections.Generic;
using static Lofelt.NiceVibrations.MMProgressBar;
using System.Net.NetworkInformation;

[RequireComponent(typeof(Rigidbody))]
public class LifterBehaviour : PartBehaviour, IBoostFireRate
{
    private static int GroundLayerIndex = -1;

    [SerializeField] float minForcePower = 5f;
    [SerializeField] float torqueDownSpeed;
    [SerializeField] float torqueUpSpeed;
    [SerializeField] private float _forceWeak;
    [SerializeField] private float _forceStrong;
    [SerializeField] private float _holdUpDuration;
    [SerializeField] private float _delayTimeForceUp;
    [PropertyRange(0, 100), SerializeField, LabelText("Rate Force Strong")] private float _percentForceStrong;
    [SerializeField] private Collider[] preventWallColliders;


    int lifterValue = 1;
    float torqueValue;

    float TorquePower => torqueValue * 100f * rb.mass;
    Vector3 TorqueAxis => GetRigidbodyAxes();
    bool IsLifting => lifterValue > 0;

    private bool _isForceUp = false;
    private bool _isTorqueUp = false;
    private float _timeTorqueDown;
    private float _timeTorqueUp = 0.3f;
    private IEnumerator _delayTorqueUpCR;
    private IEnumerator _delayTorqueDownCR;
    private ConfigurableJoint lifterJoint;
    private Dictionary<PBRobot, int> robotInsideLifterDict = new();


    private bool m_IsSpeedUp = false;
    private int m_StackSpeedUp;
    private float m_ObjectTimeScale;
    private float m_TimeScaleOrginal => TimeManager.Instance.originalScaleTime;
    private float m_BoosterPercent;
    [SerializeField, BoxGroup("Visual")] private List<MeshRenderer> m_MeshRendererBooster;
    void Awake()
    {
        m_ObjectTimeScale = m_TimeScaleOrginal;

        if (GroundLayerIndex == -1)
            GroundLayerIndex = LayerMask.NameToLayer("Ground");
        lifterJoint = rb.GetComponent<ConfigurableJoint>();
    }

    protected override IEnumerator Start()
    {
        yield return InitIgnoreCollision();
        GameEventHandler.AddActionEvent(RobotStatusEventCode.OnModelSpawned, OnModelSpawned);
        yield return base.Start();
        torqueValue = torqueDownSpeed;
    }

    void FixedUpdate()
    {
        rb.AddTorque(lifterValue * Time.deltaTime * TorquePower * TorqueAxis, ForceMode.Impulse);
    }

    void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(RobotStatusEventCode.OnModelSpawned, OnModelSpawned);
    }

    private IEnumerator InitIgnoreCollision()
    {
        if (PBFightingStage.Instance == null)
            yield break;
        yield return new WaitUntil(() => PBFightingStage.Instance.isFightingReady);
        var robots = PBRobot.allFightingRobots;
        foreach (var robot in robots)
        {
            if (robot == pbPart.RobotChassis.Robot)
                continue;
            if (robot.ChassisInstance == null) 
                continue;

            var colliders = robot.ChassisInstance.GetComponentsInChildren<Collider>().Where(collider => !collider.isTrigger);
            foreach (var collider in colliders)
            {
                foreach (var wallCollider in preventWallColliders)
                {
                    Physics.IgnoreCollision(wallCollider, collider);
                }
            }
        }
        var floorColliders = PBFightingStage.Instance.GetComponentsInChildren<Collider>().Where(collider => !collider.isTrigger && collider.gameObject.layer == GroundLayerIndex);
        if (floorColliders != null)
        {
            foreach (var collider in floorColliders)
            {
                foreach (var wallCollider in preventWallColliders)
                {
                    Physics.IgnoreCollision(wallCollider, collider);
                }
            }
        }
    }

    private void OnModelSpawned(object[] parameters)
    {
        if (parameters == null || parameters.Length <= 0)
            return;
        if (parameters[0] is not PBRobot robot || robot == pbPart.RobotChassis.Robot)
            return;
        InitIgnoreCollision();
    }

    void ChangeMomentumDirection()
    {
        lifterValue = -lifterValue;
        if (IsLifting) torqueValue = torqueUpSpeed;
        else torqueValue = torqueDownSpeed;
    }

    public override bool IsAbleToDealDamage(Collision collision, out QueryResult queryResult)
    {
        queryResult = QueryResult.Default;
        return false;
    }

    private void SetTimeScale(float timeScale)
    {
        m_ObjectTimeScale = timeScale;
    }

    private IEnumerator DelayTorqueDown()
    {
        _timeTorqueDown = _holdUpDuration;
        WaitForSeconds nextBehavior = new WaitForSeconds(attackCycleTime);

        while (true)
        {
            _timeTorqueDown -= Time.deltaTime * m_ObjectTimeScale; // Apply local time scale here

            if (_timeTorqueDown <= 0)
            {
                ChangeMomentumDirection();

                yield return nextBehavior;
                _isTorqueUp = false;
                break;
            }

            yield return null;
        }
    }

    private IEnumerator DelayTorqueUp(PBPart pbPart)
    {
        _isForceUp = true;
        yield return new WaitForSeconds(_timeTorqueUp * m_ObjectTimeScale); // Apply local time scale here

        if (!_isForceUp)
        {
            _isTorqueUp = _isForceUp;
        }
        else
        {
            ChangeMomentumDirection();
            pbPart.ReceiveDamage(this.pbPart, Const.FloatValue.ZeroF);

            int randomNumber = Random.Range(1, 11);
            if (pbPart.RobotChassis == null || pbPart.RobotChassis.Robot == null)
                yield break;

            if (!pbPart.RobotChassis.Robot.IsDead)
            {
                if (randomNumber <= _percentForceStrong / 10 && _isForceUp)
                {
                    if (pbPart.RobotBaseBody != null)
                        pbPart.RobotBaseBody.AddForceAtPosition(Vector3.up * _forceStrong, transform.position, ForceMode.VelocityChange);
                }
                else if (randomNumber > _percentForceStrong / 10 && _isForceUp)
                {
                    if (pbPart.RobotBaseBody != null)
                        pbPart.RobotBaseBody.AddForceAtPosition(Vector3.up * _forceWeak, transform.position, ForceMode.VelocityChange);
                }
            }

            if (_delayTorqueDownCR != null)
                StopCoroutine(_delayTorqueDownCR);
            _delayTorqueDownCR = DelayTorqueDown();
            StartCoroutine(_delayTorqueDownCR);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.attachedRigidbody != null && other.attachedRigidbody.TryGetComponent(out PBPart part) && part.RobotChassis != null)
        {
            robotInsideLifterDict.Set(part.RobotChassis.Robot, robotInsideLifterDict.Get(part.RobotChassis.Robot) + 1);
        }
        foreach (var item in robotInsideLifterDict)
        {
            if (item.Value > 0)
            {
                rb.mass = 50f;
                lifterJoint.connectedMassScale = 0.1f;
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!_isTorqueUp)
        {
            PBPart pbPart = other.GetComponent<PBPart>();
            if (pbPart != null)
            {
                _isTorqueUp = true;
                if (_delayTorqueUpCR != null)
                    StopCoroutine(_delayTorqueUpCR);
                _delayTorqueUpCR = DelayTorqueUp(pbPart);
                StartCoroutine(_delayTorqueUpCR);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        CarPhysics carPhysics = other.GetComponent<CarPhysics>();
        if (carPhysics != null)
            _isForceUp = false;

        if (other.attachedRigidbody != null && other.attachedRigidbody.TryGetComponent(out PBPart part) && part.RobotChassis != null)
        {
            robotInsideLifterDict.Set(part.RobotChassis.Robot, Mathf.Max(robotInsideLifterDict.Get(part.RobotChassis.Robot) - 1, 0));
        }
        foreach (var item in robotInsideLifterDict)
        {
            if (item.Value > 0)
            {
                return;
            }
        }
        rb.mass = 2.5f;
        lifterJoint.connectedMassScale = 1f;
    }

    public void BoostSpeedUpFire(float boosterPercent)
    {
        m_BoosterPercent += boosterPercent;

        //Enable VFX
        if (m_MeshRendererBooster != null && m_MeshRendererBooster.Count > 0)
            PBBoosterVFXManager.Instance.PlayBoosterSpeedUpAttackWeapon(pbPart, m_MeshRendererBooster);

        m_StackSpeedUp++;
        m_IsSpeedUp = true;
        m_ObjectTimeScale += (m_TimeScaleOrginal * boosterPercent);
        SetTimeScale(m_ObjectTimeScale);
    }
    public void BoostSpeedUpStop(float boosterPercent)
    {
        m_StackSpeedUp--;
        if (m_StackSpeedUp <= 0)
        {
            //Disable VFX
            PBBoosterVFXManager.Instance.StopBoosterSpeedUpAttackWeapon(pbPart);

            m_IsSpeedUp = false;
            m_ObjectTimeScale = m_TimeScaleOrginal;
            m_BoosterPercent = 0;
        }
        else
        {
            m_BoosterPercent -= boosterPercent;
            m_ObjectTimeScale -= (m_TimeScaleOrginal * boosterPercent);
        }
        SetTimeScale(m_ObjectTimeScale);
    }
    public bool IsSpeedUp() => m_IsSpeedUp;
    public int GetStackSpeedUp() => m_StackSpeedUp;

    public float GetPercentSpeedUp() => m_BoosterPercent;
}