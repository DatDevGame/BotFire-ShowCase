using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "BossChapterSO", menuName = "PocketBots/BossFight/BossChapterSO")]
public class BossChapterSO : SerializedScriptableObject
{
    public List<BossSO> bossList = new List<BossSO>();

#if UNITY_EDITOR
    [BoxGroup("Editor Only")]
    [FolderPath]
    public string productsFolderPath;
    [BoxGroup("Editor Only")]
    [Button("GetProducts")]
    void GetProducts()
    {
        if (bossList != null)
        {
            bossList.Clear();
        }
        bossList = EditorUtils.FindAssetsOfType<BossSO>(productsFolderPath);
    }
#endif

    [Title("References")]
    public PPrefIntVariable bossIndex;
    [Title("Info")]
    public string chapterName;

    public BossSO currentBossSO => bossIndex >= bossList.Count ? null : bossList[bossIndex];
    public BossSO lastBossSO => bossList[bossList.Count - 1];
    public int bossCount => bossList.Count;
    public bool isComingSoonChapter => bossCount <= 0;
    public bool isUnlockedTrophyRequirement => bossList.Exists(x => x.unlockedTrophyAmount > 0);
}
