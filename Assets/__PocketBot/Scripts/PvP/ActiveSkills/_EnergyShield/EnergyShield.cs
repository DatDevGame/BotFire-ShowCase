using System;
using System.Collections;
using DG.DemiLib;
using DG.Tweening;
using LatteGames;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

public class EnergyShield : MonoBehaviour
{
    private static readonly int k_NoiseTillingOffset_ID = Shader.PropertyToID("_Noise_ST");

    [SerializeField]
    private bool m_EnableShieldAfterWave = true;
    [SerializeField]
    private Vector2 m_OffsetSpeed = Vector2.down * 0.5f;
    [SerializeField]
    private float m_ExplosionRadius = 3f;
    [SerializeField]
    private AnimationConfig m_WaveAnimConfig;
    [SerializeField]
    private ParticleSystem m_OrbShieldVFX;
    [SerializeField]
    private ParticleSystemRenderer m_OrbShieldWaveVFX;
    [SerializeField]
    private NanoShieldVFX m_HexShieldVFX;
    [SerializeField]
    private HexShapeParticleEmitter m_ExplosionVFX;

    private Renderer m_OrbShieldVFXRenderer;
    public Renderer orbShieldVFXRenderer
    {
        get
        {
            if (m_OrbShieldVFXRenderer == null && m_OrbShieldVFX != null)
                m_OrbShieldVFXRenderer = m_OrbShieldVFX.GetComponent<Renderer>();
            return m_OrbShieldVFXRenderer;
        }
    }

    private void Awake()
    {
        m_HexShieldVFX.Initialize(this);
    }

    private void Update()
    {
        if (orbShieldVFXRenderer != null)
        {
            var tillingOffset = orbShieldVFXRenderer.material.GetVector(k_NoiseTillingOffset_ID);
            tillingOffset.z += m_OffsetSpeed.x * Time.deltaTime;
            tillingOffset.w += m_OffsetSpeed.y * Time.deltaTime;
            orbShieldVFXRenderer.material.SetVector(k_NoiseTillingOffset_ID, tillingOffset);
        }
        m_HexShieldVFX.Update();
    }

    [Button]
    public void CreateShield()
    {
        if (!m_EnableShieldAfterWave)
            m_OrbShieldVFX.Play();
        m_OrbShieldWaveVFX.gameObject.SetActive(true);
        m_OrbShieldWaveVFX.material.mainTextureOffset = Vector2.zero;
        m_OrbShieldWaveVFX.material.DOOffset(Vector2.one * -3.2f, m_WaveAnimConfig.duration).SetEase(m_WaveAnimConfig.ease).OnComplete(() =>
        {
            if (m_EnableShieldAfterWave)
                m_OrbShieldVFX.Play();
            m_OrbShieldWaveVFX.gameObject.SetActive(false);
        });
        m_HexShieldVFX.gameObject.SetActive(true);
        m_HexShieldVFX.PlayAnim();
    }

    [Button]
    public void Explode(Vector3 point)
    {
        m_OrbShieldVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        m_ExplosionVFX.transform.position = point;
        m_ExplosionVFX.PlayFX();
        m_HexShieldVFX.gameObject.SetActive(false);
        StartCoroutine(CommonCoroutine.Delay(0.1f, false, () =>
        {
            m_HexShieldVFX.gameObject.SetActive(true);
            m_HexShieldVFX.FadeOut();
            m_HexShieldVFX.Scale(Vector3.one * m_ExplosionRadius * 0.8f, Vector3.one * m_ExplosionRadius);
        }));

    }

    public void Initialize(float explosionRadius)
    {
        m_ExplosionRadius = explosionRadius / 1.5f;
        m_ExplosionVFX.transform.parent = null;
        m_ExplosionVFX.transform.localScale = explosionRadius / 4f * Vector3.one;
    }
}
[Serializable]
public class NanoShieldVFX
{
    [SerializeField]
    private float m_RotationOverTimeSpeed = 90f;
    [SerializeField]
    private int m_OuterVibrato = 5, m_InnerVibrato = 5;
    [SerializeField]
    private float m_OuterElasticity = 2f, m_InnerElasticity = 2f;
    [SerializeField]
    private Transform m_Outer;
    [SerializeField]
    private Transform m_Inner;
    [SerializeField]
    private Transform m_NanoShield;
    [SerializeField]
    private Renderer[] m_NanoShieldRenderers;
    [SerializeField]
    private float m_OuterScaleDelay = 0f;
    [SerializeField]
    private float m_InnerScaleDelay = 0f;
    [SerializeField]
    private AnimationConfig m_OuterScaleAnimConfig;
    [SerializeField]
    private AnimationConfig m_InnerScaleAnimConfig;
    [SerializeField]
    private RangeFloatValue m_OuterScaleRange;
    [SerializeField]
    private RangeFloatValue m_InnerScaleRange;
    [SerializeField]
    private AnimationConfig m_FadeOutAnimConfig;
    [SerializeField]
    private AnimationConfig m_ExplosionAnimConfig;

    private EnergyShield m_EnergyShield;

    public Transform transform => m_NanoShield;
    public GameObject gameObject => m_NanoShield.gameObject;

    public void Update()
    {
        if (gameObject.activeSelf)
        {
            transform.localRotation *= Quaternion.Euler(0f, m_RotationOverTimeSpeed * Time.deltaTime, 0f);
        }
    }

    [Button]
    public void PlayAnim()
    {
        transform.localScale = Vector3.one;
        m_NanoShieldRenderers.ForEach(renderer => renderer.material.DOFade(1f, 0f));
        m_Outer.transform.localScale = m_OuterScaleRange.minValue * Vector3.one;
        m_Outer.gameObject.SetActive(false);
        m_Inner.transform.localScale = m_InnerScaleRange.minValue * Vector3.one;
        m_Inner.gameObject.SetActive(false);
        m_EnergyShield.StartCoroutine(PlayAnim_CR());

        IEnumerator PlayAnim_CR()
        {
            if (m_OuterScaleDelay > 0f)
                yield return Yielders.Get(m_OuterScaleDelay);
            m_Outer.gameObject.SetActive(true);
            m_Outer.DOPunchScale(m_OuterScaleRange.maxValue * Vector3.one, m_OuterScaleAnimConfig.duration, m_OuterVibrato, m_OuterElasticity).SetEase(m_OuterScaleAnimConfig.ease);
            if (m_InnerScaleDelay > 0f)
                yield return Yielders.Get(m_InnerScaleDelay);
            m_Inner.gameObject.SetActive(true);
            m_Inner.DOPunchScale(m_InnerScaleRange.maxValue * Vector3.one, m_InnerScaleAnimConfig.duration, m_InnerVibrato, m_InnerElasticity).SetEase(m_InnerScaleAnimConfig.ease);
        }
    }

    [Button]
    public void FadeOut()
    {
        m_NanoShieldRenderers.ForEach(renderer => renderer.material.DOFade(0f, m_FadeOutAnimConfig.duration).SetEase(m_FadeOutAnimConfig.ease));
    }

    [Button]
    public void Scale(Vector3 from, Vector3 to)
    {
        transform.gameObject.SetActive(true);
        transform.localScale = from;
        transform.DOScale(to, m_ExplosionAnimConfig.duration).OnComplete(() =>
        {
            transform.gameObject.SetActive(false);
        });
    }

    public void Initialize(EnergyShield energyShield)
    {
        m_EnergyShield = energyShield;
    }
}