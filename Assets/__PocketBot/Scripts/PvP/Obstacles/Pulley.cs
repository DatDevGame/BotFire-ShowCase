using System.Collections;
using System.Collections.Generic;
using LatteGames;
using Sirenix.OdinInspector;
using UnityEngine;

public class Pulley : MonoBehaviour
{
    [SerializeField, BoxGroup("Ref")] Transform load;
    [SerializeField, BoxGroup("Ref")] LineRenderer cable;
    [SerializeField, BoxGroup("Ref")] PulleyAnchor pulleyAnchor;
    [SerializeField, BoxGroup("Ref")] Rigidbody loadRigidbody;
    [SerializeField, BoxGroup("Motion Configs")] float swingAngle = 30f;
    [SerializeField, BoxGroup("Motion Configs")] float swingSpeed = 1f;
    [SerializeField, BoxGroup("Motion Configs")] float rotateYSpeed = 1f;
    [SerializeField, BoxGroup("Motion Configs")] Vector3 rotationAxis = Vector3.forward;
    [SerializeField, BoxGroup("Motion Configs")] float forceBonusWhenUnhook = 3;

    [SerializeField, BoxGroup("Retract Configs")] float retractSpeed = 1f;
    [SerializeField, BoxGroup("Retract Configs")] float unhookTime = 1.5f;

    private float initialAngle;
    private Vector3 initialOffset;
    private float initialTime;
    private Transform swingPivot;
    private Vector3 hookPointOfLoadRigidbody;
    private bool hasUnhook;
    private Vector3 currentDirection;

    List<Vector3> cablePoints = new List<Vector3>();

    void Start()
    {
        if (load == null)
        {
            return;
        }

        swingPivot = load.parent;
        if (swingPivot == null)
        {
            return;
        }

        initialAngle = swingPivot.localEulerAngles.z;
        initialOffset = load.position - swingPivot.position;
        initialTime = Time.time;

        pulleyAnchor.OnBreak += OnBreak;
        cablePoints.Add(pulleyAnchor.transform.position);
        cablePoints.Add(transform.position);
        cablePoints.Add(swingPivot.position);
        cable.positionCount = 3;

        loadRigidbody.isKinematic = true;

        hookPointOfLoadRigidbody = loadRigidbody.transform.InverseTransformPoint(swingPivot.position);
    }

    void OnBreak()
    {
        loadRigidbody.isKinematic = false;
        loadRigidbody.AddForce(currentDirection * forceBonusWhenUnhook, ForceMode.VelocityChange);
        Debug.Log($"Key Force - {currentDirection * forceBonusWhenUnhook} - currentDirection: {currentDirection} - forceBonusWhenUnhook: {forceBonusWhenUnhook}");
        StartCoroutine(CommonCoroutine.Delay(unhookTime, false, () => { hasUnhook = true; }));
    }

    void Update()
    {
        UpdateLoadMotion();
        UpdateCable();
    }

    void UpdateLoadMotion()
    {
        if (pulleyAnchor.HasBroken) return;
        if (swingPivot == null) return;

        var previousLoadPos = load.position;
        float angle = swingAngle * Mathf.Sin((Time.time - initialTime) * swingSpeed);

        swingPivot.RotateAround(transform.position, rotationAxis, angle - swingPivot.localEulerAngles.z + initialAngle);

        load.position = swingPivot.position + initialOffset;
        Vector3 worldUp = Vector3.up;
        Vector3 cubeUp = load.up;
        Quaternion targetRotation = Quaternion.FromToRotation(cubeUp, worldUp);
        load.localRotation = targetRotation * load.localRotation;
        load.RotateAround(swingPivot.position, load.up, rotateYSpeed * Time.deltaTime);

        currentDirection = (load.position - previousLoadPos).normalized;
    }

    void UpdateCable()
    {
        if (!pulleyAnchor.HasBroken)
        {
            cablePoints[0] = pulleyAnchor.transform.position;
            cablePoints[1] = transform.position;
            cablePoints[2] = swingPivot.position;
        }
        else
        {
            cablePoints[0] = Vector3.MoveTowards(cablePoints[0], cablePoints[1], retractSpeed * Time.deltaTime);
            cablePoints[1] = transform.position;
            if (hasUnhook)
            {
                cablePoints[2] = Vector3.MoveTowards(cablePoints[2], cablePoints[1], retractSpeed * Time.deltaTime);
            }
            else
            {
                cablePoints[2] = loadRigidbody.transform.TransformPoint(hookPointOfLoadRigidbody);
            }
        }
        cable.SetPositions(cablePoints.ToArray());
    }
}