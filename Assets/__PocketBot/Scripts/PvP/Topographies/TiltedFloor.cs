using System.Collections;
using UnityEngine;
using DG.Tweening; // Import DoTween
using Sirenix.OdinInspector;
using static TiltedFloor;

public class TiltedFloor : MonoBehaviour
{
    public enum TiltDirection { Right, Left, Forward, Backward }

    [SerializeField, BoxGroup("Config")] private bool m_IsRandom = true;
    [SerializeField, BoxGroup("Config"), HideIf("m_IsRandom")] private TiltDirection m_TiltDirection = TiltDirection.Right;
    [SerializeField, BoxGroup("Config")] private float m_TiltAngle = 10f;
    [SerializeField, BoxGroup("Config")] private float m_TiltDuration = 0.5f; 
    [SerializeField, BoxGroup("Config")] private float m_TiltHold = 0.5f;
    [SerializeField, BoxGroup("Config")] private float m_TiltInterval = 2f;

    void Start()
    {
        StartCoroutine(TiltRoutine());
    }

    IEnumerator TiltRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(m_TiltInterval);

            Vector3 direction = GetTiltDirection();

            Vector3 targetRotation = new Vector3(
                direction == Vector3.forward ? m_TiltAngle : direction == Vector3.back ? -m_TiltAngle : 0f,
                0f,
                direction == Vector3.right ? -m_TiltAngle : direction == Vector3.left ? m_TiltAngle : 0f);

            transform.DOLocalRotate(targetRotation, m_TiltDuration, RotateMode.LocalAxisAdd)
                .SetEase(Ease.InOutSine);

            yield return new WaitForSeconds(m_TiltHold);

            transform.DOLocalRotate(Vector3.zero, m_TiltDuration, RotateMode.Fast)
                .SetEase(Ease.InOutSine);
        }
    }

    private Vector3 GetTiltDirection()
    {
        if (m_IsRandom)
        {
            int randomDirection = Random.Range(0, 4);
            return randomDirection switch
            {
                0 => Vector3.right,
                1 => Vector3.left,
                2 => Vector3.forward,
                3 => Vector3.back,
                _ => Vector3.zero
            };
        }
        else
        {
            return m_TiltDirection switch
            {
                TiltDirection.Right => Vector3.right,
                TiltDirection.Left => Vector3.left,
                TiltDirection.Forward => Vector3.forward,
                TiltDirection.Backward => Vector3.back,
                _ => Vector3.zero
            };
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (m_IsRandom)
            return;

        if (!Application.isPlaying)
        {
            float lenghtLine = m_TiltDirection switch
            {
                TiltDirection.Right => transform.localScale.x / 2,
                TiltDirection.Left => transform.localScale.x / 2,
                TiltDirection.Forward => transform.localScale.z / 2,
                TiltDirection.Backward => transform.localScale.z / 2,
            };

            Vector3 direction = GetTiltDirection();
            Vector3 start = new Vector3(transform.position.x, transform.position.y + (transform.localScale.y / 2), transform.position.z) ;
            Vector3 end = start + direction * (lenghtLine + 0.5f);

            Color gizmoColor = Color.white;
            switch (m_TiltDirection)
            {
                case TiltDirection.Right: gizmoColor = Color.red; break;
                case TiltDirection.Left: gizmoColor = Color.blue; break;
                case TiltDirection.Forward: gizmoColor = Color.yellow; break;
                case TiltDirection.Backward: gizmoColor = Color.green; break;
            }

            Gizmos.color = gizmoColor;

            Gizmos.DrawLine(start, end);

            DrawArrow(end, direction, gizmoColor);

            Gizmos.color = Color.white;
            Gizmos.DrawCube(start, Vector3.one * 0.2f);
        }
    }

    private void DrawArrow(Vector3 position, Vector3 direction, Color color)
    {
        Gizmos.color = color;
        Gizmos.DrawSphere(position, 0.15f);

        Vector3 right = Quaternion.Euler(0, 30, 0) * -direction * 0.5f;
        Vector3 left = Quaternion.Euler(0, -30, 0) * -direction * 0.5f;

        Gizmos.DrawLine(position, position + right);
        Gizmos.DrawLine(position, position + left);
    }
#endif
}
