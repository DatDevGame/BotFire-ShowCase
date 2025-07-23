using System;
using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using LatteGames.GameManagement;
using UnityEngine;

public class ActiveSkillIngameFTUE : MonoBehaviour
{
    [SerializeField]
    private PPrefBoolVariable ftueActiveSkillIngame;
    [SerializeField]
    private ActiveSkillButton activeSkillButton;
    [SerializeField]
    private GameObject ctaTextGO;

    private void Start()
    {
        if (SceneManager.GetActiveScene().name == SceneName.FTUE_Fighting.ToString())
            return;
        ctaTextGO.SetActive(false);
        if (!ftueActiveSkillIngame.value)
        {
            if (activeSkillButton.skillCaster != null)
                StartIngameTutorial();
            else
                activeSkillButton.onInitializeCompleted += StartIngameTutorial;
        }
    }

    private void StartIngameTutorial()
    {
        StartCoroutine(StartIngameTutorial_CR());
    }

    private IEnumerator StartIngameTutorial_CR()
    {
        #region FTUE Event
        GameEventHandler.Invoke(LogFTUEEventCode.StartActiveSkillInGame);
        #endregion

        while (activeSkillButton.skillCaster.totalSkillCastCount <= 0)
        {
            if (activeSkillButton.skillCaster.IsAbleToPerformSkill())
            {
                ctaTextGO.SetActive(true);
            }
            else
            {
                ctaTextGO.SetActive(false);
            }
            yield return null;
        }

        #region FTUE Event
        GameEventHandler.Invoke(LogFTUEEventCode.EndActiveSkillInGame);
        #endregion

        ftueActiveSkillIngame.value = true;
        ctaTextGO.SetActive(false);
    }
}