using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "BossMapSO", menuName = "PocketBots/BossFight/BossMapSO")]
public class BossMapSO : SerializedScriptableObject
{
    public List<BossChapterSO> chapterList = new List<BossChapterSO>();

#if UNITY_EDITOR
    [BoxGroup("Editor Only")]
    [FolderPath]
    public string productsFolderPath;
    [BoxGroup("Editor Only")]
    [Button("GetProducts")]
    void GetProducts()
    {
        if (chapterList != null)
        {
            chapterList.Clear();
        }
        chapterList = EditorUtils.FindAssetsOfType<BossChapterSO>(productsFolderPath);
    }
#endif

    [Title("References")]
    public PPrefIntVariable chapterIndex;

    public BossSO currentBossSO => currentChapterSO.currentBossSO ?? previousChapterSO.lastBossSO;
    public BossSO previousBossSO
    {
        get
        {
            return currentChapterSO.bossIndex.value <= 0 ? currentBossSO : currentChapterSO.bossList[currentChapterSO.bossIndex.value - 1];
        }
    }
    public BossChapterSO GetBossChapterDefault => chapterList[0];
    public BossChapterSO currentChapterSO => chapterIndex >= chapterList.Count ? null : chapterList[chapterIndex];
    public BossChapterSO previousChapterSO => chapterIndex <= 0 ? null : chapterList[chapterIndex - 1];
    public int chapterCount => chapterList.Count;
}
