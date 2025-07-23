using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewTagPlayButton : MonoBehaviour
{
    [SerializeField] PBOpenBossMapButton openBossMapButton;
    [SerializeField] GameObject newTag;

    private void Start()
    {
        newTag.SetActive(openBossMapButton.isUnlockedTrophyRequirement && openBossMapButton.isUnlocked && !openBossMapButton.isComingSoonChapter && openBossMapButton.currentBossIsUnlocked);
    }
}
