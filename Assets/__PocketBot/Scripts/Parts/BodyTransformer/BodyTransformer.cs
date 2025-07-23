using System;
using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Helpers;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;
using FIMSpace.FProceduralAnimation;
using LatteGames;
using Sirenix.Utilities;
using UnityEngine.Animations;
using HyrphusQ.Events;
using LatteGames.Template;


#if UNITY_EDITOR
using UnityEditor;
#endif

[EventCode]
public enum BodyTransformerEventCode
{
    OnTransformationStarted,
    OnTransformationEnded
}
public class BodyTransformer : MonoBehaviour
{
    public enum State
    {
        Idle,
        Attack,
    }

    public class StateTracker : MonoBehaviour
    {
        [ShowInInspector, BoxGroup("Info"), ReadOnly]
        public bool isInitialized;
        [ShowInInspector, BoxGroup("Info"), ReadOnly]
        public bool isAbleToTransform { get; set; }
        [ShowInInspector, BoxGroup("Info"), ReadOnly]
        public State currentState { get; set; }
        [ShowInInspector, BoxGroup("Info"), ReadOnly]
        public int slowMotionTimes { get; set; }

        public void Initialize(BodyTransformerConfigSO configSO)
        {
            if (isInitialized)
                return;
            isInitialized = true;
            slowMotionTimes = configSO.maxSlowMotionTimes;
        }
    }

    [Serializable]
    public class BoneParentConstraint
    {
        [SerializeField]
        private bool m_IsFrontWheel;
        [SerializeField]
        private Transform m_Source;
        [SerializeField]
        private Transform[] m_Targets;

        private Vector3[] m_OriginLocalPositions;
        private Quaternion[] m_OriginLocalRotations;
        private Transform m_FrontWheelPivot;

        public void Init()
        {
            if (m_IsFrontWheel)
            {
                m_FrontWheelPivot = new GameObject("wheel_front.l_pivot").transform;
                m_FrontWheelPivot.SetParent(m_Targets[0].parent);
                m_FrontWheelPivot.localPosition = Vector3.zero;
                m_FrontWheelPivot.localRotation = Quaternion.identity;
                m_FrontWheelPivot.localScale = Vector3.one;
                m_Targets[0].SetParent(m_FrontWheelPivot);
            }
            m_OriginLocalPositions = new Vector3[m_Targets.Length];
            m_OriginLocalRotations = new Quaternion[m_Targets.Length];
            for (int i = 0; i < m_Targets.Length; i++)
            {
                m_OriginLocalPositions[i] = m_Targets[i].localPosition;
                m_OriginLocalRotations[i] = m_Targets[i].localRotation;
            }
        }

        public void UpdatePositionAndRotation()
        {
            for (int i = 0; i < m_Targets.Length; i++)
            {
                Vector3 sourceLocalPos = m_Source.GetChild(0).localPosition / 100f;
                m_Targets[i].localPosition = m_OriginLocalPositions[i] + (m_IsFrontWheel ? Vector3.down * sourceLocalPos.y : Vector3.left * sourceLocalPos.y);
                m_Targets[i].localRotation = m_OriginLocalRotations[i] * m_Source.GetChild(0).localRotation;
            }
            if (m_IsFrontWheel)
            {
                m_FrontWheelPivot.localRotation = Quaternion.Euler(-Vector3.up * m_Source.localEulerAngles.y);
            }
        }
    }

    private readonly static int s_IsAttackingId = Animator.StringToHash("IsAttacking");
    private readonly static int s_TransformationSpeedId = Animator.StringToHash("TransformationSpeed");

    [SerializeField]
    private BodyTransformerConfigSO m_ConfigSO;
    [SerializeField]
    private RadarDetector m_RadarDetector;
    [SerializeField]
    private Animator m_Animator;
    [SerializeField]
    private PBChassis m_Chassis;
    [SerializeField]
    private LegsAnimator m_LegsAnimator;
    [SerializeField]
    private BoxCollider m_BodyCollider, m_IdleCollider, m_AttackCollider;
    [SerializeField]
    private Collider[] m_DetachedPartColliders;
    [SerializeField]
    private BoneParentConstraint[] m_BoneConstraints = new BoneParentConstraint[] { };
    [SerializeField]
    private ParticleSystem[] m_IdleStateVFXs = new ParticleSystem[] { };
    [SerializeField]
    private ParticleSystem[] m_AttackStateVFXs = new ParticleSystem[] { };
    [SerializeField]
    private Transform[] m_IdleParentConstraintSources;
    [SerializeField]
    private Transform[] m_AttackParentConstraintSources;
    [SerializeField]
    private ParentConstraint[] m_ParentConstraints;


