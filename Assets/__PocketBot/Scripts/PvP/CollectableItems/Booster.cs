using DG.Tweening;
using HyrphusQ.Events;
using HyrphusQ.Helpers;
using LatteGames;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations;
using UnityEngine.UI;
using static PBRobot;

public enum BoosterGroup
{
    None,
    A,
    B,
    C,
    D
}
public enum BoosterProperty
{
    Default,
    RegeneratesOverTime,
    Physics,
    Link
}
public class Booster : CollectableItem, IPhysicsHandler
{
    public Action OnCollected;
    public Collider TriggerCollider => triggerCollider;
    public BoosterGroup BoosterGroup => m_BoosterGroup;
    public float RegeneratesBoosterGroup => m_RegeneratesBoosterGroup;

    [SerializeField, BoxGroup("Ref HP"), ShowIf("@boosterType == PvPBoosterType.Hp")] private GameObject m_SpanerPrefab;
    [SerializeField, BoxGroup("Ref HP"), ShowIf("@boosterType == PvPBoosterType.Hp")] private ParticleSystem m_HealingVFX;
    [SerializeField, BoxGroup("Ref HP"), ShowIf("@boosterType == PvPBoosterType.Hp")] private float m_TotalPercent = 5;
    [SerializeField, BoxGroup("Ref HP"), ShowIf("@boosterType == PvPBoosterType.Hp")] private float m_SecondsPerPercent = 0.5f;
    [SerializeField, BoxGroup("Ref HP"), ShowIf("@boosterType == PvPBoosterType.Hp")] private Transform m_VFXHolder;

    [SerializeField]
    private PvPBoosterType boosterType;
    [SerializeField]
    private float boosterSpawnEverySecs = 10f;
    [SerializeField]
    private BoosterConfigSO boosterConfigSO;
    [SerializeField]
    private ParticleSystem consumeBoosterParticleFX;
    [SerializeField]
    private Transform boosterModelHolder;
    [SerializeField]
    private GameObject boosterSpawnerModel;
    [SerializeField]
    private TextMeshProUGUI boosterDescText;
    [SerializeField]
    private TextMeshProUGUI boosterDescDecreaseText;
    [SerializeField]
    private bool m_IsHoldLight = false;

    [SerializeField, EnumToggleButtons, TitleGroup("Booster Settings")]
    private BoosterProperty m_BoosterProperty;

    #region BoosterGroup
    [ShowIfGroup("BoosterGroup", Condition = "@m_BoosterProperty == BoosterProperty.Link")]
    [BoxGroup("BoosterGroup/Settings")]
    [SerializeField] private BoosterGroup m_BoosterGroup;

    [ShowIfGroup("BoosterGroup")]
    [BoxGroup("BoosterGroup/Settings")]
    [SerializeField] private float m_RegeneratesBoosterGroup;
    #endregion

    #region Regenerates Over Time
    [ShowIfGroup("RegeneratesOverTime", Condition = "@m_BoosterProperty == BoosterProperty.RegeneratesOverTime")]
    [BoxGroup("RegeneratesOverTime/Settings")]
    [SerializeField] private bool m_IsRegeneration;

    [ShowIfGroup("RegeneratesOverTime")]
    [BoxGroup("RegeneratesOverTime/Settings")]
    [SerializeField] private Image m_TimerRegenerationImage;

    [ShowIfGroup("RegeneratesOverTime")]
    [BoxGroup("RegeneratesOverTime/Settings")]
    [SerializeField] private LookAtConstraint m_LookAtConstraint;

    [ShowIfGroup("RegeneratesOverTime"), ShowIf("@m_IsRegeneration")]
    [BoxGroup("RegeneratesOverTime/Settings")]
    [SerializeField] private float m_TimerRegeneration;
    #endregion

    #region Physics
    [ShowIfGroup("Physics", Condition = "@m_BoosterProperty == BoosterProperty.Physics")]
    [BoxGroup("Physics/Settings")]
    [SerializeField] private bool m_IsEnablePhysics;

    [ShowIfGroup("Physics")]
    [BoxGroup("Physics/Settings")]
    [SerializeField] private BoxCollider m_BoxPhysics;

    [ShowIfGroup("Physics")]
    [BoxGroup("Physics/Settings")]
    [SerializeField] private Rigidbody m_Rigidbody;

    [ShowIfGroup("Physics")]
    [BoxGroup("Physics/Settings")]
    [SerializeField] private LockRotationBooster m_LockRotationBooster;

    [ShowIfGroup("Physics")]
    [BoxGroup("Physics/Settings")]
    [SerializeField] private PulleyAnchor m_PullyAnchor;
    #endregion

