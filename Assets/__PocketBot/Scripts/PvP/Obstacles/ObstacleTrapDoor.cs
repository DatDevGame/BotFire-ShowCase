using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleTrapDoor : MonoBehaviour
{
    [SerializeField, BoxGroup("Config")] private float m_StartDelay = 1;
    [SerializeField, BoxGroup("Config")] private float m_OpenDelay = 2f;
    [SerializeField, BoxGroup("Config")] private float m_CloseDelay = 2f;
    [SerializeField, BoxGroup("Config")] private float m_OpenDuration = 1f;
    [SerializeField, BoxGroup("Config")] private float m_CloseDuration = 1f;

    [SerializeField, BoxGroup("Ref")] private SkinnedMeshRenderer m_SkinnedMeshRenderer;
    [SerializeField, BoxGroup("Ref")] private Transform m_MovingRoot, m_OpenPoint, m_ClosePoint;

    [SerializeField, BoxGroup("Link")] private BoosterSpawner m_BoosterSpawner;

    private IEnumerator m_RunTrapCoroutine;
    private bool m_IsOpen = false;
    private Tweener m_TweenerOpen;
    private Tweener m_TweenerClose;

    private void Awake()
    {
        if (m_BoosterSpawner != null)
            m_BoosterSpawner.OnSpawnBooster += OnSpawnItem;
        else
        {
            m_RunTrapCoroutine = Run();
            StartCoroutine(m_RunTrapCoroutine);
        }
    }

    private void Update()
    {
        if (m_BoosterSpawner != null)
        {
            if (!m_BoosterSpawner.IsHasBooster && m_IsOpen)
            {
                if (m_RunTrapCoroutine != null)
                    StopCoroutine(m_RunTrapCoroutine);

                m_IsOpen = false;

                m_TweenerOpen.Kill();
                m_TweenerClose.Kill();
                m_MovingRoot.DOKill();
                OnClose();
            }
        }
    }

    private void OnSpawnItem()
    {
        if (m_RunTrapCoroutine != null)
            StopCoroutine(m_RunTrapCoroutine);
        m_RunTrapCoroutine = Run(true);
        StartCoroutine(m_RunTrapCoroutine);
    }

    private IEnumerator Run(bool isIgnoreDelayStart = false)
    {
        if (!isIgnoreDelayStart)
            yield return new WaitForSeconds(m_StartDelay);

        while (true)
        {
            if (m_BoosterSpawner != null)
            {
                yield return new WaitUntil(() => m_BoosterSpawner.IsHasBooster);
            }

            bool phase_1 = false;
            bool phase_2 = false;

            yield return new WaitForSeconds(m_OpenDelay);
            OnOpen(() =>
            {
                phase_1 = true;
                m_IsOpen = true;
            });
            yield return new WaitUntil(() => phase_1);
            yield return new WaitForSeconds(m_OpenDelay);
            OnClose(() =>
            {
                phase_2 = true;
                m_IsOpen = false;
            });
            yield return new WaitUntil(() => phase_2);
        }
    }

    private void OnOpen(Action OnCallBack = null)
    {
        m_TweenerOpen = DOVirtual.Float(0, 100, m_OpenDuration, (value) =>
        {
            OnCallBack?.Invoke();
            m_SkinnedMeshRenderer.SetBlendShapeWeight(0, value);
        }).SetEase(Ease.OutQuad);

        m_MovingRoot.DOMove(m_OpenPoint.position, m_OpenDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                m_IsOpen = true;
            });
    }
    private void OnClose(Action OnCallBack = null)
    {
        m_TweenerClose = DOVirtual.Float(100, 0, m_CloseDuration, (value) =>
        {
            OnCallBack?.Invoke();
            m_SkinnedMeshRenderer.SetBlendShapeWeight(0, value);
        }).SetEase(Ease.InQuad);

        m_MovingRoot.DOMove(m_ClosePoint.position, m_CloseDuration)
            .SetEase(Ease.InQuad)
            .OnComplete(() =>
            {
                m_IsOpen = false;
            });
    }

    public bool IsOpen()
    {
        float middleY = (m_ClosePoint.position.y + m_OpenPoint.position.y) / 2f;
        return m_MovingRoot.transform.position.y >= middleY;
    }


#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(m_MovingRoot.position, 0.3f);

        Gizmos.color = Color.green;
        Gizmos.DrawCube(m_OpenPoint.position, new Vector3(1, 0.2f, 0.2f));
        Gizmos.DrawCube(m_OpenPoint.position, new Vector3(0.2f, 0.2f, 1));

        Gizmos.color = Color.red;
        Gizmos.DrawCube(m_ClosePoint.position, new Vector3(1, 0.2f, 0.2f));
        Gizmos.DrawCube(m_ClosePoint.position, new Vector3(0.2f, 0.2f, 1));
    }
#endif
}
