using System;
using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Helpers;
using Sirenix.OdinInspector;
using UnityEngine;
using DG.Tweening;
using LatteGames.Template;
using Unity.AI.Navigation;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class LaserFence : MonoBehaviour, IAttackable
{
    private const float RayPointOffset = 0.2613f;
    private const float NavMeshObstacleWidthOffset = 4f;

    [SerializeField, BoxGroup("No Tweaks")]
    private float explosionHitOffset = 0.5f;
    [SerializeField, BoxGroup("No Tweaks")]
    private int segmentLength = 10;
    [SerializeField, BoxGroup("No Tweaks")]
    private float frequency = 2.5f;
    [SerializeField, BoxGroup("No Tweaks")]
    private float amplitude = 0.1f;
    [SerializeField, BoxGroup("Tweaks")]
    private float width = 2.258f;
    [SerializeField, BoxGroup("Tweaks")]
    private float damage = 5f;
    [SerializeField, BoxGroup("Tweaks")]
    private float causeDamageEverySecs = 0.2f;
    [SerializeField, BoxGroup("Tweaks")]
    private bool alwaysEnableLaser = true;
    [SerializeField, BoxGroup("Tweaks"), HideIf("alwaysEnableLaser")]
    private float disableLaserDuration = 1f;
    [SerializeField, BoxGroup("Tweaks"), HideIf("alwaysEnableLaser")]
    private float changeInDuration = 2f;
    [SerializeField, BoxGroup("Tweaks"), HideIf("alwaysEnableLaser")]
    private float enableLaserDuration = 2f;
    [SerializeField, PropertyOrder(-1), BoxGroup("No Tweaks")]
    private RangeFloatValue laserHeightRange;
    [SerializeField, BoxGroup("No Tweaks")]
    private GameObject leftPole, rightPole;
    [SerializeField, BoxGroup("No Tweaks")]
    private GameObject laserModelPrefab;
    [SerializeField, BoxGroup("No Tweaks")]
    private Transform modelTransform;
    [SerializeField, BoxGroup("No Tweaks")]
    private ParticleSystem leftLaserExplosionHit, rightLaserExplosionHit;
    [SerializeField, BoxGroup("No Tweaks")]
    private NavMeshModifierVolume navMeshModifierVolume;
    [SerializeField, BoxGroup("No Tweaks")]
    private UnityEngine.AI.NavMeshObstacle navMeshObstacle;
    [SerializeField, BoxGroup("No Tweaks")]
    private bool isOnePillar;
    [SerializeField, BoxGroup("Tweaks"), ListDrawerSettings(CustomAddFunction = "OnLaserAdded", CustomRemoveElementFunction = "OnLaserRemoved")]
    private List<Laser> lasers;

    [SerializeField, BoxGroup("Warning Effect")]
    private List<ParticleSystem> warningEffects;

    private int allLayersIgnoreMyself;
    private Dictionary<PBRobot, float> lastTimeCauseDamageDict = new Dictionary<PBRobot, float>();

    private bool _isOnController = true;
    private IEnumerator Start()
    {
        if (alwaysEnableLaser)
            yield break;
        var enableLaserWaitTime = new WaitForSeconds(enableLaserDuration);
        var changeLaserWaitTime = new WaitForSeconds(changeInDuration);
        var disableLaserWaitTime = new WaitForSeconds(disableLaserDuration);

        while (true)
        {
            yield return enableLaserWaitTime;
            warningEffects.ForEach(v => v.transform.DOScale(Vector3.one * 0.3F, changeInDuration + disableLaserDuration));

            // SoundManager.Instance.PlayLoopSFX(SFX.LazerChangeActive, 0.5f, false, true, gameObject);
            enabled = false;

            yield return changeLaserWaitTime;


            yield return disableLaserWaitTime;

            // SoundManager.Instance.PlayLoopSFX(SFX.LazerFire, 0.5f, false, true, gameObject);
            enabled = true;
            warningEffects.ForEach(v => v.transform
            .DOScale(Vector3.zero, 0.3f));
        }
    }

    private void Awake()
    {
        allLayersIgnoreMyself = -1 ^ (1 << gameObject.layer);
        foreach (var laser in lasers)
        {
            laser.Initialize(this);
        }
        _isOnController = true;
    }

    private void OnEnable()
    {
        foreach (var laser in lasers)
        {
            laser.SetActive(true);
        }
        navMeshObstacle.enabled = true;
    }

    private void OnDisable()
    {
        foreach (var laser in lasers)
        {
            laser.SetActive(false);
        }
        navMeshObstacle.enabled = false;
        leftLaserExplosionHit.Stop();
        rightLaserExplosionHit.Stop();
    }

    private void Update()
    {
        if (!_isOnController) return;

        int totalTarget = 0;
        foreach (var laser in lasers)
        {
            if (isOnePillar)
            {
                laser.UpdateRayDistance();
            }
            var damagableTargets = laser.FindDamagableTargets();
            for (int i = 0; i < damagableTargets.Count; i++)
            {
                var target = damagableTargets[i];
                if (IsAbleToCauseDamage(target))
                {
                    CauseDamage(target);
                    if (i == 0)
                    {
                        leftLaserExplosionHit.transform.position = target.Item2.point + target.Item2.normal * explosionHitOffset;
                        leftLaserExplosionHit.Play();
                    }
                    if (i == damagableTargets.Count - 1)
                    {
                        if (laser.TrySphereCast(out RaycastHit rightHitInfo, false))
                        {
                            rightLaserExplosionHit.transform.position = rightHitInfo.point + rightHitInfo.normal * explosionHitOffset;
                            rightLaserExplosionHit.Play();
                        }
                    }
                }
            }
            totalTarget += damagableTargets.Count;
            laser.UpdateLaser();
        }

        if (totalTarget <= 0)
        {
            leftLaserExplosionHit.Stop();
            rightLaserExplosionHit.Stop();
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        navMeshObstacle.size = new Vector3(width * 2f + NavMeshObstacleWidthOffset, navMeshObstacle.size.y, navMeshObstacle.size.z);
        navMeshModifierVolume.size = new Vector3(width * 2f + NavMeshObstacleWidthOffset, navMeshModifierVolume.size.y, navMeshModifierVolume.size.z);
        leftPole.transform.localPosition = new Vector3(-width, leftPole.transform.localPosition.y, leftPole.transform.localPosition.z);
        rightPole.transform.localPosition = new Vector3(width, rightPole.transform.localPosition.y, rightPole.transform.localPosition.z);
        foreach (var laser in lasers)
        {
            laser.SetLaserPos(width, laserHeightRange.CalcInterpolatedValue(laser.NormalizedHeight));
        }
    }

    private void OnDrawGizmosSelected()
    {
        foreach (var laser in lasers)
        {
            laser.InvokeMethod("OnDrawGizmosSelected");
        }
    }

    private Laser OnLaserAdded()
    {
        var laser = new Laser();
        var laserModel = PrefabUtility.InstantiatePrefab(laserModelPrefab, modelTransform) as GameObject;
        laser.SetFieldValue("laserModel", laserModel);
        EditorUtility.SetDirty(this);
        return laser;
    }

    private void OnLaserRemoved(Laser newLaser)
    {
        if (newLaser.LaserModel != null)
        {
            DestroyImmediate(newLaser.LaserModel);
        }
        lasers.Remove(newLaser);
        EditorUtility.SetDirty(this);
    }
#endif

    private bool IsAbleToCauseDamage(Tuple<PBPart, RaycastHit> tuple)
    {
        if (tuple.Item1.RobotChassis == null || tuple.Item1.RobotChassis.Robot == null)
            return false;
        var robot = tuple.Item1.RobotChassis.Robot;
        return Time.time - lastTimeCauseDamageDict.Get(robot) > causeDamageEverySecs;
    }

    private void CauseDamage(Tuple<PBPart, RaycastHit> tuple)
    {
        lastTimeCauseDamageDict.Set(tuple.Item1.RobotChassis.Robot, Time.time);
        tuple.Item1.ReceiveDamage(this, Const.FloatValue.ZeroF);
        //Debug.Log($"Cause damage: {tuple.Item1} - {tuple.Item1.RobotChassis.Robot} - {Time.time}");
    }

    public float GetDamage()
    {
        return damage;
    }

    public void OnController()
    {
        _isOnController = true;
        OnEnable();
    }
    public void OffController()
    {
        _isOnController = false;
        OnDisable();
    }

    [Serializable]
    public class Laser
    {
        [SerializeField, Range(0f, 1f)]
        private float normalizedHeight = 0.5f;
        [SerializeField]
        private GameObject laserModel;

        private float maxRayDistance;
        private LineRenderer laserLineRenderer;
        private LaserFence laserFence;
        private Vector3 originalRightRayPointPos;
        private RaycastHit[] targetHitResults = new RaycastHit[20];

        public float NormalizedHeight => normalizedHeight;
        public GameObject LaserModel => laserModel;
        public Transform LeftRayPoint => LaserModel.transform.GetChild(0);
        public Transform RightRayPoint => LaserModel.transform.GetChild(1);
        public LineRenderer LaserLineRenderer
        {
            get
            {
                if (laserLineRenderer == null)
                    laserLineRenderer = LaserModel.GetComponent<LineRenderer>();
                return laserLineRenderer;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(LeftRayPoint.position + LeftRayPoint.up * RayPointOffset, LeftRayPoint.up * maxRayDistance);
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(RightRayPoint.position + RightRayPoint.up * RayPointOffset, RightRayPoint.up * maxRayDistance);
        }

        public void Initialize(LaserFence laserFence)
        {
            this.laserFence = laserFence;
            maxRayDistance = Vector3.Distance(LeftRayPoint.position, RightRayPoint.position) - RayPointOffset * 2f;
            LaserLineRenderer.enabled = true;
            LaserLineRenderer.positionCount = laserFence.segmentLength + 1;
            originalRightRayPointPos = RightRayPoint.localPosition;
            UpdateLaser();
        }

        public void SetLaserPos(float width, float height)
        {
            LaserModel.transform.localPosition = height * Vector3.up;
            LeftRayPoint.localPosition = new Vector3(-width + RayPointOffset, LeftRayPoint.localPosition.y, LeftRayPoint.localPosition.z);
            RightRayPoint.localPosition = new Vector3(width - RayPointOffset, RightRayPoint.localPosition.y, RightRayPoint.localPosition.z);
            originalRightRayPointPos = RightRayPoint.localPosition;
        }

        public void UpdateLaser(Vector3 from, Vector3 to)
        {
            if (LaserLineRenderer.positionCount != laserFence.segmentLength)
                LaserLineRenderer.positionCount = laserFence.segmentLength + 1;
            for (int i = 0; i <= laserFence.segmentLength; i++)
            {
                var t = (float)i / laserFence.segmentLength;
                var pos = Vector3.Lerp(from, to, t);
                pos.y += Mathf.Sin((t + Time.time) * Mathf.PI * laserFence.frequency) * laserFence.amplitude * (i > 1 && i < laserFence.segmentLength ? 1f : 0f);
                LaserLineRenderer.SetPosition(i, pos);
            }
        }

        public void UpdateLaser()
        {
            UpdateLaser(LeftRayPoint.localPosition + Vector3.right * RayPointOffset, RightRayPoint.localPosition - Vector3.right * RayPointOffset);
        }

        public List<Tuple<PBPart, RaycastHit>> FindDamagableTargets()
        {
            var damagableParts = new List<Tuple<PBPart, RaycastHit>>();
            var currentRayDistance = Vector3.Distance(LeftRayPoint.position, RightRayPoint.position) - RayPointOffset * 2f;
            var hitCount = Physics.SphereCastNonAlloc(LeftRayPoint.position + LeftRayPoint.up * RayPointOffset, 0.1f, LeftRayPoint.up, targetHitResults, currentRayDistance, laserFence.allLayersIgnoreMyself, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < hitCount; i++)
            {
                var hitInfo = targetHitResults[i];
                if (hitInfo.collider.TryGetComponent(out PBPart part))
                {
                    damagableParts.Add(Tuple.Create(part, hitInfo));
                }
            }
            return damagableParts;
        }

        public void UpdateRayDistance()
        {
            if (Physics.SphereCast(LeftRayPoint.position + LeftRayPoint.up * RayPointOffset, 0.1f, LeftRayPoint.up, out RaycastHit hit, maxRayDistance, laserFence.allLayersIgnoreMyself, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.TryGetComponent(out LaserStopper _))
                {
                    RightRayPoint.transform.position = hit.point + (hit.point - LeftRayPoint.transform.position).normalized * 0.1f;
                }
            }
            else
            {
                RightRayPoint.localPosition = originalRightRayPointPos;
            }
        }

        public bool TrySphereCast(out RaycastHit hitInfo, bool isLeft)
        {
            if (isLeft)
                return Physics.SphereCast(LeftRayPoint.position + LeftRayPoint.up * RayPointOffset, 0.1f, LeftRayPoint.up, out hitInfo, maxRayDistance, laserFence.allLayersIgnoreMyself, QueryTriggerInteraction.Ignore);
            else
                return Physics.SphereCast(RightRayPoint.position + RightRayPoint.up * RayPointOffset, 0.1f, RightRayPoint.up, out hitInfo, maxRayDistance, laserFence.allLayersIgnoreMyself, QueryTriggerInteraction.Ignore);
        }

        public void SetActive(bool isActive)
        {
            laserLineRenderer.enabled = isActive;
        }
    }
}