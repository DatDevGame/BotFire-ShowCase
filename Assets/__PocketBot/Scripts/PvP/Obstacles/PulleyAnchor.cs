using System;

using DG.Tweening;

using HyrphusQ.Events;

using Sirenix.OdinInspector;

using UnityEngine;

public class PulleyAnchor : MonoBehaviour, IDamagable
{
    public Action OnBreak;

    [SerializeField, BoxGroup("Breakable")] private float _breakForceMagnitudeThreshold = 500;

    [SerializeField, BoxGroup("AnChor Ref")] private Rigidbody m_AnchorRigidbody;

    [SerializeField, BoxGroup("AnChor Config")] private bool m_IsCollisionBreak;
    [SerializeField, BoxGroup("AnChor Config")] private float m_ExplosionForce = 20f;
    [SerializeField, BoxGroup("AnChor Config")] private float m_ExplosionRadius = 10f;
    [SerializeField, BoxGroup("AnChor Config")] private float m_UpwardModifier = 2f;

    [SerializeField, BoxGroup("AnChor Check Ground")] LayerMask m_GroundLayer;
    [SerializeField, BoxGroup("AnChor Check Ground")] float m_RaycastDistance = 1.0f;

    bool hasBroken = false;
    public bool HasBroken => hasBroken;

    private void Awake()
    {
        OnBreak += OnBeakHandle;
        GameEventHandler.AddActionEvent(CollisionEventCode.OnPartCollide, HandlePartCollide);
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(CollisionEventCode.OnPartCollide, HandlePartCollide);
    }

    private void Update()
    {
        bool isGrounded = Physics.Raycast(transform.position, Vector3.down, m_RaycastDistance, m_GroundLayer);
        if (!isGrounded && !hasBroken)
        {
            hasBroken = true;
            OnBreak?.Invoke();
        }
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

    private void OnBeakHandle()
    {
        m_AnchorRigidbody.isKinematic = false;
        m_AnchorRigidbody.useGravity = true;
        m_AnchorRigidbody.AddExplosionForce(m_ExplosionForce, transform.position, m_ExplosionRadius, m_UpwardModifier, ForceMode.Impulse);

        transform
            .DOScale(Vector3.zero, 1f)
            .SetDelay(2f);
    }

    [Button]
    void TestBreak()
    {
        if (!hasBroken)
        {
            hasBroken = true;
            OnBreak?.Invoke();
        }
    }

    public void ReceiveDamage(IAttackable attacker, float forceTaken)
    {
        if (!hasBroken)
        {
            hasBroken = true;
            OnBreak?.Invoke();

            #region Design Event
            try
            {
                if (PBFightingStage.Instance != null)
                {
                    if (attacker is PBPart attackerPart && attackerPart.RobotChassis?.Robot?.PersonalInfo?.isLocal == true)
                    {
                        string statgeID = ($"{PBFightingStage.Instance.name}").Replace(" Variant(Clone)", "");
                        string objectType = $"Pulley";
                        GameEventHandler.Invoke(DesignEvent.StageInteraction, statgeID, objectType);
                        Debug.Log($"Key - statgeID: {statgeID} | objectType: {objectType}");

                    }
                }
            }
            catch
            { }
            #endregion
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (m_IsCollisionBreak && !hasBroken)
        {
            hasBroken = true;
            OnBreak?.Invoke();

            #region Design Event
            try
            {
                if (PBFightingStage.Instance != null)
                {
                    PBPart part = collision.gameObject.gameObject.GetComponent<PBPart>();
                    if (part != null)
                    {
                        if (part.RobotChassis?.Robot?.PersonalInfo?.isLocal == true)
                        {
                            string statgeID = ($"{PBFightingStage.Instance.name}").Replace(" Variant(Clone)", "");
                            string objectType = $"Pulley";
                            GameEventHandler.Invoke(DesignEvent.StageInteraction, statgeID, objectType);
                            Debug.Log($"Key - statgeID: {statgeID} | objectType: {objectType}");

                        }
                    }
                }
            }
            catch
            { }
            #endregion

            return;
        }

        if (!hasBroken)
        {
            if (collision.impulse.magnitude > _breakForceMagnitudeThreshold)
            {
                hasBroken = true;
                OnBreak?.Invoke();
            }
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * m_RaycastDistance);
    }
#endif
}
