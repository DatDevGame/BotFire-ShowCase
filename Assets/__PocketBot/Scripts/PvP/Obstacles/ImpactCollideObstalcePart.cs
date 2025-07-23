using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpactCollideObstalcePart : PartBehaviour
{
    [SerializeField] float minForcePower = 10;
    private Dictionary<PBRobot, float> robotLastTimeCauseDamageDict = new Dictionary<PBRobot, float>();

    public override bool IsAbleToDealDamage(Collision collision, out QueryResult queryResult)
    {
        queryResult = QueryResult.Default;
        var forcePower = Vector3.Dot(collision.relativeVelocity, -transform.right);
        if (forcePower < minForcePower)
            return false;
        var robot = collision.rigidbody.GetComponent<PBPart>().RobotChassis.Robot;
        var time = Time.time;
        if (time - robotLastTimeCauseDamageDict.Get(robot) < 0.5f)
            return false;
        robotLastTimeCauseDamageDict.Set(robot, time);
        return true;
    }
}