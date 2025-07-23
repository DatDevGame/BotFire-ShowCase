using System;
using System.Text;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HyrphusQ.Helpers;
using Sirenix.OdinInspector;

[Serializable]
public class LookingForTargetState : AIBotState
{
    private static float lastTimeFocusAtPlayer;
    private static readonly RangeFloatValue focusAtPlayerRandomTimeRange = new RangeFloatValue(10f, 15f) { minValue = 10f, maxValue = 15f };

    [ShowInInspector, ReadOnly]
    private bool isHighestOverallScore;
    [ShowInInspector, ReadOnly]
    private PBRobot playerRobot;
    [ShowInInspector, ReadOnly]
    private List<AIBotController> otherBotControllersIgnoreMyself;

    protected override void OnStateUpdate()
    {
        base.OnStateUpdate();
        if (TryFindTarget(out var target))
        {
            BotController.Target = target;
        }
        else
        {
            BotController.Target = null;
            BotController.CarPhysics.AccelInput = 0f;
            BotController.CarPhysics.RotationInput = 0f;
        }
    }

    public override void InitializeState(AIBotController botController)
    {
        base.InitializeState(botController);
        if (botController.Robot.RobotStatsSO != null)
        {
            isHighestOverallScore = Mathf.Approximately(botController.Robot.HighestOverallScore, botController.Robot.RobotStatsSO.value);
        }
        lastTimeFocusAtPlayer = 0f;
        var botControllers = ObjectFindCache<AIBotController>.GetAll(true);
        playerRobot = botControllers.FirstOrDefault(controller => controller.Robot.PersonalInfo.isLocal).Robot;
        otherBotControllersIgnoreMyself = botControllers.Where(controller => controller != BotController && !controller.Robot.PersonalInfo.isLocal).ToList();
    }

    private bool IsAbleToFocusAtPlayer()
    {
        return false;
        // return otherBotControllersIgnoreMyself.Count > 1 // In battle mode
        // && Time.time - lastTimeFocusAtPlayer >= 0 // Enough 10-15s
        // && !playerRobot.IsDead // Player is not died
        // && !otherBotControllersIgnoreMyself.Find(controller => controller.Target == playerRobot as INavigationPoint); // No one focus at player
    }

    [Button]
    private void ButtonTest()
    {
        BotController.StartStateMachine();
    }
    private bool TryFindTarget(out INavigationPoint target)
    {
        if (IsAbleToFocusAtPlayer())
        {
            lastTimeFocusAtPlayer = Time.time + focusAtPlayerRandomTimeRange.RandomRange();
            target = playerRobot;
#if UNITY_EDITOR
            Log(target);
#endif
            void Log(INavigationPoint finalTarget)
            {
                var logMessage = new StringBuilder();
                logMessage.AppendLine($"Try Focus At Player - {Time.time}");
                logMessage.AppendLine($"Final Point: {finalTarget}");
                base.Log(logMessage.ToString());
            }
        }
        else
        {

            var randomNavPoints = GetRandomNavPoints()
                .Where(v =>
                {
                    if (v is PBRobot robot)
                        return robot.TeamId != BotController.Robot.TeamId;
                    return true;
                })
                .ToList();

            var navPointRngInfo = new List<NavPointRngInfo>(randomNavPoints.Count);
            var navPointTravelCosts = new List<ValueTuple<INavigationPoint, float>>();
            // Calculate the traveling cost from this robot to each navigation point.
            foreach (var navPoint in randomNavPoints)
            {
                var cost = CalculateTravelCost(BotController.Robot.GetTargetPoint(), navPoint.GetTargetPoint());
                navPointTravelCosts.Add(ValueTuple.Create(navPoint, cost));
            }
            // Sort the traveling cost from this robot to navigation point in ascending order.
            navPointTravelCosts.Sort((x, y) => y.Item2.CompareTo(x.Item2));
            // Create a list contains navigation points and pick a random point in the list.
            for (int i = 0; i < navPointTravelCosts.Count; i++)
            {
                var probability = PB_AIProfile.GetProbabilityOfPointType(navPointTravelCosts[i].Item1.GetPointType(), isHighestOverallScore, BotController.AIProfile) * (i + 1);
                navPointRngInfo.Add(new NavPointRngInfo(probability, navPointTravelCosts[i].Item1));
            }
            target = navPointRngInfo.GetRandomRedistribute()?.NavigationPoint;
#if UNITY_EDITOR
            Log(target);
#endif
            void Log(INavigationPoint finalTarget)
            {
                var logMessage = new StringBuilder();
                logMessage.AppendLine($"Try Find Target State - {Time.time}");
                for (int i = 0; i < navPointTravelCosts.Count; i++)
                {
                    var travelCostPair = navPointTravelCosts[i];
                    var probability = navPointRngInfo.Find(item => item.NavigationPoint == travelCostPair.Item1).Probability;
                    logMessage.AppendLine($"Point: {travelCostPair.Item1} - Cost: {travelCostPair.Item2} - Probability: {probability}");
                }
                logMessage.AppendLine($"Final Point: {finalTarget}");
                base.Log(logMessage.ToString());
            }
        }
        return target != null;
    }

