using UnityEngine;
using Sirenix.OdinInspector;

public enum MoveDirection
{
    Horizontal,
    Vertical,
    Both,
    Circle,
    TripleAngle
}


public class MovablePad : MonoBehaviour
{
    [Title("Movement Settings")]
    [SerializeField, BoxGroup("Config")] private bool m_IsActive = false;
    [SerializeField, BoxGroup("Config")] private float m_Speed = 5f;
    [SerializeField, BoxGroup("Config")] private Vector2 m_MoveRange = new Vector2(3f, 3f); // X: Trái-Phải, Z: Trước-Sau

    [EnumToggleButtons]
    [SerializeField, BoxGroup("Config")] private MoveDirection m_MoveDirection = MoveDirection.Both;

    private Vector3 m_StartPosition;
    private float m_TimeOffset;

    void Start()
    {
        m_StartPosition = transform.position;
        m_TimeOffset = Random.Range(0f, Mathf.PI * 2f); // Để các pad không đồng bộ nhau
    }

    private void Update()
    {
        if (!m_IsActive)
            return;

        float moveX = 0;
        float moveZ = 0;

        if (m_MoveDirection == MoveDirection.Circle)
        {
            moveX = Mathf.Sin(Time.time * m_Speed + m_TimeOffset) * m_MoveRange.x;
            moveZ = Mathf.Cos(Time.time * m_Speed + m_TimeOffset) * m_MoveRange.y;
        }
        else if (m_MoveDirection == MoveDirection.Both)
        {
            float move = Mathf.Sin(Time.time * m_Speed + m_TimeOffset);
            moveX = move * m_MoveRange.x;
            moveZ = move * m_MoveRange.y;
        }
        else if (m_MoveDirection == MoveDirection.TripleAngle)
        {
            float cycleTime = 3f; // Tổng thời gian đi hết một vòng tam giác
            float phase = (Time.time * m_Speed + m_TimeOffset) % cycleTime;

            Vector3 p1 = m_StartPosition + new Vector3(0, 0, m_MoveRange.y);
            Vector3 p2 = m_StartPosition + new Vector3(-m_MoveRange.x, 0, -m_MoveRange.y);
            Vector3 p3 = m_StartPosition + new Vector3(m_MoveRange.x, 0, -m_MoveRange.y);

            if (phase < cycleTime / 3f)
            {
                float t = phase / (cycleTime / 3f);
                transform.position = Vector3.Lerp(p1, p2, t);
            }
            else if (phase < 2f * cycleTime / 3f)
            {
                float t = (phase - cycleTime / 3f) / (cycleTime / 3f);
                transform.position = Vector3.Lerp(p2, p3, t);
            }
            else
            {
                float t = (phase - 2f * cycleTime / 3f) / (cycleTime / 3f);
                transform.position = Vector3.Lerp(p3, p1, t);
            }
            return;
        }
        else
        {
            moveX = (m_MoveDirection == MoveDirection.Horizontal) ? Mathf.Sin(Time.time * m_Speed + m_TimeOffset) * m_MoveRange.x : 0f;
            moveZ = (m_MoveDirection == MoveDirection.Vertical) ? Mathf.Cos(Time.time * m_Speed + m_TimeOffset) * m_MoveRange.y : 0f;
        }

        transform.position = m_StartPosition + new Vector3(moveX, 0f, moveZ);
    }

    void OnDrawGizmos()
    {
        if (!m_IsActive)
            return;

        if (!Application.isPlaying) m_StartPosition = transform.position;

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(m_StartPosition, new Vector3(m_MoveRange.x * 2, 0.1f, m_MoveRange.y * 2));

        // Vẽ đường di chuyển
        Gizmos.color = Color.red;
        Vector3 pointA = m_StartPosition + new Vector3(-m_MoveRange.x, 0f, -m_MoveRange.y);
        Vector3 pointB = m_StartPosition + new Vector3(m_MoveRange.x, 0f, m_MoveRange.y);

        if (m_MoveDirection == MoveDirection.Horizontal)
        {
            pointA = m_StartPosition + new Vector3(-m_MoveRange.x, 0f, 0f);
            pointB = m_StartPosition + new Vector3(m_MoveRange.x, 0f, 0f);
            Gizmos.DrawLine(pointA, pointB);
        }
        else if (m_MoveDirection == MoveDirection.Vertical)
        {
            pointA = m_StartPosition + new Vector3(0f, 0f, -m_MoveRange.y);
            pointB = m_StartPosition + new Vector3(0f, 0f, m_MoveRange.y);
            Gizmos.DrawLine(pointA, pointB);
        }
        else if (m_MoveDirection == MoveDirection.Both)
        {
            Gizmos.DrawLine(m_StartPosition + new Vector3(-m_MoveRange.x, 0f, -m_MoveRange.y),
                            m_StartPosition + new Vector3(m_MoveRange.x, 0f, m_MoveRange.y));
        }
        else if (m_MoveDirection == MoveDirection.Circle)
        {
            Gizmos.color = Color.cyan;
            int segments = 30;
            Vector3 prevPoint = m_StartPosition + new Vector3(m_MoveRange.x, 0f, 0f);
            for (int i = 1; i <= segments; i++)
            {
                float angle = (i / (float)segments) * Mathf.PI * 2;
                Vector3 newPoint = m_StartPosition + new Vector3(Mathf.Cos(angle) * m_MoveRange.x, 0f, Mathf.Sin(angle) * m_MoveRange.y);
                Gizmos.DrawLine(prevPoint, newPoint);
                prevPoint = newPoint;
            }
        }
        else if (m_MoveDirection == MoveDirection.TripleAngle)
        {
            Gizmos.color = Color.yellow;
            Vector3 p1 = m_StartPosition + new Vector3(0, 0, m_MoveRange.y);
            Vector3 p2 = m_StartPosition + new Vector3(-m_MoveRange.x, 0, -m_MoveRange.y);
            Vector3 p3 = m_StartPosition + new Vector3(m_MoveRange.x, 0, -m_MoveRange.y);

            Gizmos.DrawLine(p1, p2);
            Gizmos.DrawLine(p2, p3);
            Gizmos.DrawLine(p3, p1);
        }
    }
}
