using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "ObstaclePatternListSO", menuName = "PocketBots/PvP/ObstaclePatternListSO")]
public class ObstaclePatternListSO : ScriptableObject
{
    [SerializeField] ModeVariable currentChosenModeVariable;
    [SerializeField] List<PBObstaclePattern> ObstaclePatternsNormalMode;
    [SerializeField] List<PBObstaclePattern> ObstaclePatternsBattleMode;

    [System.Serializable]
    public class PBObstaclePattern
    {
        public List<ObstaclePosition> ObstaclePositions;
    }
    [System.Serializable]
    public struct ObstaclePosition
    {
        [HorizontalGroup, LabelWidth(50)]
        public int Type;

        [HorizontalGroup, LabelWidth(100)]
        public int PositionIndex;
    }

    public PBObstaclePattern GetRandomPattern()
    {
        var ObstaclePatterns = currentChosenModeVariable.value == Mode.Normal ? ObstaclePatternsNormalMode : ObstaclePatternsBattleMode;
        var randomPattern = ObstaclePatterns.GetRandom();
        return randomPattern;
    }
}