    // Should be replaced by calculate real cost (walkable path) to reach destination by using NavMesh API
    private float CalculateTravelCost(Vector3 from, Vector3 to)
    {
        return Vector3.Distance(from, to);
    }

    // private float CalculateTravelCost(NavMeshPath path)
    // {
    //     var corners = path.corners;
    //     if (corners.Length < 2)
    //         return Mathf.Infinity;
    //     var hit = new NavMeshHit();
    //     NavMesh.SamplePosition(corners[0], out hit, 0.1f, NavMesh.AllAreas);
    //     var pathCost = 0.0f;
    //     var costMultiplier = NavMesh.GetAreaCost(IndexFromMask(hit.mask));
    //     int mask = hit.mask;
    //     var rayStart = corners[0];
    //     for (int i = 1; i < corners.Length; ++i)
    //     {
    //         // the segment may contain several area types - iterate over each
    //         while (NavMesh.Raycast(rayStart, corners[i], out hit, hit.mask))
    //         {
    //             pathCost += costMultiplier * hit.distance;
    //             costMultiplier = NavMesh.GetAreaCost(IndexFromMask(hit.mask));
    //             mask = hit.mask;
    //             rayStart = hit.position;
    //         }
    //         // advance to next segment
    //         pathCost += costMultiplier * hit.distance;
    //         costMultiplier = NavMesh.GetAreaCost(IndexFromMask(hit.mask));
    //         mask = hit.mask;
    //         rayStart = hit.position;
    //     }
    //     return pathCost;

    //     // return index for mask if it has exactly one bit set
    //     // otherwise returns -1
    //     int IndexFromMask(int mask)
    //     {
    //         for (int i = 0; i < 32; ++i)
    //         {
    //             if ((1 << i) == mask)
    //                 return i;
    //         }
    //         return -1;
    //     }
    // }

    protected List<INavigationPoint> GetRandomNavPoints()
    {
        var previousTarget = BotController.Target;
        var navigationPoints = new List<INavigationPoint>();
        var navigationPointDictionary = PBFightingStage.Instance.GetAllNavigationPoints();
        foreach (var item in navigationPointDictionary)
        {
            // Not able to get UtilityPoint or NormalPoint 2 times consecutively
            if (previousTarget != null && previousTarget.GetPointType() == PointType.UtilityPoint && item.Key == PointType.UtilityPoint)
            {
                continue;
            }
            if (previousTarget != null && previousTarget.GetPointType() == PointType.NormalPoint && item.Key == PointType.NormalPoint)
            {
                continue;
            }
            var randomNavPoint = item.Value.Where(FilterNavPoint).ToArray().GetRandom();
            if (randomNavPoint != null)
                navigationPoints.Add(randomNavPoint);
        }
        return navigationPoints;
    }

    protected bool FilterNavPoint(INavigationPoint point)
    {
        if (!point.IsAvailable())
            return false;
        if (point == BotController.Robot as INavigationPoint)
            return false;
        if (point.GetPointType() == PointType.CollectablePoint && !BotController.IsAbleToReach(point))
            return false;
        if (point.GetPointType() == PointType.UtilityPoint)
        {
            var pointToMe = Vector3.Scale((point.GetTargetPoint() - BotController.Robot.GetTargetPoint()).normalized, new Vector3(1f, 0f, 1f));
            var forwardDir = BotController.Robot.ChassisInstanceTransform.forward;
            if (point is AcceleratingPad acceleratingPadPoint && (Vector3.Angle(pointToMe, acceleratingPadPoint.GetForceDirection()) > 90f || Vector3.Angle(forwardDir, acceleratingPadPoint.GetForceDirection()) > 90f))
            {
                return false;
            }
            else if (point is JumpingPad jumpingPadPoint && (Vector3.Angle(pointToMe, Vector3.Scale(jumpingPadPoint.GetForceDirection(), new Vector3(1f, 0f, 1f))) > 90f || Vector3.Angle(forwardDir, Vector3.Scale(jumpingPadPoint.GetForceDirection(), new Vector3(1f, 0f, 1f))) > 90f))
            {
                return false;
            }
        }
        return true;
    }

    public class NavPointRngInfo : IRandomizable
    {
        public NavPointRngInfo(float probability, INavigationPoint navigationPoint)
        {
            this.probability = probability;
            this.navigationPoint = navigationPoint;
        }

        private float probability;
        private INavigationPoint navigationPoint;

        public float Probability
        {
            get => probability;
            set => probability = value;
        }
        public INavigationPoint NavigationPoint
        {
            get => navigationPoint;
            set => navigationPoint = value;
        }
    }
}
[Serializable]
public class LookingForTargetToChasingTargetTransition : AIBotStateTransition
{
    protected override bool Decide()
    {
        if (botController.Target != null)
        {
            Log($"LookingForTarget->ChasingTarget because the target is found");
            return true;
        }
        return false;
    }
}