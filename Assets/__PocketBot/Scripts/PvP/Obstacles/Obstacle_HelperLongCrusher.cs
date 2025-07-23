using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;

public class Obstacle_HelperLongCrusher : MonoBehaviour
{
    private enum ScaleDirection { X_Positive, X_Negative, Y_Positive, Y_Negative, Z_Positive, Z_Negative }

    [OnValueChanged(nameof(OnChangeScaleDirection))] 
    [SerializeField, BoxGroup("Config")] private ScaleDirection m_ScaleAxis = ScaleDirection.X_Positive;
    [SerializeField, BoxGroup("Config")] private bool m_IsLoop = false;
    [SerializeField, BoxGroup("Config")] private float m_StartDelay = 0f;
    [SerializeField, BoxGroup("Config")] private float m_ScaleInDelay = 5f;
    [SerializeField, BoxGroup("Config")] private float m_ScaleOutDelay = 5f;
    [SerializeField, BoxGroup("Config")] private float m_TargetLength = 1f;
    [SerializeField, BoxGroup("Config")] private float m_ScaleInDuration = 1f;
    [SerializeField, BoxGroup("Config")] private float m_ScaleOutDuration = 1f;

    [SerializeField, BoxGroup("Ref")] private Transform m_ObjectScale;
    [SerializeField, BoxGroup("Ref")] private Transform m_PointRoot;
    [SerializeField, BoxGroup("Ref")] private Transform m_Root;

    private Vector3 m_OriginalSize = Vector3.one;
    private Vector3 m_OriginalPosition = Vector3.zero;

    private void Start()
    {
        m_OriginalSize = m_ObjectScale.localScale;
        m_OriginalPosition = m_ObjectScale.position;

        StartCoroutine(StartScaling());
    }

    private void Update()
    {
        m_Root.transform.position = m_PointRoot.transform.position;
    }

    private IEnumerator StartScaling()
    {
        yield return new WaitForSeconds(m_StartDelay);
        while (m_IsLoop) 
        {
            yield return ActionScale();
        }

        yield return ActionScale();

        IEnumerator ActionScale()
        {
            yield return new WaitForSeconds(m_ScaleInDelay);
            yield return ApplyScaling();
            yield return new WaitForSeconds(m_ScaleOutDelay);
            yield return Reverse();
        }
    }
    private IEnumerator ApplyScaling()
    {
        Vector3 newScale = m_OriginalSize;
        Vector3 newPosition = m_OriginalPosition;
        bool isCompleted = false;

        switch (m_ScaleAxis)
        {
            case ScaleDirection.X_Positive:
                newScale.x = m_TargetLength;
                newPosition.x = m_OriginalPosition.x + (m_TargetLength - m_OriginalSize.x) / 2f;
                break;
            case ScaleDirection.X_Negative:
                newScale.x = m_TargetLength;
                newPosition.x = m_OriginalPosition.x - (m_TargetLength - m_OriginalSize.x) / 2f;
                break;
            case ScaleDirection.Y_Positive:
                newScale.y = m_TargetLength;
                newPosition.y = m_OriginalPosition.y + (m_TargetLength - m_OriginalSize.y) / 2f;
                break;
            case ScaleDirection.Y_Negative:
                newScale.y = m_TargetLength;
                newPosition.y = m_OriginalPosition.y - (m_TargetLength - m_OriginalSize.y) / 2f;
                break;
            case ScaleDirection.Z_Positive:
                newScale.z = m_TargetLength;
                newPosition.z = m_OriginalPosition.z + (m_TargetLength - m_OriginalSize.z) / 2f;
                break;
            case ScaleDirection.Z_Negative:
                newScale.z = m_TargetLength;
                newPosition.z = m_OriginalPosition.z - (m_TargetLength - m_OriginalSize.z) / 2f;
                break;
        }

        m_ObjectScale.DOScale(newScale, m_ScaleInDuration).SetEase(Ease.OutQuad);
        m_ObjectScale
            .DOMove(newPosition, m_ScaleInDuration).SetEase(Ease.OutQuad)
            .OnComplete(() => 
            {
                isCompleted = true;
            });

        yield return new WaitUntil(() => isCompleted);
    }

