using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using HyrphusQ.SerializedDataStructure;

public class RocketMovementWithPhysicsRotation : MonoBehaviour
{
    [SerializeField] private List<Transform> rocketParts;
    [SerializeField] private List<GameObject> m_Jets;
    [SerializeField] private List<GameObject> m_JetsBooster;
    [SerializeField] private List<Vector3> m_DefaultRotateJets;

    private DoubleTapToReverse m_DoubleTapToReverse;
    private Joystick m_Joystick;
    private CarPhysics m_CarPhysics;

    private float m_MaxForceBooster = 80;

    private void Start()
    {
        m_DoubleTapToReverse = FindObjectOfType<DoubleTapToReverse>();
        m_Joystick = FindObjectOfType<Joystick>();
        m_CarPhysics = GetComponent<CarPhysics>();

        if (m_DoubleTapToReverse != null)
        {
            m_DoubleTapToReverse.OnStartReversing += OnStartReversing;
        }
    }

    private void OnStartReversing()
    {
        Vector3 newRotation = new Vector3(-65, 0, 0);
        rocketParts.ForEach(v => v.DOLocalRotate(newRotation, 0.5f));
    }

    private void FixedUpdate()
    {
        if (m_Joystick == null) return;
        float horizontal = m_Joystick.Horizontal;
        float vertical = m_Joystick.Vertical;
        Vector3 joyStickdir = Vector3.zero;

        horizontal = m_Joystick.Horizontal;
        vertical = m_Joystick.Vertical;
        joyStickdir = new Vector3(horizontal, 0, vertical);
        joyStickdir = MainCameraFindCache.Get().transform.TransformDirection(joyStickdir);
        
        var velocityCarPhysics = new Vector3(m_CarPhysics.CarRb.velocity.x, 0, m_CarPhysics.CarRb.velocity.z);
        float magnitude = velocityCarPhysics.magnitude;
        if (rocketParts.Count > 0)
        {
            for (int i = 0; i < rocketParts.Count; i++)
            {
                if (magnitude <= 2)
                {
                    rocketParts[i].DOLocalRotate(m_DefaultRotateJets[i], 3);
                }
                else
                {
                    float newXRotation = Mathf.Clamp(magnitude * 5.5f, 0, 90);
                    float newYRotation = m_CarPhysics.RotationInput * 50;
                    Vector3 newRotation = new Vector3(newXRotation, newYRotation, 0);
                    rocketParts[i].DOLocalRotate(newRotation, 3);
                    m_Jets.ForEach(v => v.SetActive(newXRotation < m_MaxForceBooster));
                    m_JetsBooster.ForEach(v => v.SetActive(newXRotation >= m_MaxForceBooster));
                }
            }
        }
    }
}