    [ShowInInspector, BoxGroup("Info"), ReadOnly]
    private bool m_IsTransformationInProgress;
    [ShowInInspector, BoxGroup("Info"), ReadOnly]
    private float m_IdleToAttackAccumulatedTime;
    [ShowInInspector, BoxGroup("Info"), ReadOnly]
    private float m_AttackToIdleAccumulatedTime;
    private PBRobot m_Robot;
    private StateTracker m_StateTracker;
    private Renderer[] m_Renderers;
    private PvPSlowMotion m_SlowMotion;

    public bool isForceSoundEnabled { get; set; }
    public PBRobot robot => m_Robot;
    public State currentState => m_StateTracker.currentState;

    private void Start()
    {
        isForceSoundEnabled = GetComponentInParent<TransformersManager>() != null;
        m_Robot = m_Chassis.Robot;
        m_Robot.OnHealthChanged += OnHealthChanged;
        if (!m_Robot.IsPreview)
            m_SlowMotion = ObjectFindCache<PvPSlowMotion>.Get();
        m_Animator.updateMode = m_ConfigSO.updateMode;
        m_RadarDetector.Initialize(m_Robot.AIBotController, m_ConfigSO.idleTransformationRange, 360f);
        m_RadarDetector.IsEnabled = !m_Robot.IsPreview;
        m_StateTracker = m_Robot.GetOrAddComponent<StateTracker>();
        m_StateTracker.Initialize(m_ConfigSO);
        m_Renderers = m_Chassis.GetComponentsInChildren<Renderer>();
        m_BoneConstraints.ForEach(item => item.Init());
        if (m_ConfigSO.defaultState == State.Idle)
            m_Robot.MovementSpeedMultiplier *= m_ConfigSO.attackStateSpeed;
        if (m_Robot.IsPreview)
            PreviewTransformationSequence();
        else
            Transform(m_ConfigSO.defaultState, AnimationDuration.ZERO);
        GameEventHandler.AddActionEvent(PBLevelEventCode.OnLevelStart, OnLevelStarted);
        GameEventHandler.AddActionEvent(PBLevelEventCode.OnLevelEnded, OnLevelEnded);
    }

    private void OnDestroy()
    {
        if (m_Robot != null)
            m_Robot.OnHealthChanged -= OnHealthChanged;
        GameEventHandler.RemoveActionEvent(PBLevelEventCode.OnLevelStart, OnLevelStarted);
        GameEventHandler.RemoveActionEvent(PBLevelEventCode.OnLevelEnded, OnLevelEnded);
    }

    private void Update()
    {
        HandleTransformation();
    }

    private void LateUpdate()
    {
        HandleWheels();
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        m_RadarDetector.InvokeMethod("OnDrawGizmos");
    }

    private void OnDrawGizmosSelected()
    {
        m_RadarDetector.InvokeMethod("OnDrawGizmosSelected");
    }

    [Button]
    private void FetchDetachedPartColliders()
    {
        m_DetachedPartColliders = m_Animator.transform.FindRecursive("root").GetComponentsInChildren<Collider>();
        EditorUtility.SetDirty(this);
    }

    [Button]
    private void SetDefaultTPose(State state)
    {
        Dictionary<Transform, TransformData> poseData = GetPoseDataDictionary(state == State.Attack ? m_ConfigSO.attackAnimClip : m_ConfigSO.idleAnimClip);
        foreach (var item in poseData)
        {
            Transform bone = item.Key;
            TransformData transformData = item.Value;
            bone.localPosition = transformData.position;
            bone.localRotation = transformData.rotation;
            bone.localScale = transformData.scale;
        }
        EditorUtility.SetDirty(this);
    }

    private Dictionary<Transform, TransformData> GetPoseDataDictionary(AnimationClip animationClip)
    {
        Dictionary<Transform, TransformData> boneToTransformDataDict = new Dictionary<Transform, TransformData>();
        QueryPoseDataRecursive(m_Animator.transform.FindRecursive("root"));
        return boneToTransformDataDict;

        void QueryPoseDataRecursive(Transform rootTransform)
        {
            if (rootTransform == null)
                return;
            foreach (Transform childTransform in rootTransform)
            {
                QueryPoseDataRecursive(childTransform);
            }
            boneToTransformDataDict.Set(rootTransform, GetTransformDataAtFrame(rootTransform.name, animationClip, 0));
        }
    }

