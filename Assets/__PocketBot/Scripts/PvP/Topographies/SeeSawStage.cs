using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using LatteGames;

public class SeeSawStage : MonoBehaviour
{
    [SerializeField] private float m_StartTiltedDelay = 3;
    [SerializeField] private float m_MaxTiltAngle = 30f;
    [SerializeField] private float m_TiltSpeed = 1;
    [SerializeField] private float m_TorqueMultiplier = 10;
    [SerializeField] private float tiltRecoverySpeed = 1.0f; // Tốc độ hồi phục về cân bằng
    [SerializeField] private float distanceMultiplier = 1.5f; // Hệ số tăng ảnh hưởng theo khoảng cách

    [SerializeField] Transform m_Axis;
    [SerializeField] Vector3 boxSize = new Vector3(2, 2, 2);
    [SerializeField] Vector3 boxOffset = Vector3.zero;
    [SerializeField] LayerMask detectLayer;

    private Tweener m_TiltTweener;
    private bool m_IsCanRotation = false;
    [ShowInInspector] private Dictionary<PBChassis, Rigidbody> m_Cars = new Dictionary<PBChassis, Rigidbody>();

    private void Start()
    {
        StartCoroutine(CommonCoroutine.Delay(m_StartTiltedDelay, false, () => { m_IsCanRotation = true; }));
    }

    private void FixedUpdate()
    {
        if (!m_IsCanRotation)
            return;

        Vector3 checkPosition = transform.position + transform.rotation * boxOffset;
        Collider[] objectsInside = Physics.OverlapBox(checkPosition, boxSize / 2, transform.rotation, detectLayer);

        if (objectsInside.Length > 0)
        {
            ApplyCarTilt(objectsInside);
        }
        else
        {
            RecoverBalance();
        }
    }

    private void ApplyCarTilt(Collider[] objectsInside)
    {
        float totalTorque = 0;
        float totalWeight = 0;

        foreach (Collider col in objectsInside)
        {
            m_Cars = m_Cars.Where(v => v.Value != null || v.Key != null)
               .ToDictionary(v => v.Key, v => v.Value);

            PBPart part = col.GetComponent<PBPart>();
            Rigidbody carRb = null;
            if (part != null && part.RobotChassis != null)
            {
                if (m_Cars.ContainsKey(part.RobotChassis))
                    carRb = m_Cars[part.RobotChassis];
                else
                {
                    m_Cars.Add(part.RobotChassis, part.RobotBaseBody);
                    carRb = part.RobotBaseBody;
                }
            }

            float weight = 0;
            float distanceFactor = 0;
            //Handle Car
            if (carRb != null)
            {
                Vector3 localPos = transform.InverseTransformPoint(col.transform.position);

                weight = carRb.mass;
                distanceFactor = Mathf.Abs(localPos.x) * distanceMultiplier;
                totalTorque += -localPos.x * weight * distanceFactor;
                totalWeight += weight;
                totalTorque = totalTorque * m_TorqueMultiplier;
            }

            //Handle Obj Other
            if (part == null && carRb == null)
            {
                Vector3 localPos = transform.InverseTransformPoint(col.transform.position);
                Rigidbody objOther = col.GetComponent<Rigidbody>();
                if (objOther != null)
                {
                    weight = objOther.mass;
                    distanceFactor = Mathf.Abs(localPos.x) * distanceMultiplier;
                    totalTorque += -localPos.x * weight * distanceFactor;
                    totalWeight += weight;
                    totalTorque = totalTorque * m_TorqueMultiplier;
                }
            }
        }

        if (totalWeight > 0)
        {
            float targetAngle = totalTorque / totalWeight;
            targetAngle = Mathf.Clamp(targetAngle, -m_MaxTiltAngle, m_MaxTiltAngle);

            if (m_TiltTweener != null && m_TiltTweener.IsActive())
                m_TiltTweener.Kill();

            m_TiltTweener = m_Axis.DOLocalRotate(new Vector3(0, 0, targetAngle), m_TiltSpeed)
                .SetEase(Ease.InOutSine);
        }
    }

    private void RecoverBalance()
    {
        if (m_TiltTweener != null && m_TiltTweener.IsActive())
            m_TiltTweener.Kill();

        m_TiltTweener = m_Axis.DOLocalRotate(Vector3.zero, tiltRecoverySpeed)
            .SetEase(Ease.OutSine);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Vector3 checkPosition = transform.position + transform.rotation * boxOffset;
        Gizmos.matrix = Matrix4x4.TRS(checkPosition, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, boxSize);
    }
}