    [Button]
    private IEnumerator Reverse()
    {
        bool isCompleted = false;
        m_ObjectScale.DOScale(m_OriginalSize, m_ScaleOutDuration).SetEase(Ease.InQuad);
        m_ObjectScale
            .DOMove(m_OriginalPosition, m_ScaleOutDuration).SetEase(Ease.InQuad)
            .OnComplete(() =>
            {
                isCompleted = true;
            });

        yield return new WaitUntil(() => isCompleted);
    }

    private void OnChangeScaleDirection()
    {
        m_ObjectScale.localPosition = Vector3.zero;

        switch (m_ScaleAxis)
        {
            case ScaleDirection.X_Positive:
                m_ObjectScale.position = new Vector3(m_ObjectScale.position.x - 0.5f, m_ObjectScale.position.y, m_ObjectScale.position.z);
                m_ObjectScale.localScale = new Vector3(0, 1, 1);

                m_PointRoot.localPosition = new Vector3(0.5f ,0, 0);
                break;
            case ScaleDirection.X_Negative:
                m_ObjectScale.position = new Vector3(m_ObjectScale.position.x + 0.5f, m_ObjectScale.position.y, m_ObjectScale.position.z);
                m_ObjectScale.localScale = new Vector3(0, 1, 1);

                m_PointRoot.localPosition = new Vector3(-0.5f, 0, 0);
                break;
            case ScaleDirection.Y_Positive:
                m_ObjectScale.position = new Vector3(m_ObjectScale.position.x, m_ObjectScale.position.y - 0.5f, m_ObjectScale.position.z);
                m_ObjectScale.localScale = new Vector3(1, 0, 1);

                m_PointRoot.localPosition = new Vector3(0, 0.5f, 0);
                break;
            case ScaleDirection.Y_Negative:
                m_ObjectScale.position = new Vector3(m_ObjectScale.position.x, m_ObjectScale.position.y + 0.5f, m_ObjectScale.position.z);
                m_ObjectScale.localScale = new Vector3(1, 0, 1);

                m_PointRoot.localPosition = new Vector3(0, -0.5f, 0);
                break;
            case ScaleDirection.Z_Positive:
                m_ObjectScale.position = new Vector3(m_ObjectScale.position.x, m_ObjectScale.position.y, m_ObjectScale.position.z - 0.5f);
                m_ObjectScale.localScale = new Vector3(1, 1, 0);

                m_PointRoot.localPosition = new Vector3(0, 0, 0.5f);
                break;
            case ScaleDirection.Z_Negative:
                m_ObjectScale.position = new Vector3(m_ObjectScale.position.x, m_ObjectScale.position.y, m_ObjectScale.position.z + 0.5f);
                m_ObjectScale.localScale = new Vector3(1, 1, 0);

                m_PointRoot.localPosition = new Vector3(0, 0, -0.5f);
                break;
        }

        m_Root.position = m_PointRoot.position;
    }
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Vector3 previewScale = m_ObjectScale.localScale;
        Vector3 previewPosition = m_ObjectScale.position;

        switch (m_ScaleAxis)
        {
            case ScaleDirection.X_Positive:
                previewScale.x = m_TargetLength;
                previewPosition.x = transform.position.x + (m_TargetLength - transform.localScale.x) / 2f;
                break;
            case ScaleDirection.X_Negative:
                previewScale.x = m_TargetLength;
                previewPosition.x = transform.position.x - (m_TargetLength - transform.localScale.x) / 2f;
                break;
            case ScaleDirection.Y_Positive:
                previewScale.y = m_TargetLength;
                previewPosition.y = transform.position.y + (m_TargetLength - transform.localScale.y) / 2f;
                break;
            case ScaleDirection.Y_Negative:
                previewScale.y = m_TargetLength;
                previewPosition.y = transform.position.y - (m_TargetLength - transform.localScale.y) / 2f;
                break;
            case ScaleDirection.Z_Positive:
                previewScale.z = m_TargetLength;
                previewPosition.z = transform.position.z + (m_TargetLength - transform.localScale.z) / 2f;
                break;
            case ScaleDirection.Z_Negative:
                previewScale.z = m_TargetLength;
                previewPosition.z = transform.position.z - (m_TargetLength - transform.localScale.z) / 2f;
                break;
        }

        Gizmos.color = new Color(0, 1, 0, 0.3f);
        Gizmos.DrawCube(previewPosition, previewScale);

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(previewPosition, previewScale);
    }
#endif

}