    private TransformData GetTransformDataAtFrame(string boneName, AnimationClip animationClip, int frame)
    {
        if (m_Animator == null || animationClip == null || string.IsNullOrEmpty(boneName))
        {
            Debug.LogError("Animator, Animation Clip, or Bone Name is not set.");
            return default;
        }

        // Convert frame number to time
        float frameRate = animationClip.frameRate;
        float time = frame / frameRate;

        // Sample the animation at the specific time
        GameObject rootObject = m_Animator.gameObject;
        AnimationMode.StartAnimationMode();
        AnimationMode.SampleAnimationClip(rootObject, animationClip, time);

        // Find the bone's transform
        Transform boneTransform = FindBoneTransform(rootObject.transform, boneName);
        if (boneTransform == null)
        {
            Debug.LogError($"Bone '{boneName}' not found.");
            AnimationMode.StopAnimationMode();
            return default;
        }

        // Copy the transform
        Vector3 position = boneTransform.localPosition;
        Quaternion rotation = boneTransform.localRotation;
        Vector3 scale = boneTransform.localScale;
        TransformData transformData = new TransformData(boneTransform, Space.Self);

        Debug.Log($"Bone '{boneName}' at frame {frame}:\nPosition: {position}\nRotation: {rotation.eulerAngles}\nScale: {scale}");
        AnimationMode.StopAnimationMode();
        return transformData;
    }

    private Transform FindBoneTransform(Transform parent, string name)
    {
        if (parent.name == name)
            return parent;

        foreach (Transform child in parent)
        {
            Transform result = FindBoneTransform(child, name);
            if (result != null)
                return result;
        }
        return null;
    }
#endif

    private void OnLevelStarted()
    {
        m_StateTracker.isAbleToTransform = true;
    }

    private void OnLevelEnded()
    {
        m_StateTracker.isAbleToTransform = false;
    }

    private void HandleWheels()
    {
        if (m_Robot.IsDead || m_StateTracker.currentState == State.Attack)
            return;
        for (int i = 0; i < m_BoneConstraints.Length; i++)
        {
            m_BoneConstraints[i].UpdatePositionAndRotation();
        }
    }

    private void HandleTransformation()
    {
        if (m_Robot.IsDead || m_Chassis.CarPhysics.IsImmobilized || !m_ConfigSO.isAbleToTransform || !m_StateTracker.isAbleToTransform || m_IsTransformationInProgress || Time.timeScale <= 0f)
            return;
        var isAnyBotInAttackRange = m_RadarDetector.TryScanRobotInDetectArea(out PBRobot _);
        if (m_StateTracker.currentState == State.Idle)
        {
            if (isAnyBotInAttackRange)
            {
                m_IdleToAttackAccumulatedTime += Time.deltaTime;
                if (m_IdleToAttackAccumulatedTime >= m_ConfigSO.idleToAttackTimeThreshold)
                {
                    Transform(State.Attack, m_ConfigSO.idleToAttackTransformationDuration);
                }
            }
            else
            {
                m_IdleToAttackAccumulatedTime = 0f;
            }
        }
        else
        {
            if (!isAnyBotInAttackRange)
            {
                m_AttackToIdleAccumulatedTime += Time.deltaTime;
                if (m_AttackToIdleAccumulatedTime >= m_ConfigSO.attackToIdleTimeThreshold)
                {
                    Transform(State.Idle, m_ConfigSO.attackToIdleTransformationDuration);
                }
            }
            else
            {
                m_AttackToIdleAccumulatedTime = 0f;
            }
        }
    }

