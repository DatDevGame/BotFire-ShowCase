using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HyrphusQ.Events;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class FTUEMainButton : MonoBehaviour
{
    [SerializeField] GameObject ftueHand;
    [SerializeField] PPrefBoolVariable FTUE_PVP;
    [SerializeField] PPrefBoolVariable FTUE_Upgrade;
    [SerializeField] DockerController dockerController;
    [SerializeField] MultiImageButton characterButton;
    [SerializeField] MultiImageButton mainButton;
    [SerializeField] MultiImageButton shopButton;

    [SerializeField, BoxGroup("PSFTUE")] private GameObject m_FTUEHandle_1;
    [SerializeField, BoxGroup("PSFTUE")] private PSFTUESO m_PSFTUESO;

    private void Awake()
    {
        GameEventHandler.AddActionEvent(FTUEEventCode.OnFinishUpgradeFTUE, HandleOnFinishUpgrade);
        GameEventHandler.AddActionEvent(MainSceneEventCode.OnClickButtonMain, HandleOnClick);
        FTUEPreludeSeason.ShowDoMissionFTUE += ShowDoMissionFTUE;
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(FTUEEventCode.OnFinishUpgradeFTUE, HandleOnFinishUpgrade);
        GameEventHandler.RemoveActionEvent(MainSceneEventCode.OnClickButtonMain, HandleOnClick);
        FTUEPreludeSeason.ShowDoMissionFTUE -= ShowDoMissionFTUE;
    }

    private void Start()
    {
        if (!FTUEMainScene.Instance.FTUEUpgrade.value)
        {
            mainButton.enabled = false;
            shopButton.enabled = false;
            return;
        }
        if (!FTUEMainScene.Instance.FTUE_Equip.value)
        {
            mainButton.enabled = false;
            shopButton.enabled = false;
            return;
        }
        if (!FTUE_PVP.value)
        {
            if (dockerController.SelectedDockerButton == this.GetComponent<IDockerButton>())
                return;
            characterButton.enabled = false;
            ftueHand.SetActive(true);
            return;
        }
        mainButton.enabled = true;
        shopButton.enabled = true;
        if (FTUE_Upgrade.value && !FTUE_PVP.value)
        {
            if (dockerController.SelectedDockerButton == this.GetComponent<IDockerButton>())
                return;
            ftueHand.SetActive(true);
        }
    }

    void HandleOnFinishUpgrade()
    {
        if (!FTUE_PVP.value)
        {
            mainButton.enabled = true;
            characterButton.enabled = false;
            ftueHand.SetActive(true);
        }
    }

    void HandleOnClick()
    {
        ftueHand.SetActive(false);

        if (m_PSFTUESO.FTUEOpenInfoPopup.value && !m_PSFTUESO.FTUEStart2ndMatch.value)
        {
            m_FTUEHandle_1.SetActive(false);
            GameEventHandler.Invoke(PSFTUEBlockAction.Unblock, this.gameObject);
        }
    }

    void ShowDoMissionFTUE(GameObject obj)
    {
        #region FTUE Event
        GameEventHandler.Invoke(LogFTUEEventCode.StartPreludeSeason_DoMission);
        #endregion

        ftueHand.SetActive(true);
        GameEventHandler.Invoke(FTUEEventCode.OnPreludeDoMission_FTUE, mainButton.gameObject, obj);
        GameEventHandler.AddActionEvent(MainSceneEventCode.OnClickButtonMain, HandleOnClickDoMissionFTUE);

        var canvas = obj.GetOrAddComponent<Canvas>();
        var raycaster = obj.GetOrAddComponent<GraphicRaycaster>();
        canvas.overrideSorting = true;
        canvas.sortingOrder = 9000;

        void HandleOnClickDoMissionFTUE()
        {
            #region FTUE Event
            GameEventHandler.Invoke(LogFTUEEventCode.EndPreludeSeason_DoMission);
            #endregion

            GameEventHandler.Invoke(FTUEEventCode.OnPreludeDoMission_FTUE);
            GameEventHandler.RemoveActionEvent(MainSceneEventCode.OnClickButtonMain, HandleOnClickDoMissionFTUE);
            DestroyImmediate(obj.GetComponent<GraphicRaycaster>());
            DestroyImmediate(obj.GetComponent<Canvas>());
        }
    }

    public void FTUE2ndMath()
    {
        if (m_PSFTUESO.FTUEOpenInfoPopup.value && !m_PSFTUESO.FTUEStart2ndMatch.value)
        {
            m_FTUEHandle_1.SetActive(true);
            GameEventHandler.Invoke(PSFTUEBlockAction.Block, this.gameObject, PSFTUEBubbleText.None);
        }
    }
}
