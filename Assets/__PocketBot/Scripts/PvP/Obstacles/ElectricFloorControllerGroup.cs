using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricFloorControllerGroup : MonoBehaviour
{
    [SerializeField] private float enableDuration = 3f;
    [SerializeField] private List<ElectricFloorGroup> _electricFloorGroups;

    private void DisableAllLazer()
    {
        for (int x = 0; x < _electricFloorGroups.Count; x++)
        {
            for (int y = 0; y < _electricFloorGroups[x].ElectricFloor.Count; y++)
            {
                _electricFloorGroups[x].ElectricFloor[y].OffController();
            }
        }
    }
    private IEnumerator Start()
    {
        if (_electricFloorGroups.Count <= 0) yield break;
        DisableAllLazer();

        while (true)
        {
            for (int x = 0; x < _electricFloorGroups.Count; x++)
            {
                for (int y = 0; y < _electricFloorGroups[x].ElectricFloor.Count; y++)
                {
                    if (x > 0)
                        _electricFloorGroups[x - 1].ElectricFloor[y].OffController();

                    _electricFloorGroups[x].ElectricFloor[y].OnController();
                }
                yield return new WaitForSeconds(enableDuration);
            }

            DisableAllLazer();
        }
    }
}


[Serializable]
public class ElectricFloorGroup
{
    public List<ElectricFloor> ElectricFloor;
}
