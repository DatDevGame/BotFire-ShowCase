using DG.Tweening;
using HyrphusQ.Events;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using LatteGames.Template;

public class ObstacleBreak : MonoBehaviour, IBreakObstacle, IDamagable
{
    [SerializeField, BoxGroup("Config")] private bool m_IsCollisionBreak;
    [SerializeField, BoxGroup("Config")] private float _healPoint;
    [SerializeField, BoxGroup("Config")] private float _maxHealPoint = 100;
    [SerializeField, BoxGroup("Config")] private float _lifeTime = 2;
    [SerializeField, BoxGroup("Ref")] private Collider _mainCollider;
    [SerializeField, BoxGroup("Ref")] private Rigidbody _mainrig;
    [SerializeField, BoxGroup("Ref")] private MeshRenderer _mainMeshObject;
    [SerializeField, BoxGroup("Ref")] private GameObject _objectBreak;
    [SerializeField, BoxGroup("Ref")] private GameObject _healbarObstaclePrefab;
    [SerializeField, BoxGroup("Ref")] private HiddenReward m_HiddenReward;
    [SerializeField, BoxGroup("Ref")] private List<MeshRenderer> m_SplinterMeshRendererDefaults;
    [SerializeField, BoxGroup("Ref")] private List<MeshRenderer> m_SplinterMeshRendererBreaks;

    private bool _isBroken;
    private HealthBarObstacle _healthBarObstacle;
    private bool m_IsHasBreak = false;
    private void Awake()
    {
        GameEventHandler.AddActionEvent(CollisionEventCode.OnPartCollide, HandlePartCollide);

        _healPoint = _maxHealPoint;
        if (m_IsCollisionBreak)
            _healPoint = 0;

        if (_healbarObstaclePrefab != null)
        {
            _healthBarObstacle = Instantiate(_healbarObstaclePrefab, transform).GetComponentInChildren<HealthBarObstacle>();
            if (_healthBarObstacle != null)
                _healthBarObstacle.SetUp(_objectBreak.transform);
        }

        _objectBreak.SetActive(false);
        m_SplinterMeshRendererDefaults.ForEach(v => v.enabled = true);
        m_SplinterMeshRendererBreaks.ForEach(v => v.enabled = false);
    }
    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(CollisionEventCode.OnPartCollide, HandlePartCollide);
    }

    [Button]
    private void BreakObject(IAttackable attacker = null)
    {
        if (m_IsHasBreak)
            return;

        #region Design Event
        try
        {
            if (PBFightingStage.Instance != null)
            {
                if (attacker is PBPart attackerPart && attackerPart.RobotChassis?.Robot?.PersonalInfo?.isLocal == true)
                {
                    string statgeID = ($"{PBFightingStage.Instance.name}").Replace(" Variant(Clone)", "");
                    string objectType = $"BreakableObject";
                    GameEventHandler.Invoke(DesignEvent.StageInteraction, statgeID, objectType);
                    Debug.Log($"Key - statgeID: {statgeID} | objectType: {objectType}");

                }
            }
        }
        catch
        {}
        #endregion

        m_IsHasBreak = true;

        SoundManager.Instance.PlaySFX(SFX.WoodenboxBreak);
        if ((attacker is PBPart part && part.RobotChassis != null && part.RobotChassis.Robot.PersonalInfo.isLocal) ||
            (attacker is PartBehaviour partBehaviour && partBehaviour.PbPart != null &&
            partBehaviour.PbPart.RobotChassis != null && partBehaviour.PbPart.RobotChassis.Robot.PersonalInfo.isLocal))
        {
            HapticManager.Instance.PlayFlashHaptic();
        }

        if (_healthBarObstacle != null)
            Destroy(_healthBarObstacle.gameObject);

        _isBroken = true;
        _mainrig.isKinematic = true;
        _mainCollider.enabled = false;

        _objectBreak.SetActive(true);
        m_SplinterMeshRendererDefaults
            .Where(x => x != null).ToList()
            .ForEach(v => v.enabled = false);

        m_SplinterMeshRendererBreaks
            .Where(x => x != null).ToList()
            .ForEach(v => v.enabled = true);

        if (m_HiddenReward != null)
            m_HiddenReward.Spawn(transform.position, attacker);

        StartCoroutine(LifeTimeSplinter());
    }
    private IEnumerator LifeTimeSplinter()
    {
        WaitForSeconds waitForSecondsDelay = new WaitForSeconds(_lifeTime);
        WaitForSeconds waitForSecondsEverySplinterDisable = new WaitForSeconds(0.1f);
        yield return waitForSecondsDelay;

        if (m_SplinterMeshRendererBreaks != null)
        {
            for (int i = 0; i < m_SplinterMeshRendererBreaks.Count; i++)
            {
                m_SplinterMeshRendererBreaks[i].gameObject.SetActive(false);
                yield return waitForSecondsEverySplinterDisable;
            }
        }
    }
    public void ReceiveDamage(IAttackable attacker, float forceTaken)
    {
        if (_maxHealPoint < 0)
            return;

        _healPoint -= attacker.GetDamage();
        if (_healPoint <= 0)
        {
            BreakObject(attacker);

            #region Design Event
            try
            {
                if (PBFightingStage.Instance != null)
                {
                    if (attacker is PBPart attackerPart && attackerPart.RobotChassis?.Robot?.PersonalInfo?.isLocal == true)
                    {
                        string statgeID = ($"{PBFightingStage.Instance.name}").Replace(" Variant(Clone)", "");
                        string objectType = $"BreakableObject";
                        GameEventHandler.Invoke(DesignEvent.StageInteraction, statgeID, objectType);
                        Debug.Log($"Key - statgeID: {statgeID} | objectType: {objectType}");

                    }
                }
            }
            catch
            { }
            #endregion

        }

        if (_healthBarObstacle != null)
            _healthBarObstacle.SetFilledAmount(_healPoint / _maxHealPoint);
    }

    private void HandlePartCollide(params object[] parameters)
    {
        if (parameters.Length <= 0) return;

        PBPart pBPart = null;
        Collision collision = null;
        Collider collider = null;
        CollisionInfo collisionInfo = null;

        if (parameters[0] is PBPart)
            pBPart = parameters[0] as PBPart;

        if (parameters[1] is Collision)
            collision = parameters[1] as Collision;

        if (parameters[1] is Collider)
            collider = parameters[1] as Collider;

        if (parameters[1] is CollisionInfo)
            collisionInfo = parameters[1] as CollisionInfo;


        if (pBPart == null) return;

        if (collisionInfo != null)
        {
            if (collisionInfo.body.gameObject == this.gameObject)
                ReceiveDamage(pBPart.GetComponent<IAttackable>(), Const.FloatValue.ZeroF);
        }
        if (collision != null)
        {
            if (collision.gameObject == this.gameObject)
                ReceiveDamage(pBPart.GetComponent<IAttackable>(), Const.FloatValue.ZeroF);
        }
        if (collider != null)
        {
            if (collider.gameObject == this.gameObject)
                ReceiveDamage(pBPart.GetComponent<IAttackable>(), Const.FloatValue.ZeroF);
        }
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (m_IsCollisionBreak)
        {
            PBPart part = collision.gameObject.GetComponent<PBPart>();
            PartBehaviour partBehaviour = collision.gameObject.GetComponent<PartBehaviour>();
            if (part != null)
            {
                BreakObject(part);
                return;
            }

            if (partBehaviour != null)
                BreakObject();
        }
    }

    public bool IsBroken() => _isBroken;
}
public interface IBreakObstacle
{
    public bool IsBroken();
}
