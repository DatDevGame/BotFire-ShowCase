using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class AutoAimPart : MonoBehaviour
{
    [SerializeField] private Rigidbody _rbPart;
    [SerializeField] private Transform _pillarGun;
    [SerializeField] private float _timeResetAxis = 1.5f;
    [SerializeField] private float maxDistance = 10.0f;
    [SerializeField] private float detectionAngle = 45f;

    private PBPart m_MyPart;
    private CarPhysics m_CarPhysics;
    private float _timeMinus;
    private Vector3 _rotationStart;
    private LayerMask _detectionLayer;
    private bool m_IsAimTarget = false;

    public bool IsActiveAutoAim = true;
    public bool IsAimTarget => m_IsAimTarget;

    public void InitData(float maxDistance, float angle)
    {
        this.maxDistance = maxDistance;
        detectionAngle = angle;
    }
    private void Start()
    {
        _rotationStart = _pillarGun.localEulerAngles;
        _detectionLayer = GetAllLayersExceptMask(1 << gameObject.layer);
        m_MyPart = GetComponent<PBPart>();
        if (m_MyPart == null)
            m_MyPart = GetComponentInParent<PBPart>();
    }

    private void Update()
    {
        if(IsActiveAutoAim)
            AutoAim();
    }

    private void AutoAim()
    {
        if (_rbPart == null) return;

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, maxDistance, _detectionLayer, QueryTriggerInteraction.Ignore);
        if (hitColliders.Length > 0)
        {
            PBPart hitCollider = hitColliders
                .Select(c => c != null ? c.GetComponent<PBPart>() : null)
                .Where(part => part != null && part.RobotChassis != null && part.RobotChassis.Robot.TeamId != m_MyPart.RobotChassis.Robot.TeamId)
                .OrderBy(part => Vector3.Distance(part.transform.position, transform.position))
                .FirstOrDefault();

            //Aim object
            if (hitCollider != null)
            {
                PBPart pbPart = hitCollider.GetComponent<PBPart>();
                if (pbPart != null)
                {
                    PBChassis pBChassis = pbPart.RobotChassis;
                    if (pBChassis != null )
                    {
                        CarPhysics carPhysics = pBChassis.CarPhysics;
                        if (carPhysics != null)
                        {
                            if (m_CarPhysics == null)
                            {
                                var connectedPart = GetComponent<ConnectedPart>() ?? GetComponentInParent<ConnectedPart>();
                                m_CarPhysics = connectedPart?.RobotChassis?.CarPhysics;
                            }

                            if (m_CarPhysics != null)
                            {
                                Vector3 direction = m_CarPhysics.transform.forward;
                                Vector3 hitDirection = (hitCollider.transform.position - m_CarPhysics.transform.position).normalized;
                                float angle = Vector3.Angle(direction, hitDirection);

                                // Kiểm tra góc nghiêng với mặt phẳng ngang (world up)
                                float tiltAngle = Vector3.Angle(direction, Vector3.up);

                                if (angle <= detectionAngle * 0.5f && carPhysics.enabled)
                                {
                                    if (tiltAngle <= 70 || tiltAngle >= 140)
                                    {
                                        ResetAxis();
                                        return;
                                    }
                                    _timeMinus = _timeResetAxis;
                                    _pillarGun.transform.DOLookAt(carPhysics.transform.position, 0.3f, AxisConstraint.None);
                                    m_IsAimTarget = true;
                                }
                            }
                        }
                    }
                }
            }
        }

        _timeMinus -= Time.deltaTime;
        if (_timeMinus <= 0)
            ResetAxis();
    }

    public void ResetAxis()
    {
        if (_pillarGun.localEulerAngles != _rotationStart)
        {
            _pillarGun.DOKill();
            _pillarGun.DOLocalRotate(_rotationStart, 0.2f);
            m_IsAimTarget = false;
        }
    }

    private LayerMask GetAllLayersExceptMask(LayerMask maskToExclude)
    {
        LayerMask allLayersMask = GetAllLayersMask();
        LayerMask invertedMask = ~maskToExclude;
        LayerMask resultMask = allLayersMask & invertedMask;

        return resultMask;
    }

    private LayerMask GetAllLayersMask()
    {
        LayerMask allLayersMask = 0;

        for (int i = 0; i < 32; i++)
        {
            if (IsLayerInUse(i))
            {
                allLayersMask |= 1 << i;
            }
        }

        return allLayersMask;
    }

    private bool IsLayerInUse(int layer)
    {
        // Check if the layer is in use in the project settings
        string layerName = LayerMask.LayerToName(layer);
        return !string.IsNullOrEmpty(layerName);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (_rbPart == null) return;

        Vector3 position = transform.position;
        Vector3 direction = _rbPart.transform.forward;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(position, maxDistance);

        Handles.color = new Color(1, 0, 0, 0.2f);
        Handles.DrawSolidArc(position, Vector3.up, Quaternion.Euler(0, -detectionAngle * 0.5f, 0) * direction, detectionAngle, maxDistance);

        Gizmos.color = Color.red;
        float halfAngle = detectionAngle * 0.5f;

        Quaternion leftRayRotation = Quaternion.AngleAxis(-halfAngle, Vector3.up);
        Quaternion rightRayRotation = Quaternion.AngleAxis(halfAngle, Vector3.up);
        Vector3 leftRayDirection = leftRayRotation * direction * maxDistance;
        Vector3 rightRayDirection = rightRayRotation * direction * maxDistance;
        Gizmos.DrawLine(position, position + leftRayDirection);
        Gizmos.DrawLine(position, position + rightRayDirection);

        Quaternion upRayRotation = Quaternion.AngleAxis(-halfAngle, Vector3.right);
        Quaternion downRayRotation = Quaternion.AngleAxis(halfAngle, Vector3.right);
        Vector3 upRayDirection = upRayRotation * direction * maxDistance;
        Vector3 downRayDirection = downRayRotation * direction * maxDistance;
        Gizmos.DrawLine(position, position + upRayDirection);
        Gizmos.DrawLine(position, position + downRayDirection);
    }
#endif
}
