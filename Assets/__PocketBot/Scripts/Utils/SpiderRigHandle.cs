using System.Collections;
using System.Collections.Generic;
using FIMSpace.FProceduralAnimation;
using LatteGames.GameManagement;
using UnityEngine;

public class SpiderRigHandle : MonoBehaviour
{
    [SerializeField] private Joint _fixedJoint;

    private void Awake()
    {
        _fixedJoint = gameObject.GetComponentInChildren<Joint>();
    }

    private IEnumerator Start()
    {
        yield return null;
        if (!TryGetComponent<PBPart>(out var part))
            yield break;
        var rootRb = part.RobotChassis.CarPhysics.CarRb;
        _fixedJoint.connectedBody = rootRb;
    }
}