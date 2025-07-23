using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class CharacterInfo : MonoBehaviour
{
    public Camera RenderCamera => m_RenderCamera;
    [ShowInInspector] public CharacterSystem CharacterSystem => m_CharacterSystem;
    [ShowInInspector] public RenderTexture RenderTexture => m_RenderTexture;
    [ShowInInspector] public CharacterSO CharacterSO => m_CharacterSO;

    [SerializeField, BoxGroup("Config")] private float m_SmoothTimeCamera = 0.1f;
    [SerializeField, BoxGroup("Ref")] private Camera m_RenderCamera;

    private CharacterSystem m_CharacterSystem;
    private RenderTexture m_RenderTexture;
    private CharacterSO m_CharacterSO;
    private bool m_IsCameraFollowingHead = false;


    private void Awake()
    {
        m_RenderTexture = RenderTexture.GetTemporary(256, 256, 0, GraphicsFormat.R8G8B8A8_UNorm);
        m_RenderCamera.targetTexture = m_RenderTexture;
    }

    private void OnDestroy()
    {
        m_RenderCamera.targetTexture = null;
        if (m_RenderTexture != null)
            RenderTexture.ReleaseTemporary(m_RenderTexture);
    }

    private void Update()
    {
        if (m_IsCameraFollowingHead)
        {
            //m_RenderCamera.transform.eulerAngles = new Vector3(0, m_RenderCamera.transform.eulerAngles.y, 0);
            m_RenderCamera.transform
                .DORotate(CharacterSystem.PlayerHeadPointCamera.eulerAngles, m_SmoothTimeCamera)
                .SetEase(Ease.Linear);
            m_RenderCamera.transform
                .DOMove(CharacterSystem.IsPlayer ? CharacterSystem.PlayerHeadPointCamera.position : CharacterSystem.OpponentHeadPointCamera.position, m_SmoothTimeCamera)
                .SetEase(Ease.Linear);

            m_RenderCamera.transform.DOLookAt(CharacterSystem.HeadCharacter.position, 0);
        }
    }

    public void SetCharacterSystem(CharacterSystem characterSystem, CharacterSO characterSO)
    {
        m_CharacterSO = characterSO;
        m_CharacterSystem = characterSystem;
        m_IsCameraFollowingHead = true;
    }
}
