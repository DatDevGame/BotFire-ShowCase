using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LatteGames.PvP;
using HyrphusQ.Events;
using System.Linq;

[Serializable]
public class PBPvPStage
{
    public class SpawnStageTask : IAsyncTask
    {
        public event Action onCompleted
        {
            add
            {
                value?.Invoke();
            }
            remove
            {
                // Do nothing
            }
        }
        public bool isCompleted => true;
        public float percentageComplete => 1f;
    }
    public class DestroyStageTask : IAsyncTask
    {
        public event Action onCompleted
        {
            add
            {
                value?.Invoke();
            }
            remove
            {
                // Do nothing
            }
        }
        public bool isCompleted => true;
        public float percentageComplete => 1f;
    }

    [SerializeField]
    private GameObject m_StagePrefab;

    private GameObject m_StageInstance;

    public GameObject GetStagePrefab()
    {
        return m_StagePrefab;
    }
    public IAsyncTask SpawnStage(Transform stageAnchor)
    {
        m_StageInstance = UnityEngine.Object.Instantiate(m_StagePrefab, stageAnchor);
        return new SpawnStageTask();
    }
    public IAsyncTask DestroyStage()
    {
        UnityEngine.Object.Destroy(m_StageInstance);
        return new DestroyStageTask();
    }
}

public class PBPvPStageSpawner : PvPStageSpawner
{
    [SerializeField] List<PBFightingStage> dualStageList, battleStageList;
    [SerializeField] ModeVariable currentChosenModeVar;
    [SerializeField] PvPArenaVariable arenaVariable;
    [SerializeField] BattleBetArenaVariable batleBetArenaVariable;
    [SerializeField] PBFightingStage testingStage;
    List<Sprite> currentStageAvatars = new();
    public List<Sprite> CurrentStageAvatars { get => currentStageAvatars; }

    private void Start()
    {
        GameEventHandler.Invoke(PBPvPEventCode.OnStartSearchingArena, this);
    }

    public virtual PBFightingStage GetCurrentFightingStagePrefab()
    {
        var arena = currentChosenModeVar.GameMode == Mode.Battle ?
            batleBetArenaVariable.StageArena :
            (PBPvPArenaSO)arenaVariable.value;
        var currentStageContainer = arena.Stages.Find(stage => stage.GetMode() == currentChosenModeVar.value);
        currentStageAvatars.Clear();
        currentStageContainer.RandomStageList.ForEach(stage => currentStageAvatars.Add(stage.GetStagePrefab().GetComponent<PBFightingStage>().GetThumbnail()));

        if (GameDataSO.Instance.isDevMode)
        {
            if(testingStage != null)
            {
                return testingStage;
            }
            if (!string.IsNullOrEmpty(CheatTestStageInputField.stageName))
            {
                var stageList = currentChosenModeVar.value == Mode.Normal ? dualStageList : battleStageList;
                var stage = stageList.Find(stage => string.Equals(stage.name, CheatTestStageInputField.stageName, StringComparison.OrdinalIgnoreCase));
                if (stage != null)
                {
                    return stage;
                }
            }
        }
        return currentStageContainer.GetCurrentFightingStagePrefab();
    }

    public virtual PBFightingStage SpawnStage(PBFightingStage fightingStagePrefab)
    {
        return Instantiate(fightingStagePrefab, transform);
    }

    public override IAsyncTask SpawnStage(PvPArenaSO arenaSO)
    {
        // Do nothing
        return null;
    }
}