    private bool isAbleToCollect = true;
    private float lastTimeCollectBooster;
    private Vector3 originBoosterModelLocalPos;
    private Camera mainCam;
    private ObstacleTrapDoor obstacleTrapDoor;

    private IEnumerator Start()
    {
        obstacleTrapDoor = GetComponentInParent<ObstacleTrapDoor>();

        if (BoosterManager.Instance != null)
            BoosterManager.Instance.ReceiveBoosterLink(this);

        if (m_Rigidbody != null && m_BoxPhysics != null && m_LockRotationBooster != null)
        {
            m_Rigidbody.useGravity = m_IsEnablePhysics;
            m_BoxPhysics.enabled = m_IsEnablePhysics;
            m_BoxPhysics.isTrigger = false;
            m_LockRotationBooster.IsLock = m_IsEnablePhysics;
        }

        GetCacheMainCameraHandle();

        if (!m_IsHoldLight)
            boosterSpawnerModel.gameObject.SetActive(m_IsRegeneration);

        m_TimerRegenerationImage.gameObject.SetActive(false);
        boosterDescText.gameObject.SetActive(false);
        boosterDescText.SetText(boosterConfigSO.GetBooster(boosterType).BoosterDesc);
        boosterDescDecreaseText.gameObject.SetActive(false);
        boosterDescDecreaseText.SetText(boosterConfigSO.GetBooster(boosterType).BoosterDescDecrease);
        originBoosterModelLocalPos = boosterModelHolder.transform.localPosition;
        var conditionToWait = new WaitUntil(() => !IsAbleToCollect() && (Time.time - lastTimeCollectBooster) > boosterSpawnEverySecs);
        while (true)
        {
            SpawnBooster();
            yield return conditionToWait;
        }
    }

    private void Update()
    {
        boosterModelHolder.transform.localPosition = Mathf.Sin(Time.time * Mathf.PI * boosterConfigSO.BoosterModelMovingSpeed) * boosterConfigSO.BoosterModelMovingRange + originBoosterModelLocalPos;
        boosterModelHolder.transform.Rotate(Vector3.up, Time.deltaTime * boosterConfigSO.BoosterModelRotationSpeed, Space.Self);
        if (boosterDescText.gameObject.activeSelf)
        {
            if (mainCam == null)
                GetCacheMainCameraHandle();
            if (mainCam != null)
                boosterDescText.transform.forward = (boosterDescText.transform.position - mainCam.transform.position).normalized;
        }
        if (boosterDescDecreaseText.gameObject.activeSelf)
        {
            boosterDescDecreaseText.transform.forward = (boosterDescDecreaseText.transform.position - mainCam.transform.position).normalized;
        }
    }
    private void GetCacheMainCameraHandle()
    {
        mainCam = MainCameraFindCache.Get();
        if (m_IsRegeneration && m_LookAtConstraint != null)
        {
            ConstraintSource constraintSource = new ConstraintSource();
            constraintSource.sourceTransform = mainCam.transform;
            constraintSource.weight = 1.0f;
            m_LookAtConstraint.AddSource(constraintSource);
            m_LookAtConstraint.constraintActive = true;
        }
    }
    private void ShowBoosterDesc(PBRobot robot)
    {
        HandleTextBooster(boosterDescText, robot);
    }

    private void ShowBoosterDescDecrease(PBRobot robot)
    {
        HandleTextBooster(boosterDescDecreaseText, robot);
    }

    private void HandleTextBooster(TextMeshProUGUI textMeshProUGUI, PBRobot robot)
    {
        textMeshProUGUI.gameObject.SetActive(true);
        var positionConstraint = textMeshProUGUI.GetOrAddComponent<PositionConstraint>();
        positionConstraint.AddSource(new ConstraintSource() { weight = 1f, sourceTransform = robot.ChassisInstance.RobotBaseBody.transform });
        positionConstraint.translationOffset = new Vector3(0f, 3f);
        positionConstraint.constraintActive = true;
        StartCoroutine(CommonCoroutine.Delay(AnimationDuration.SHORT, false, () =>
        {
            positionConstraint.RemoveSource(0);
            textMeshProUGUI.gameObject.SetActive(false);
        }));
    }

    public virtual PvPBoosterType GetBoosterType()
    {
        return boosterType;
    }

    public override bool IsAbleToCollect()
    {
        if (obstacleTrapDoor != null && !obstacleTrapDoor.IsOpen())
            return false;
        return isAbleToCollect;
    }

    public virtual void SpawnBooster()
    {
        if (IsAbleToCollect())
            return;
        isAbleToCollect = true;
        boosterSpawnerModel.SetActive(m_IsRegeneration);
        boosterModelHolder.gameObject.SetActive(true);
        boosterModelHolder.transform.SetLocalPositionAndRotation(originBoosterModelLocalPos, Quaternion.identity);
        //if (enterNavPointRobots.Count > 0)
        //    CollectItem(enterNavPointRobots.First.Value);
    }