    private void OnHealthChanged(Competitor.HealthChangedEventData eventData)
    {
        if (eventData.MaxHealth > 0f && eventData.CurrentHealth <= 0f)
        {
            m_Animator.enabled = false;
            m_LegsAnimator.enabled = false;
            Destroy(m_Animator);
            Destroy(m_LegsAnimator);
            CarConfigSO carConfigSO = m_Chassis.RobotBaseBody.GetComponent<GravityPhysics>().CarConfigSO;
            foreach (var detachedPartCollider in m_DetachedPartColliders)
            {
                detachedPartCollider.enabled = true;
                var randomForceDir = new Vector3(Random.Range(-1f, 1f), 1f, Random.Range(-1f, 1f));
                var rb = detachedPartCollider.GetOrAddComponent<Rigidbody>();
                rb.mass = m_Robot.ChassisInstance.RobotBaseBody.mass;
                rb.transform.parent = null;
                rb.AddForce(randomForceDir * m_Robot.ExplodeForce * m_ConfigSO.explodeForceMultiplier, ForceMode.Impulse);
                var gravityPhysics = detachedPartCollider.GetOrAddComponent<GravityPhysics>();
                gravityPhysics.CarConfigSO = carConfigSO;
            }
        }
    }

    private void NotifyEventTransformationStarted(params object[] parameters)
    {
        m_IsTransformationInProgress = true;
        GameEventHandler.Invoke(BodyTransformerEventCode.OnTransformationStarted, parameters);
    }

    private void NotifyEventTransformationEnded(params object[] parameters)
    {
        m_IsTransformationInProgress = false;
        GameEventHandler.Invoke(BodyTransformerEventCode.OnTransformationEnded, parameters);
    }

    private void Transform(State state, float duration)
    {
        bool isAbleToSlowMotion = !m_Robot.IsPreview && m_Robot.PersonalInfo.isLocal && m_StateTracker.currentState == State.Idle && state == State.Attack && m_StateTracker.slowMotionTimes-- > 0;
        m_StateTracker.currentState = state;
        m_IdleToAttackAccumulatedTime = 0f;
        m_AttackToIdleAccumulatedTime = 0f;
        m_Robot.MovementSpeedMultiplier = state == State.Idle ? m_Robot.MovementSpeedMultiplier / m_ConfigSO.attackStateSpeed : m_Robot.MovementSpeedMultiplier * m_ConfigSO.attackStateSpeed;
        m_Animator.SetBool(s_IsAttackingId, state == State.Attack);
        foreach (var vfx in m_IdleStateVFXs)
        {
            if (state == State.Idle)
            {
                vfx.Play();
            }
            else
            {
                vfx.Stop();
            }
        }
        foreach (var vfx in m_AttackStateVFXs)
        {
            if (state == State.Idle)
            {
                vfx.Stop();
            }
            else
            {
                vfx.Play();
            }
        }
        for (int i = 0; i < m_ParentConstraints.Length; i++)
        {
            m_ParentConstraints[i].translationOffsets = new Vector3[1] { state == State.Idle ? m_IdleParentConstraintSources[i].localPosition : m_AttackParentConstraintSources[i].localPosition };
        }
        m_BodyCollider.size = state == State.Idle ? m_IdleCollider.size : m_AttackCollider.size;
        m_BodyCollider.center = state == State.Idle ? m_IdleCollider.center : m_AttackCollider.center;
        m_RadarDetector.MaxRadius = state == State.Idle ? m_ConfigSO.idleTransformationRange : m_ConfigSO.attackTransformationRange;

        if (Mathf.Approximately(duration, 0f))
        {
            NotifyEventTransformationStarted(this, duration);
            // Transform immediately
            m_Animator.Play(state == State.Attack ? "Anim_AttackState" : "Anim_IdleState", 0, 0f);
            m_LegsAnimator.enabled = state == State.Attack;
            m_Robot.EnabledAllParts(state == State.Attack);
            NotifyEventTransformationEnded(this, duration);
        }
        else
        {
            NotifyEventTransformationStarted(this, duration);
            // Transform in x duration
            if (isAbleToSlowMotion)
            {
                m_SlowMotion.StopSloMo();
                m_SlowMotion.StartSloMoTransformer(m_ConfigSO.timeScale);
            }

            m_Animator.SetFloat(s_TransformationSpeedId, (state == State.Attack ? m_ConfigSO.idleToAttackAnimClipLength : m_ConfigSO.attackToIdleAnimClipLength) / duration);
            // m_Robot.ChassisInstance.RobotBaseBody
            //     .DOJump(m_Robot.ChassisInstance.RobotBaseBody.position + m_Robot.ChassisInstance.RobotBaseBody.transform.forward * m_ConfigSO.jumpStrength, m_ConfigSO.jumpPower, m_ConfigSO.numJumps, m_ConfigSO.jumpDuration)
            //     .SetEase(m_ConfigSO.jumpEase)
            //     .SetUpdate(m_ConfigSO.jumpIndependentUpdate);

            if (state == State.Idle)
            {
                m_LegsAnimator.enabled = false;
                m_Robot.EnabledAllParts(state == State.Attack);
            }
            else
            {
                if (isAbleToSlowMotion)
                {
                    Rigidbody robotBaseBody = m_Robot.ChassisInstance.RobotBaseBody;
                    Vector3 velocity = robotBaseBody.transform.rotation * Quaternion.Euler(m_ConfigSO.jumpAngle, 0f, 0f) * Vector3.up;
                    robotBaseBody.velocity = Mathf.Min(robotBaseBody.velocity.magnitude * m_ConfigSO.jumpStrength, m_ConfigSO.maxJumpStrength) * velocity.normalized;
                }
                StartCoroutine(CommonCoroutine.Delay(duration, m_ConfigSO.updateMode == AnimatorUpdateMode.UnscaledTime, () =>
                {
                    if (isAbleToSlowMotion)
                    {
                        m_SlowMotion.StopSloMoTransformer(m_ConfigSO.timeScale);
                    }
                    if (!m_Robot.IsPreview && m_Robot.IsDead)
                        return;
                    m_LegsAnimator.enabled = true;
                    m_Robot.EnabledAllParts(state == State.Attack);
                }));
            }
            StartCoroutine(CommonCoroutine.Delay(duration, m_ConfigSO.updateMode == AnimatorUpdateMode.UnscaledTime, () => NotifyEventTransformationEnded(this, duration)));
            // Sound
            if (IsSoundEnabled())
            {
                if (state == State.Attack)
                    SoundManager.Instance.PlaySFX(SFX.TFM_TransformStartUp);
                StartCoroutine(CommonCoroutine.Delay(0.167f, false, () => SoundManager.Instance.PlaySFX(state == State.Idle ? SFX.TFM_TransformCar : SFX.TFM_TransformRobot)));
            }

            AstroblastLazerGunBehaviour astroblastLazerGunBehaviour = m_Robot.PartInstances.Find(v => v.PartSlotType == PBPartSlot.Front_1).Parts[0].gameObject.GetComponent<AstroblastLazerGunBehaviour>();
            if(astroblastLazerGunBehaviour != null)
                astroblastLazerGunBehaviour.HandleFakeVisualBooster();
        }
    }

