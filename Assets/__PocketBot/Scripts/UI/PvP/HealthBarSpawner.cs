using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HyrphusQ.Events;

public class HealthBarSpawner : MonoBehaviour
{
    [SerializeField] bool isShowFlag = true;
    [SerializeField] HealthBar healthBarPrefab;

    Dictionary<PBRobot, HealthBar> healthBarBindData = new();

    void Awake()
    {
        GameEventHandler.AddActionEvent(RobotStatusEventCode.OnModelSpawned, HandleRobotModelSpawned);
    }

    void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(RobotStatusEventCode.OnModelSpawned, HandleRobotModelSpawned);
    }

    void HandleRobotModelSpawned(params object[] paramters)
    {
        if (!gameObject.activeInHierarchy)
            return;
        if (paramters[0] is not PBRobot pbRobot) return;
        if (paramters[1] is not GameObject modelGameObject) return;
        HealthBar healthBar;
        if (healthBarBindData.ContainsKey(pbRobot)) healthBar = healthBarBindData[pbRobot];
        else
        {
            healthBar = Instantiate(healthBarPrefab, transform);
            healthBar.Spawner = this;
            if (!isShowFlag)
            {
                healthBar.TurnOffFlag();
            }
            healthBarBindData.Add(pbRobot, healthBar);
        }
        healthBar.Competitor = pbRobot;
        healthBar.robotTransform = modelGameObject.transform;
    }

    public void RemoveHealthBar(HealthBar healthBar)
    {
        healthBarBindData.Remove(healthBar.Competitor as PBRobot);
    }
}
