using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using LatteGames.Template;
using HyrphusQ.Events;

public class ChainsawCollider : ContinuousCollider, IBoostFireRate
{
    [SerializeField, BoxGroup("Visual")] MeshRenderer meshRenderer;
    [SerializeField, BoxGroup("Visual")] int materialIndex = 1;
    [SerializeField, BoxGroup("Visual")] ParticleSystem sparkVFX;
    [SerializeField, BoxGroup("Visual")] float spinningSpeed = 25f;
    [SerializeField, BoxGroup("Visual")] bool spinOnContactOnly = true;
    [SerializeField, BoxGroup("Visual")] bool vfxAtContactPoint = true;
    [SerializeField, BoxGroup("Visual")] bool vfxIgnoreGround = false;

    float targetVelocity = 0;
    float currentVelocity;
    float offsetX = 0;
    Material instanceMat;

    //Speed Up Handle
    private bool m_IsSpeedUp;
    private int m_StackSpeedUp;
    private float m_OriginalAttackSpeed;
    private float m_BoosterPercent;
    [SerializeField, BoxGroup("Visual")] private List<MeshRenderer> m_MeshRendererBooster;

    private void Awake()
    {
        m_OriginalAttackSpeed = spinningSpeed;
    }

    protected override void Start()
    {
        base.Start();
        if (spinOnContactOnly == false) targetVelocity = 1;
        instanceMat = meshRenderer.materials[materialIndex];
        sparkVFX.Stop();
    }

    private void Update()
    {
        if (!CheckEnablePart()) return;

        currentVelocity = Mathf.Lerp(currentVelocity, targetVelocity, Time.deltaTime * 4);
        offsetX += currentVelocity * spinningSpeed * Time.deltaTime;
        offsetX %= 100;
        instanceMat.mainTextureOffset = Vector2.right * offsetX;
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

        if (m_IsSpeedUp)
        {
            damageCoolDownHandle = ApplyBooster(DamageCooldown, m_BoosterPercent);
            if (damageCoolDownHandle <= 0)
                damageCoolDownHandle = 0.001f;

            float ApplyBooster(float currentCooldown, float boosterPercent)
            {
                return currentCooldown / (1 + boosterPercent);
            }
        }
        else
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
        if (sparkVFX != null)
        {
            sparkVFX.Play();
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

    public void BoostSpeedUpFire(float boosterPercent)
    {
        m_BoosterPercent += boosterPercent;

        //Enable VFX
        if (m_MeshRendererBooster != null && m_MeshRendererBooster.Count > 0)
            PBBoosterVFXManager.Instance.PlayBoosterSpeedUpAttackWeapon(partBehaviour.PbPart, m_MeshRendererBooster);

        m_StackSpeedUp++;
        m_IsSpeedUp = true;
        spinningSpeed += m_OriginalAttackSpeed * boosterPercent;
    }
    public void BoostSpeedUpStop(float boosterPercent)
    {
        m_StackSpeedUp--;
        if (m_StackSpeedUp <= 0)
        {
            //Disable VFX
            PBBoosterVFXManager.Instance.StopBoosterSpeedUpAttackWeapon(partBehaviour.PbPart);

            m_IsSpeedUp = false;
            m_BoosterPercent = 0;
            spinningSpeed = m_OriginalAttackSpeed;
        }
        else
        {
            m_BoosterPercent -= boosterPercent;
            spinningSpeed -= m_OriginalAttackSpeed * boosterPercent;
        }
    }

    public bool IsSpeedUp() => m_IsSpeedUp;
    public int GetStackSpeedUp() => m_StackSpeedUp;
    public float GetPercentSpeedUp() => m_BoosterPercent;
}
