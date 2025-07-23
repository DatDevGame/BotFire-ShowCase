using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public static class NavMeshHelper
{
    public const float ReachablePointPartitionAngles = 15f;
    public const float ReachablePointMaxDistance = 5f;

    public static bool TryGetRandomReachablePointFromSource(Transform source, out Vector3 point, float angles = ReachablePointPartitionAngles, float maxDistance = ReachablePointMaxDistance, Predicate<Vector3> filterPredicate = null)
    {
        bool isSuccess = TryGetReachablePointsFromSource(source, out List<Vector3> points, angles, maxDistance, filterPredicate);
        point = points.GetRandom();
        return isSuccess;
    }

    public static bool TryGetReachablePointsFromSource(CarPhysics carPhysics, out List<Vector3> points, float angles = ReachablePointPartitionAngles, float maxDistanceMultiplier = 1f, Predicate<Vector3> filterPredicate = null)
    {
        var halfDiagonalOfBounds = carPhysics.CalcDiagonalOfBounds() / 2f;
        var maxDistance = (ReachablePointMaxDistance + halfDiagonalOfBounds) * maxDistanceMultiplier;
        return TryGetReachablePointsFromSource(carPhysics.transform, out points, angles, maxDistance, filterPredicate);
    }

    public static bool TryGetReachablePointsFromSource(Transform source, out List<Vector3> points, float angles = ReachablePointPartitionAngles, float maxDistance = ReachablePointMaxDistance, Predicate<Vector3> filterPredicate = null)
    {
        return TryGetReachablePointsFromSource(source.position, source.rotation, out points, angles, maxDistance, filterPredicate);
    }

    public static bool TryGetReachablePointsFromSource(Vector3 position, Quaternion rotation, out List<Vector3> points, float angles = ReachablePointPartitionAngles, float maxDistance = ReachablePointMaxDistance, Predicate<Vector3> filterPredicate = null)
    {
        var validPoints = new List<Vector3>();
        var count = 360f / angles;
        for (int i = 0; i < count; i++)
        {
            var direction = rotation * Quaternion.Euler(new Vector3(0f, i * angles, 0f)) * Vector3.forward;
            var sourcePos = position;
            var targetPos = position + direction * maxDistance;
            if (!NavMesh.Raycast(sourcePos, targetPos, out var hit, NavMesh.AllAreas))
            {
                if (filterPredicate?.Invoke(targetPos) ?? true)
                    validPoints.Add(targetPos);
            }
        }
        points = validPoints;
        return validPoints.Count > 0;
    }

    public static int CalcNumOfReachablePoints(Vector3 position, Quaternion rotation, float angles = ReachablePointPartitionAngles, float maxDistance = ReachablePointMaxDistance, Predicate<Vector3> filterPredicate = null)
    {
        if (TryGetReachablePointsFromSource(position, rotation, out var points, angles, maxDistance, filterPredicate))
        {
            return points.Count;
        }
        return 0;
    }
}