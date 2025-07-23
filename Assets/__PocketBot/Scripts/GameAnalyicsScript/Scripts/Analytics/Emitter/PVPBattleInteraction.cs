using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Sirenix.OdinInspector;

public class PVPBattleInteraction : MonoBehaviour
{
    [ShowInInspector, ReadOnly] private float _timeSurvival;
    [ShowInInspector, ReadOnly] private float _timeInterction;
    private float _distanceZone = 8;
    private List<CarPhysics> _carPhysicOpponents;
    private IEnumerator _CountTimeInterectionCR;

    public void StartInteraction(List<CarPhysics> carPhysics)
    {
        _carPhysicOpponents = carPhysics;
        if (_CountTimeInterectionCR != null)
            StopCoroutine(_CountTimeInterectionCR);
        _CountTimeInterectionCR = CountTimeInterection();
        StartCoroutine(_CountTimeInterectionCR);
    }
    public void StopInterection()
    {
        if (_CountTimeInterectionCR != null)
            StopCoroutine(_CountTimeInterectionCR);
    }

    public int CaculatedPercentInteraction()
    {
        return RoundToNearestMultipleOf5((_timeInterction / _timeSurvival) * 100);
    }

    private void OnDestroy()
    {
        if (_CountTimeInterectionCR != null)
            StopCoroutine(_CountTimeInterectionCR);
    }

    private int RoundToNearestMultipleOf5(float num)
    {
        // Round the number to the nearest integer
        int rounded = (int)num;

        // Calculate the remainder when dividing by 5
        int remainder = rounded % 5;

        // If the remainder is less than 3, round down; otherwise, round up
        if (remainder < 3)
        {
            return rounded - remainder;
        }
        else
        {
            return rounded + (5 - remainder);
        }
    }

    private IEnumerator CountTimeInterection()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(1);
        while (true)
        {
            _timeSurvival++;
            Vector3 playerPos = new Vector3(transform.position.x, 0, transform.position.z);
            bool isInteract = _carPhysicOpponents.Any(v => v != null && v.enabled && Vector3.Distance(new Vector3(v.transform.position.x, 0, v.transform.position.z), playerPos) <= _distanceZone);
            if (isInteract)
                _timeInterction++;

            yield return waitForSeconds;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // Set the color of the gizmo
        Gizmos.color = Color.yellow;

        // Draw a wireframe sphere gizmo with the specified radius at the object's position
        Gizmos.DrawWireSphere(transform.position, _distanceZone);
    }
#endif
}