    public virtual void EnableBooster()
    {
        triggerCollider.enabled = true;
        isAbleToCollect = true;
        boosterModelHolder.gameObject.SetActive(true);
        boosterSpawnerModel.gameObject.SetActive(m_IsRegeneration);
    }
    public virtual void DisableBooster()
    {
        RegeneratesItemCR(RegeneratesBoosterGroup);

        triggerCollider.enabled = false;
        isAbleToCollect = false;
        lastTimeCollectBooster = Time.time;
        boosterModelHolder.gameObject.SetActive(false);
    }

    public override void CollectItem(PBRobot robot)
    {
        if (robot == null)
            return;
        if (boosterType == PvPBoosterType.Hp && robot.Health >= robot.MaxHealth)
            return;
        if (robot.IsDead)
            return;

        if (m_BoosterGroup != BoosterGroup.None && BoosterManager.Instance != null)
            BoosterManager.Instance.CollectBoosterLink(m_BoosterGroup);

        isAbleToCollect = false;
        lastTimeCollectBooster = Time.time;
        if (!m_IsRegeneration)
            boosterSpawnerModel.SetActive(false);
        if (m_BoosterGroup != BoosterGroup.None)
            boosterSpawnerModel.SetActive(true);

        boosterModelHolder.gameObject.SetActive(false);
        ConsumeBooster();
        if (consumeBoosterParticleFX != null)
            consumeBoosterParticleFX.Play();
        ShowBoosterDesc(robot);
        NotifyEventBoosterBeingCollected(robot);

        List<PBPart> RelevantParts()
        {
            var relevantParts = robot.PartInstances
                .Where(part => part.PartSlotType != PBPartSlot.Body &&
                            part.PartSlotType != PBPartSlot.PrebuiltBody &&
                            part.PartSlotType != PBPartSlot.Wheels_1 &&
                            part.PartSlotType != PBPartSlot.Wheels_2 &&
                            part.PartSlotType != PBPartSlot.Wheels_3)
                .SelectMany(part => part.Parts)
                .Where(part => part != null);

            return relevantParts.Where(part => part != null).ToList();
        }
        void BoostSpeedupFire(float boosterPercent)
        {
            foreach (var part in RelevantParts())
            {
                var boostFireRates = part.GetComponentsInChildren<IBoostFireRate>();
                foreach (var boostFireRate in boostFireRates)
                    boostFireRate?.BoostSpeedUpFire(boosterPercent);
            }
        }
        void BoosterSpeedUpEnd(float boosterPercent)
        {
            foreach (var part in RelevantParts())
            {
                var boostFireRates = part.GetComponentsInChildren<IBoostFireRate>();
                foreach (var boostFireRate in boostFireRates)
                    boostFireRate?.BoostSpeedUpStop(boosterPercent);
            }
            ShowBoosterDescDecrease(robot);
        }

        void ConsumeBooster()
        {
            BoosterConfigSO.Booster booster = boosterConfigSO.GetBooster(boosterType);
            switch (boosterType)
            {
                case PvPBoosterType.Hp:
                    robot.StartHealing(m_HealingVFX, m_SpanerPrefab, booster.BoostPercent, m_TotalPercent / 100, m_SecondsPerPercent);
                    OnCollected?.Invoke();
                    break;
                case PvPBoosterType.Atk:
                    robot.AtkMultiplier *= 1f + booster.BoostPercent;
                    OnCollected?.Invoke();
                    break;
                case PvPBoosterType.Speed:
                    PBBoosterVFXManager.Instance.PlaySpeedBooster(robot.ChassisInstance);
                    robot.MovementSpeedMultiplier *= 1f + booster.BoostPercent;
                    OnCollected?.Invoke();
                    break;
                case PvPBoosterType.AttackSpeed:
                    BoostSpeedupFire(booster.BoostPercent);
                    OnCollected?.Invoke();

                    #region Design Event
                    try
                    {
                        if (PBFightingStage.Instance != null)
                        {
                            if (robot?.PersonalInfo?.isLocal == true)
                            {
                                string statgeID = ($"{PBFightingStage.Instance.name}").Replace(" Variant(Clone)", "");
                                string objectType = $"Powerup";
                                GameEventHandler.Invoke(DesignEvent.StageInteraction, statgeID, objectType);
                                Debug.Log($"Key - statgeID: {statgeID} | objectType: {objectType}");

                            }
                        }
                    }
                    catch { }
                    #endregion

                    break;
                case PvPBoosterType.Giant:
                    {
                        OnCollected?.Invoke();
                        robot.SizeMultiplier *= 1f + booster.BoostPercent;
                        ParentConstraint[] constraints = robot.ChassisInstance.GetComponentsInChildren<ParentConstraint>();
                        robot.ChassisInstanceTransform.localScale *= 1f + booster.BoostPercent;
                        for (int i = 0; i < constraints.Length; i++)
                        {
                            constraints[i].transform.localScale *= 1f + booster.BoostPercent;
                            constraints[i].SetTranslationOffset(0, constraints[i].GetTranslationOffset(0) * (1f + booster.BoostPercent));
                        }
                        for (int i = 0; i < robot.PartInstances.Count; i++)
                        {
                            PBPartType partType = robot.PartInstances[i].PartSlotType.GetPartTypeOfPartSlot();
                            if (partType == PBPartType.Body || partType == PBPartType.Wheels)
                                continue;
                            Joint[] joints = robot.PartInstances[i].Parts[0].GetComponentsInChildren<Joint>();
                            for (int j = 0; j < joints.Length; j++)
                            {
                                Joint joint = joints[j];
                                Vector3 anchor = joint.anchor;
                                Vector3 connectedAnchor = joint.connectedAnchor;
                                joint.anchor = anchor;
                                joint.connectedAnchor = connectedAnchor;
                                if (j == 0)
                                    joint.transform.localScale *= 1f + booster.BoostPercent;
                            }
                        }
                        break;
                    }
            }
            StartCoroutine(CommonCoroutine.Delay(booster.Duration, false, () =>
            {
                switch (boosterType)
                {
                    case PvPBoosterType.Hp:
                        break;
                    case PvPBoosterType.Atk:
                        robot.AtkMultiplier /= 1f + booster.BoostPercent;
                        break;
                    case PvPBoosterType.Speed:
                        robot.MovementSpeedMultiplier /= 1f + booster.BoostPercent;
                        break;
                    case PvPBoosterType.AttackSpeed:
                        BoosterSpeedUpEnd(booster.BoostPercent);
                        break;
                }
            }));
            robot.ConsumeBooster(boosterType);
        }

        if (m_IsRegeneration)
        {
            RegeneratesItemCR(m_TimerRegeneration, () =>
            {
                isAbleToCollect = false;
                SpawnBooster();
            });
        }
    }