    private IEnumerator PreviewTransformationSequence_CR()
    {
        RocketArtilleryBehaviour[] rocketGuns = GetComponentsInChildren<RocketArtilleryBehaviour>(true);
        AstroblastLazerGunBehaviour laserGun = GetComponentInChildren<AstroblastLazerGunBehaviour>(true);
        while (m_Robot.IsDisarmed)
        {
            m_Robot.IsDisarmed = false;
        }
        Transform(State.Idle, AnimationDuration.ZERO);
        while (true)
        {
            yield return Yielders.Get(1.5f);
            Transform(State.Attack, m_ConfigSO.idleToAttackTransformationDuration);
            yield return Yielders.Get(m_ConfigSO.idleToAttackTransformationDuration);
            for (int i = 0; i < m_ConfigSO.weaponPreviewCycleTimes; i++)
            {
                rocketGuns.ForEach(item => item.Fire(true));
                yield return Yielders.Get(rocketGuns[0].ConfigSO.FireRate * rocketGuns[0].MaxBulletSize);
                yield return laserGun.Fire_CR(true);
            }
            yield return Yielders.Get(1f);
            Transform(State.Idle, m_ConfigSO.attackToIdleTransformationDuration);
            yield return Yielders.Get(m_ConfigSO.attackToIdleTransformationDuration);
        }
    }

    private void PreviewTransformationSequence()
    {
        StartCoroutine(PreviewTransformationSequence_CR());
    }

    public bool IsSoundEnabled()
    {
        return PBSoundUtility.IsOnSound() || isForceSoundEnabled;
    }

    public Vector3 GetCenterOfMassOffset()
    {
        if (m_StateTracker == null)
            return Vector3.zero;
        return m_StateTracker.currentState == State.Idle ? m_ConfigSO.idleCenterOfMassOffset : m_ConfigSO.attackCenterOfMassOffset;
    }
}