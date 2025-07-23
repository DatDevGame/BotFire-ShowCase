using HyrphusQ.Events;
using LatteGames.Template;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LongCrusherContinousCollider : ContinuousCollider
{
    [SerializeField, BoxGroup("Visual")] protected Transform spinnerTransform;
    [SerializeField, BoxGroup("Visual")] protected Vector3 spinnerAxis = Vector3.right;
    [SerializeField, BoxGroup("Visual")] protected ParticleSystem sparkVFX;
    [SerializeField, BoxGroup("Visual")] protected float spinningSpeed = 25f;
    [SerializeField, BoxGroup("Visual")] protected bool spinOnContactOnly = true;
    [SerializeField, BoxGroup("Visual")] protected bool vfxAtContactPoint = true;
    [SerializeField, BoxGroup("Visual")] protected bool vfxIgnoreGround = false;

    float targetVelocity = 0;
    float currentVelocity;

    #region Design Event
    private bool m_IsInteractionPlayer = false;
    #endregion

    protected override void Start()
    {
        base.Start();
        if (spinOnContactOnly == false) targetVelocity = 1;
    }

    private void FixedUpdate()
    {
        if (!CheckEnablePart()) return;
        currentVelocity = Mathf.Lerp(currentVelocity, targetVelocity, Time.deltaTime * 4);
        spinnerTransform.Rotate(currentVelocity * spinningSpeed * spinnerAxis * Time.fixedDeltaTime);
    }

    private void OnDestroy()
    {
        if (sparkVFX != null) sparkVFX.Stop();
        SoundManager.Instance?.StopLoopSFX(gameObject);
    }

    private bool CheckEnablePart()
    {
        if (partBehaviour is not SmasherBehaviour)
            return true;
        else
            return (partBehaviour as SmasherBehaviour).IsEnable;
    }

    private void StopAllEffects(GameObject unityEventCallbackGO)
    {
        if (gameObject == null)
            return;
        if (unityEventCallbackGO.TryGetComponent(out UnityLifecycleEvent unityEventCallback))
        {
            unityEventCallback.onDisable -= StopAllEffects;
            if (sparkVFX != null)
            {
                sparkVFX.Stop();
            }
            SoundManager.Instance.PauseLoopSFX(gameObject);
        }
    }

    protected override void CollisionStayBehaviour(Collision collision)
    {
        if (!CheckEnablePart()) return;

        if (vfxIgnoreGround == true && collision.collider.CompareTag("Ground") == true)
            return;

        damageCoolDownHandle = DamageCooldown;

        base.CollisionStayBehaviour(collision);

        if (spinOnContactOnly == true)
        {
            targetVelocity = 1;
        }
        if (collision.rigidbody != null && collision.rigidbody.GetComponent<IDamagable>() != null)
        {
            UnityLifecycleEvent unityEventCallback = collision.rigidbody.gameObject.GetOrAddComponent<UnityLifecycleEvent>();
            unityEventCallback.onDisable -= StopAllEffects;
            unityEventCallback.onDisable += StopAllEffects;
            SoundManager.Instance.PlayLoopSFX(SFX.SawHit, PBSoundUtility.IsOnSound() ? 0.8f : 0, true, true, gameObject);
        }
        if (vfxAtContactPoint == true)
        {
            var contactPoint = transform.InverseTransformPoint(collision.GetContact(0).point);
            if (sparkVFX != null) sparkVFX.transform.localPosition = new Vector3(0, contactPoint.y, contactPoint.z);
        }
        if (sparkVFX != null && enabled)
        {
            sparkVFX.Play();
        }
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        PBPart part = collision.gameObject.GetComponent<PBPart>();
        PartBehaviour partBehaviour = collision.gameObject.GetComponent<PartBehaviour>();
        if (part != null)
        {
            #region Design Event
            try
            {
                if (PBFightingStage.Instance != null)
                {
                    if (part.RobotChassis?.Robot?.PersonalInfo?.isLocal == true && !m_IsInteractionPlayer)
                    {
                        string statgeID = ($"{PBFightingStage.Instance.name}").Replace(" Variant(Clone)", "");
                        string objectType = $"LongCrusher";
                        GameEventHandler.Invoke(DesignEvent.StageInteraction, statgeID, objectType);
                        m_IsInteractionPlayer = true;
                        Debug.Log($"Key - statgeID: {statgeID} | objectType: {objectType}");
                    }
                }
            }
            catch
            { }
            #endregion
        }
    }

    protected override void CollisionExitBehaviour(Collision collision)
    {
        base.CollisionExitBehaviour(collision);
        if (collision.rigidbody != null && collision.rigidbody.GetComponent<IDamagable>() != null)
        {
            SoundManager.Instance.PauseLoopSFX(gameObject);
        }
        if (sparkVFX != null)
        {
            sparkVFX.Stop();
        }
        if (spinOnContactOnly == true) targetVelocity = 0;
    }

    private void OnDisable()
    {
        if (sparkVFX != null)
        {
            sparkVFX.Stop();
        }
        SoundManager.Instance?.StopLoopSFX(gameObject);
    }
}