    private void RegeneratesItemCR(float timeDuration, Action OnCompleteCallBack = null)
    {
        m_TimerRegenerationImage.gameObject.SetActive(true);
        TweenCallback<float> tweenCallback = (value) =>
        {
            m_TimerRegenerationImage.gameObject.SetActive(value > 0);
            triggerCollider.enabled = value <= 0;
            m_TimerRegenerationImage.fillAmount = value;
            if (value <= 0)
            {
                OnCompleteCallBack?.Invoke();
                m_TimerRegenerationImage.gameObject.SetActive(false);
            }
        };
        DOVirtual.Float(1, 0, timeDuration, tweenCallback).SetEase(Ease.Linear);
    }

    public void EnablePhysics()
    {
        if (m_Rigidbody != null && m_BoxPhysics != null && m_LockRotationBooster != null)
        {
            StartCoroutine(CommonCoroutine.Delay(0.3f, false, () =>
            {
                m_Rigidbody.useGravity = true;
                m_BoxPhysics.enabled = true;
                m_BoxPhysics.isTrigger = false;
                m_LockRotationBooster.IsLock = true;
            }));
        }
    }

    public void DisablePhysics()
    {
        if (m_Rigidbody != null && m_BoxPhysics != null && m_LockRotationBooster != null)
        {
            m_Rigidbody.useGravity = false;
            m_BoxPhysics.enabled = false;
            m_BoxPhysics.isTrigger = true;
            m_LockRotationBooster.IsLock = false;
        }
    }

    public bool IsOnPulley()
    {
        return m_PullyAnchor != null && !m_PullyAnchor.HasBroken;
    }

    public bool IsOnBreakableObject(out Vector3 point)
    {
        point = transform.position;
        RaycastHit hit = default;
        bool isHitBreakableObject = Physics.Raycast(transform.position, Vector3.down, out hit, float.MaxValue, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore) && hit.collider.GetComponent<IBreakObstacle>() != null;
        bool isHitGround = Physics.Raycast(transform.position, Vector3.down, out hit, float.MaxValue, LayerMask.GetMask("Ground"), QueryTriggerInteraction.Ignore);
        if (isHitBreakableObject && isHitGround)
        {
            point = hit.point;
        }
        return isHitBreakableObject;
    }

    public override Vector3 GetTargetPoint()
    {
        Vector3 point = transform.position;
        Vector3 alternativePoint;
        if (IsOnPulley())
            point = m_PullyAnchor.transform.position;
        else if (IsOnBreakableObject(out alternativePoint))
            point = alternativePoint;
        return point;
    }
}