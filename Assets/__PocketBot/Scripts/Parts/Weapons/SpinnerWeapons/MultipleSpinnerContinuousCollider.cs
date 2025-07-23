using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using static PBRobot;

public class MultipleSpinnerContinuousCollider : ContinuousCollider, IBoostFireRate
{
    [SerializeField, BoxGroup("Visual")] List<VisualSpinner> visualSpinners;
    [SerializeField, BoxGroup("Visual")] float spinningSpeed = 25f;
    [SerializeField, BoxGroup("Visual")] bool spinOnContactOnly = true;
    [SerializeField, BoxGroup("Visual")] bool vfxAtContactPoint = true;
    [SerializeField, BoxGroup("Visual")] bool vfxIgnoreGround = false;

    float targetVelocity = 0;
    float currentVelocity;


    private bool m_IsSpeedUp = false;
    private int m_StackSpeedUp;
    private float m_ObjectTimeScale;
    private float m_TimeScaleOrginal => TimeManager.Instance.originalScaleTime;
    private float m_BoosterPercent;

    [SerializeField, BoxGroup("Visual")] private List<MeshRenderer> m_MeshRendererBooster;
    private void Awake()
    {
        m_ObjectTimeScale = m_TimeScaleOrginal;
    }

    protected override void Start()
    {
        base.Start();
        if (spinOnContactOnly == false) targetVelocity = 1;
        if (vfxIgnoreGround == true)
        {
            var groundList = GameObject.FindGameObjectsWithTag("Ground");
            var selfColliders = GetComponents<Collider>();
            List<Collider> groundColliders = new();
            foreach (var ground in groundList)
            {
                groundColliders.AddRange(ground.GetComponents<Collider>());
            }
            foreach (var collider in selfColliders)
            {
                foreach (var groundCollider in groundColliders)
                {
                    Physics.IgnoreCollision(collider, groundCollider, true);
                }
            }
        }
    }

    protected override void CollisionStayBehaviour(Collision collision)
    {
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

        foreach (var spinner in visualSpinners)
        {
            var sparkVFX = spinner.sparkVFX;
            if (spinOnContactOnly == true)
            {
                targetVelocity = 1;
                //SoundManager.Instance.PlaySFX(SFX.BladeHit);
            }
            if (vfxAtContactPoint == true)
            {
                var contactPoint = transform.InverseTransformPoint(collision.GetContact(0).point);
                if (sparkVFX != null) sparkVFX.transform.localPosition = new Vector3(0, contactPoint.y, contactPoint.z);
            }
            if (sparkVFX != null) sparkVFX.Play();
        }
    }

    private void FixedUpdate()
    {
        foreach (var spinner in visualSpinners)
        {
            var spinnerTransform = spinner.spinnerTransform;
            var spinnerAxis = spinner.spinAxis;
            currentVelocity = Mathf.Lerp(currentVelocity, targetVelocity, Time.deltaTime * 4);
            spinnerTransform.Rotate(currentVelocity * (spinningSpeed * m_ObjectTimeScale) * spinnerAxis * Time.fixedDeltaTime);
        }
    }

    private void OnDestroy()
    {
        foreach (var spinner in visualSpinners)
        {
            var sparkVFX = spinner.sparkVFX;
            if (sparkVFX != null) sparkVFX.Stop();
        }
    }

    protected override void CollisionExitBehaviour(Collision collision)
    {
        base.CollisionExitBehaviour(collision);
        foreach (var spinner in visualSpinners)
        {
            var sparkVFX = spinner.sparkVFX;
            if (sparkVFX != null) sparkVFX.Stop();
            if (spinOnContactOnly == true) targetVelocity = 0;
        }
    }
    public void BoostSpeedUpFire(float boosterPercent)
    {
        m_BoosterPercent += boosterPercent;

        //Enable VFX
        if (m_MeshRendererBooster != null && m_MeshRendererBooster.Count > 0)
            PBBoosterVFXManager.Instance.PlayBoosterSpeedUpAttackWeapon(partBehaviour.PbPart, m_MeshRendererBooster);

        m_StackSpeedUp++;
        m_IsSpeedUp = true;
        m_ObjectTimeScale += m_TimeScaleOrginal * boosterPercent;
    }
    public void BoostSpeedUpStop(float boosterPercent)
    {
        m_StackSpeedUp--;
        if (m_StackSpeedUp <= 0)
        {
            //Disable VFX
            PBBoosterVFXManager.Instance.StopBoosterSpeedUpAttackWeapon(partBehaviour.PbPart);

            m_IsSpeedUp = false;
            m_ObjectTimeScale = m_TimeScaleOrginal;
            m_BoosterPercent = 0;
        }
        else
        {
            m_BoosterPercent -= boosterPercent;
            m_ObjectTimeScale -= m_TimeScaleOrginal * boosterPercent;
        }
    }
    public bool IsSpeedUp() => m_IsSpeedUp;
    public int GetStackSpeedUp() => m_StackSpeedUp;
    public float GetPercentSpeedUp() => m_BoosterPercent;

    [System.Serializable]
    private class VisualSpinner
    {
        public Transform spinnerTransform;
        public Vector3 spinAxis;
        public ParticleSystem sparkVFX;
    }
}
