using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using HyrphusQ.Events;
using I2.Loc;
using LatteGames;
using LatteGames.Monetization;
using LatteGames.PvP;
using LatteGames.Template;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using HightLightDebug;
using GachaSystem.Core;
using System;

public class PBPvPEarnBoxProgressUI : MonoBehaviour
{
    [HideInInspector] public Action OnEndUnbox = delegate { };
    [HideInInspector] public UnityEvent OnStartAnimBox = new UnityEvent();
    [HideInInspector] public UnityEvent OnEndAnimBox = new UnityEvent();

    [SerializeField, BoxGroup("Ref")] private Image boxReward;
    [SerializeField, BoxGroup("Ref")] private Color boxRewardColorWhenFullSlots;
    [SerializeField, BoxGroup("Ref")] private GameObject boxRewardHolder;
    [SerializeField, BoxGroup("Ref")] private GameObject progressBarHolder;
    [SerializeField, BoxGroup("Ref")] private EZAnimVector2 ezAnimVector2OpenBtn;
    [SerializeField, BoxGroup("Ref")] private EZAnimBase ezAnimShowGachaBox;
    [SerializeField, BoxGroup("Ref")] private PackDockOpenByGemButton openNowBtn;
    [SerializeField, BoxGroup("Ref")] private RVButtonBehavior openNowRVBtn;
    [SerializeField, BoxGroup("Ref")] private Button openNowIAPBtn;
    [SerializeField, BoxGroup("Ref")] private MultiImageButton replaceBtn;
    [SerializeField, BoxGroup("Ref")] private TMP_Text titleBoxText;
    [SerializeField, BoxGroup("Ref")] private GameObject noAvailableSlotsPanel;
    [SerializeField, BoxGroup("Ref")] private CanvasGroupVisibility footerGroupCanvasGroup;
    [SerializeField, BoxGroup("Ref")] private List<Image> progressNodes;
    [SerializeField, BoxGroup("Ref")] private PBPvPGameOverPackInfoUI gameOverPackInfoUI;
    [SerializeField, BoxGroup("Data")] private PPrefIntVariable cumulatedWinMatch;
    [SerializeField, BoxGroup("Data")] private LocalizationParamsManager getBoxTitleParamsManager;

    private int currentNodeAmount;
    [ShowInInspector] private GachaPack gachaPackCurrent;
    private bool isBattleMode;
    private bool isFTUE;

    private void Awake()
    {
        GameEventHandler.AddActionEvent(GachaPackDockEventCode.OnGachaPackDockUpdated, OnGachaPackDockUpdated);
        openNowRVBtn.OnRewardGranted += OnRewardGranted;
        openNowBtn.OnEnoughGemClicked.AddListener(OnEnoughGemClicked);
        openNowBtn.OnNotEnoughGemClicked.AddListener(OnNotEnoughGemClicked);
        replaceBtn.onClick.AddListener(OnReplaceBoxSlot);
        openNowIAPBtn.onClick.AddListener(OnOpenNowIAPClicked);
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(GachaPackDockEventCode.OnGachaPackDockUpdated, OnGachaPackDockUpdated);
        openNowRVBtn.OnRewardGranted -= OnRewardGranted;
        openNowBtn.OnEnoughGemClicked.RemoveListener(OnEnoughGemClicked);
        openNowBtn.OnNotEnoughGemClicked.RemoveListener(OnNotEnoughGemClicked);
        replaceBtn.onClick.RemoveListener(OnReplaceBoxSlot);
        openNowIAPBtn.onClick.RemoveListener(OnOpenNowIAPClicked);
    }

    private void OnEnoughGemClicked()
    {
        string location = gameOverPackInfoUI.IsPopup ? "BoxPopup" : "GameOverUI";
        GameEventHandler.Invoke(GachaPackDockEventCode.OnOpenPackNow);
        GameEventHandler.Invoke(GachaPackDockEventCode.OnOpenNowByGem, location);

        OnEndUnbox?.Invoke();

        #region Firebase Event
        if (gachaPackCurrent != null)
        {
            string openType = "gems";
            GameEventHandler.Invoke(LogFirebaseEventCode.BoxOpen, gachaPackCurrent, openType);
        }
        #endregion
    }

    private void OnNotEnoughGemClicked()
    {
        IAPGemPackPopup.Instance?.Show();
    }

    private void OnOpenNowIAPClicked()
    {
        #region Firebase Event
        if (gachaPackCurrent != null)
        {
            string openType = "RV";
            GameEventHandler.Invoke(LogFirebaseEventCode.BoxOpen, gachaPackCurrent, openType);
        }
        #endregion

        #region Design Event
        string openStatus = "OpenNowOffer";
        string locationDesignEvent = "Standard";
        GameEventHandler.Invoke(DesignEvent.OpenBox, openStatus, locationDesignEvent);
        #endregion

        OnEndUnbox?.Invoke();
        openNowBtn.OpenNow();
        string location = gameOverPackInfoUI.IsPopup ? "BoxPopup" : "GameOverUI";
        GameEventHandler.Invoke(GachaPackDockEventCode.OnOpenPackNow);
    }

