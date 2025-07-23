using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class ConstructStageSceneSetBotPosition : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField] Transform player, enemy;
    [SerializeField] PBFightingStage stage;
    [Button]
    void SetBotPosition()
    {
        if (stage == null)
        {
            stage = FindObjectOfType<PBFightingStage>();
        }
        player.position = stage.GetContestantSpawnPoints()[0].transform.position;
        enemy.position = stage.GetContestantSpawnPoints()[1].transform.position;
    }
#endif
}
