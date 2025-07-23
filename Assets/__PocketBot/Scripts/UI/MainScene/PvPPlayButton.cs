using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using LatteGames.GameManagement;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PvPPlayButton : MonoBehaviour
{
    [SerializeField]
    protected Button m_PlayButton;
    [SerializeField]
    protected ModeVariable m_CurrentChosenModeVariable;
    [SerializeField]
    protected Image imageMode;

    protected virtual Mode mode => Mode.Normal;

    protected virtual void Start()
    {
        m_PlayButton.onClick.AddListener(OnPlayButtonClicked);
    }

    protected virtual void OnDestroy()
    {
        m_PlayButton.onClick.RemoveListener(OnPlayButtonClicked);
    }

    protected virtual void OnPlayButtonClicked()
    {
        if (!FTUEMainScene.Instance.FTUE_Equip.value)
        {
            return;
        }

        if (!FTUEMainScene.Instance.FTUEUpgrade.value)
        {
            return;
        }
        //m_CurrentChosenModeVariable.value = mode;
        //SceneManager.LoadScene(SceneName.PvP);
    }
}