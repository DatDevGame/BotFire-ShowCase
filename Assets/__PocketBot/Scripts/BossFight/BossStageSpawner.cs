using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LatteGames.PvP;
using HyrphusQ.SerializedDataStructure;
using HyrphusQ.Events;

[Serializable]
public class BossStageSpawner : MonoBehaviour
{
    private BossSO bossSO => BossFightManager.Instance.bossMapSO.currentBossSO;

    private void Start()
    {
        SpawnStage();
    }
    public IAsyncTask SpawnStage()
    {
        var pvpStage = bossSO.stage;
        return pvpStage.SpawnStage(transform);
    }
}