    private void OnRewardGranted(RVButtonBehavior.RewardGrantedEventData data)
    {
        #region MonetizationEventCode
        string adsLocation = "GameOverUI";
        string input = $"{gachaPackCurrent.GetModule<NameItemModule>().displayName}";
        string typeBox = input.Split(' ')[0];
        GameEventHandler.Invoke(MonetizationEventCode.OpenNowBox_GameOverUI, adsLocation, typeBox);
        #endregion

        #region Firebase Event
        if (gachaPackCurrent != null)
        {
            string openType = "RV";
            GameEventHandler.Invoke(LogFirebaseEventCode.BoxOpen, gachaPackCurrent, openType);
        }
        #endregion

        OnEndUnbox?.Invoke();
        string location = gameOverPackInfoUI.IsPopup ? "BoxPopup" : "GameOverUI";
        openNowBtn.OpenNow();
        GameEventHandler.Invoke(GachaPackDockEventCode.OnOpenPackNow);
        GameEventHandler.Invoke(GachaPackDockEventCode.OnOpenNowByRV, location);
    }

    public void Setup(int activeNodeAmount, GachaPack gachaPack, bool isBattleMode, bool isFTUE = false)
    {
        this.isBattleMode = isBattleMode;
        this.isFTUE = isFTUE;
        this.gachaPackCurrent = gachaPack;
        openNowBtn.Setup(gachaPack);

        currentNodeAmount = activeNodeAmount;
        for (var i = 0; i < progressNodes.Count; i++)
        {
            if (i < activeNodeAmount)
            {
                progressNodes[i].transform.localScale = Vector3.one;
            }
            else
            {
                progressNodes[i].transform.localScale = Vector3.zero;
            }
        }

        Sprite sprite = gachaPackCurrent.GetOriginalPackThumbnail();
        boxReward.sprite = sprite;
        boxRewardHolder.SetActive(false);
        noAvailableSlotsPanel.SetActive(false);
        progressBarHolder.SetActive(!isBattleMode);
        footerGroupCanvasGroup.HideImmediately();

        if (isFTUE)
        {
            boxRewardHolder.SetActive(true);
            PlayShowingBoxAnim();
            getBoxTitleParamsManager.SetParameterValue("BoxName", $"Classic Box");
        }

    }

    public void PlayIncreaseAnimation(int targetActiveNodeAmount)
    {
        StartCoroutine(CR_PlayIncreaseAnimation(targetActiveNodeAmount));
    }

    public void PlayShowingBoxAnim()
    {
        OnStartAnimBox.Invoke();

        cumulatedWinMatch.value = 0;
        boxRewardHolder.SetActive(true);

        getBoxTitleParamsManager.SetParameterValue("BoxName", gachaPackCurrent.GetOriginalPackName().Split(" - ")[0]);
        ezAnimShowGachaBox.Play(() =>
        {
            OnEndAnimBox.Invoke();
        });
    }

    public void ShowGachaPack(bool isFullSlot)
    {
        noAvailableSlotsPanel.SetActive(isFullSlot);
        if (isFullSlot)
        {
            footerGroupCanvasGroup.Show();
            ezAnimVector2OpenBtn.Play();
            var isApplyingOpenNow = OpenNowOfferManager.Instance.IsApplyingOpenNow;
            if (isApplyingOpenNow)
            {
                openNowIAPBtn.gameObject.SetActive(true);
                openNowBtn.gameObject.SetActive(false);
                openNowRVBtn.gameObject.SetActive(false);
            }
            else
            {
                openNowIAPBtn.gameObject.SetActive(false);
                if (gachaPackCurrent.UnlockedDuration <= GachaPackDockConfigs.UNLOCK_TIME_PER_RV)
                {
                    openNowBtn.gameObject.SetActive(false);
                    openNowRVBtn.gameObject.SetActive(true);

                    #region Design Events
                    string input = $"{gachaPackCurrent.GetDisplayName()}";
                    string typeBox = input.Split(' ')[0];
                    string boxType = typeBox;
                    string rvName = $"OpenNowBox_{boxType}";
                    string location = "GameOverUI";
                    GameEventHandler.Invoke(DesignEvent.RVShow);
                    #endregion
                }
                else
                {
                    openNowBtn.gameObject.SetActive(true);
                    openNowRVBtn.gameObject.SetActive(false);
                }
            }

            boxReward.color = boxRewardColorWhenFullSlots;
            gameOverPackInfoUI.Show(gachaPackCurrent, openNowBtn.GetComponent<Button>(), openNowRVBtn.GetComponent<Button>(), openNowIAPBtn);
        }
        else
        {
            boxReward.color = Color.white;
        }
    }

    IEnumerator CR_PlayIncreaseAnimation(int targetActiveNodeAmount)
    {
        for (int i = currentNodeAmount; i < targetActiveNodeAmount; i++)
        {
            var item = progressNodes[i];
            item.transform.DOScale(Vector3.one, AnimationDuration.TINY);
            yield return new WaitForSeconds(0.2f);
        }
        if (targetActiveNodeAmount >= progressNodes.Count)
        {
            PlayShowingBoxAnim();
        }
    }

    private void OnReplaceBoxSlot()
    {
        if (PackDockManager.Instance.IsFull)
        {
            GameEventHandler.Invoke(GachaPackDockEventCode.OnShowReplaceUI, gachaPackCurrent);
        }
    }

    private void OnGachaPackDockUpdated()
    {
        footerGroupCanvasGroup.Hide();
    }
}
