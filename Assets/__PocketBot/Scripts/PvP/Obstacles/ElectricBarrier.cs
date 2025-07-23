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

public class ElectricBarrier : MonoBehaviour, IAttackable
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
    private List<LaserElectric> lasers;

    [SerializeField, BoxGroup("Warning Effect")]
    private List<ParticleSystem> warningEffects;

    [SerializeField, BoxGroup("Electric VFX")] private ParticleSystem _electricVFX;
    [SerializeField, BoxGroup("Electric VFX")] private List<ParticleSystem> _electricLazerBeams;

    private int allLayersIgnoreMyself;
    private Dictionary<PBRobot, float> lastTimeCauseDamageDict = new Dictionary<PBRobot, float>();
    private Dictionary<string, string> _saveObjectDead;
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
            _electricLazerBeams.ForEach(v => v.gameObject.SetActive(false));
            //warningEffects.ForEach(v => v.transform.DOScale(Vector3.one * 0.3F, changeInDuration + disableLaserDuration));
            enabled = false;

            yield return changeLaserWaitTime;

            yield return disableLaserWaitTime;

            _electricLazerBeams.ForEach(v => v.gameObject.SetActive(true));
            enabled = true;
            warningEffects.ForEach(v => v.transform
            .DOScale(Vector3.zero, 0.3f));
        }
    }

    private void Awake()
    {
        _saveObjectDead = new Dictionary<string, string>();
        allLayersIgnoreMyself = -1 ^ (1 << gameObject.layer);
        foreach (var laser in lasers)
        {
            laser.Initialize(this);
        }
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
        int totalTarget = 0;
        foreach (var laser in lasers)
        {
            if (isOnePillar)
            {
                laser.UpdateRayDistance();
                for (int i = 0; i < lasers.Count; i++)
                {
                    var laserElectric = lasers[i];
                    var newWidth = (laserElectric.RightRayPoint.position - laserElectric.LeftRayPoint.position).magnitude;
                    var laserBeam = _electricLazerBeams[i];
                    laserBeam.transform.localScale = new Vector3(1, newWidth / 4.25f, 1);
                    laserBeam.transform.position = Vector3.Lerp(laserElectric.LeftRayPoint.position, laserElectric.RightRayPoint.position, 0.5f);
                }
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

        foreach (var lazerBeam in _electricLazerBeams)
            lazerBeam.transform.localScale = new Vector3(1, width / 2.3f, 1);
    }

    private void OnDrawGizmosSelected()
    {
        foreach (var laser in lasers)
        {
            laser.InvokeMethod("OnDrawGizmosSelected");
        }
    }

    private LaserElectric OnLaserAdded()
    {
        var laser = new LaserElectric();
        var laserModel = PrefabUtility.InstantiatePrefab(laserModelPrefab, modelTransform) as GameObject;
        laser.SetFieldValue("laserModel", laserModel);
        EditorUtility.SetDirty(this);
        return laser;
    }

    private void OnLaserRemoved(LaserElectric newLaser)
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
        var robot = tuple.Item1.RobotChassis.Robot;
        return Time.time - lastTimeCauseDamageDict.Get(robot) > causeDamageEverySecs;
    }

    private void CauseDamage(Tuple<PBPart, RaycastHit> tuple)
    {
        lastTimeCauseDamageDict.Set(tuple.Item1.RobotChassis.Robot, Time.time);
        tuple.Item1.ReceiveDamage(this, Const.FloatValue.ZeroF);

        if (tuple.Item1.RobotChassis.Robot.Health <= 0 && !_saveObjectDead.ContainsKey(tuple.Item1.RobotChassis.CarPhysics.name))
        {
            if (_electricVFX != null)
            {
                _saveObjectDead[tuple.Item1.RobotChassis.CarPhysics.name] = tuple.Item1.RobotChassis.CarPhysics.name;

                var electricVFX = Instantiate(_electricVFX, transform).GetComponent<ParticleSystem>();
                var electricVFXShape = electricVFX.shape;
                electricVFXShape.shapeType = ParticleSystemShapeType.MeshRenderer;

                var meshRenderBody = tuple.Item1.GetComponentInChildren<MeshRenderer>();

                if (meshRenderBody != null)
                {
                    electricVFX.Stop();
                    var mainVfx = electricVFX.main;
                    mainVfx.duration = 0.1f;

                    electricVFXShape.meshRenderer = meshRenderBody;
                    electricVFX.loop = true;
                    electricVFX.Play();
                }
            }
        }
    }

    public float GetDamage()
    {
        return damage;
    }

    [Serializable]
    public class LaserElectric
    {
        [SerializeField, Range(0f, 1f)]
        private float normalizedHeight = 0.5f;
        [SerializeField]
        private GameObject laserModel;

        private float maxRayDistance;
        private LineRenderer laserLineRenderer;
        private ElectricBarrier electricBarrier;
        private Vector3 originalRightRayPointPos;

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

        public void Initialize(ElectricBarrier electricBarrier)
        {
            this.electricBarrier = electricBarrier;
            maxRayDistance = Vector3.Distance(LeftRayPoint.position, RightRayPoint.position) - RayPointOffset * 2f;
            LaserLineRenderer.enabled = true;
            LaserLineRenderer.positionCount = electricBarrier.segmentLength + 1;
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
            if (LaserLineRenderer.positionCount != electricBarrier.segmentLength)
                LaserLineRenderer.positionCount = electricBarrier.segmentLength + 1;
            for (int i = 0; i <= electricBarrier.segmentLength; i++)
            {
                var t = (float)i / electricBarrier.segmentLength;
                var pos = Vector3.Lerp(from, to, t);
                pos.y += Mathf.Sin((t + Time.time) * Mathf.PI * electricBarrier.frequency) * electricBarrier.amplitude * (i > 1 && i < electricBarrier.segmentLength ? 1f : 0f);
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
            var hitInfos = Physics.SphereCastAll(LeftRayPoint.position + LeftRayPoint.up * RayPointOffset, 0.1f, LeftRayPoint.up, currentRayDistance, electricBarrier.allLayersIgnoreMyself, QueryTriggerInteraction.Ignore);
            foreach (var hitInfo in hitInfos)
            {
                if (hitInfo.collider.TryGetComponent(out PBPart part))
                {
                    damagableParts.Add(Tuple.Create(part, hitInfo));
                }
            }
            return damagableParts;
        }

        public void UpdateRayDistance()
        {
            RaycastHit hit;
            if (Physics.SphereCast(LeftRayPoint.position + LeftRayPoint.up * RayPointOffset, 0.1f, LeftRayPoint.up, out hit, maxRayDistance, electricBarrier.allLayersIgnoreMyself, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.TryGetComponent(out LaserStopper part))
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
                return Physics.SphereCast(LeftRayPoint.position + LeftRayPoint.up * RayPointOffset, 0.1f, LeftRayPoint.up, out hitInfo, maxRayDistance, electricBarrier.allLayersIgnoreMyself, QueryTriggerInteraction.Ignore);
            else
                return Physics.SphereCast(RightRayPoint.position + RightRayPoint.up * RayPointOffset, 0.1f, RightRayPoint.up, out hitInfo, maxRayDistance, electricBarrier.allLayersIgnoreMyself, QueryTriggerInteraction.Ignore);
        }

        public void SetActive(bool isActive)
        {
            laserLineRenderer.enabled = isActive;
        }
    }
}