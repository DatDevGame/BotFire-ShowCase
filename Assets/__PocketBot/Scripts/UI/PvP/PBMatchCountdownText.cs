using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using HyrphusQ.Events;
using LatteGames.Template;

public class PBMatchCountdownText : MonoBehaviour
{
    [SerializeField] PvPArenaVariable currentArenaSO;
    [SerializeField] TMP_Text countdownText;
    PBLevelController levelController;

    private void Start()
    {
        countdownText.gameObject.SetActive(false);
        levelController = ObjectFindCache<PBLevelController>.Get();
        GameEventHandler.AddActionEvent(PBLevelEventCode.OnLevelStart, HandleLevelStarted);
        GameEventHandler.AddActionEvent(PBLevelEventCode.OnLevelEnded, HandleLevelEnded);
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(PBLevelEventCode.OnLevelStart, HandleLevelStarted);
        GameEventHandler.RemoveActionEvent(PBLevelEventCode.OnLevelEnded, HandleLevelEnded);
    }

    void HandleLevelStarted()
    {
        countdownText.gameObject.SetActive(true);
    }

    void HandleLevelEnded()
    {
        countdownText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (levelController == null) return;
        if (countdownText.gameObject.activeInHierarchy == false) return;

        int totalSeconds = Mathf.CeilToInt(levelController.RemainingTime);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        countdownText.text = $"{minutes}:{seconds:00}";
    }

}
