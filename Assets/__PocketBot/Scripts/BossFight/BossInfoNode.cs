using System;
using System.Collections;
using System.Collections.Generic;
using GachaSystem.Core;
using HyrphusQ.Events;
using I2.Loc;
using LatteGames;
using LatteGames.GameManagement;
using LatteGames.Monetization;
using LatteGames.PvP;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class BossInfoNode : MonoBehaviour
{
    public static bool IS_CHALLENGEABLE = true;
    static float lowestTrophyAmountToUnlock = Mathf.Infinity;

    [SerializeField] private RVButtonBehavior revengeBtn;
    [SerializeField] TMP_Text overallScoreTxt, secondOverallScoreTxt, bossNameTxt, stateText;
    [SerializeField] LocalizationParamsManager lockedTrophyTxt;
    [SerializeField] string trophySpriteString;
    [SerializeField] Image avatarImg;
    [SerializeField] Button fightBtn, claimBtn, revengeNotAdsBtn;
    [SerializeField] BossInfoRewardSlotUI slot1, slot2;
    [SerializeField] GameObject claimedGroup, canFightBGImg, canNotFightBGImg, lockedGroup, m_RPSBarImg, unlockedProgressBar, lockedPreviousBossTxt;
    [SerializeField] GraphicColorController graphicColorController;
    [SerializeField] RPSCalculatorSO m_RPSCalculatorSO;
    [SerializeField] RectTransform m_Arrow;
    [SerializeField] AnimationCurve m_CurveX;
    [SerializeField] PBRobotStatsSO bossPBRobotStatsSO, playerPBRobotStatsSO;
    [SerializeField] SlicedFilledImage coreProgressImg;
    [SerializeField] private FloatVariable m_HighestAchievedMedalsVariable;

    BossSO bossSO;

    private int currentChapterIndex => BossFightManager.Instance.selectingChapterIndex;
    private BossChapterSO currentChapterSO => BossFightManager.Instance.bossMapSO.chapterList[currentChapterIndex];
    List<BossSO> currentBossList => currentChapterSO.bossList;
    int currentBossIndex => currentChapterSO.bossIndex.value;

    [SerializeField, TitleGroup("FTUE")] private GameObject _handFTUE;
    [SerializeField, TitleGroup("FTUE")] private PPrefBoolVariable _pPrefBoolFTUEBattleBossFightingUI;

    public BossSO BossSO => bossSO;

    void Start()
    {
        GameEventHandler.AddActionEvent(BossFightEventCode.OnBossMapOpened, HandleOpened);
        fightBtn.onClick.AddListener(OnPlayButtonClicked);
        claimBtn.onClick.AddListener(OnClaimButtonClicked);
        revengeBtn.OnRewardGranted += OnRewardGrantedRevenge;
    }

    void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(BossFightEventCode.OnBossMapOpened, HandleOpened);
        fightBtn.onClick.RemoveListener(OnPlayButtonClicked);
        claimBtn.onClick.RemoveListener(OnClaimButtonClicked);
        revengeBtn.OnRewardGranted -= OnRewardGrantedRevenge;
        lowestTrophyAmountToUnlock = Mathf.Infinity;
    }

    void HandleOpened()
    {
        if (!_pPrefBoolFTUEBattleBossFightingUI)
        {
            var bossIndex = currentBossList.IndexOf(bossSO);
            if (bossIndex == currentBossIndex)
            {
                _handFTUE.SetActive(true);
                GameEventHandler.Invoke(BlockBackGround.LockDarkHasObject, this.gameObject);
            }
        }
    }

    void OnPlayButtonClicked()
    {
        GameEventHandler.Invoke(LogFTUEEventCode.EndPlayBossFight);
        BossFightManager.Instance.OnSelectCurrentBoss();
        BossFightManager.Instance.currentChosenModeVariable.value = Mode.Boss;

        var activeScene = SceneManager.GetActiveScene();
        var mainScreenUI = activeScene.GetRootGameObjects().FirstOrDefault(go => go.name == "MainScreenUI");
        foreach (var gameObject in activeScene.GetRootGameObjects())
        {
            if (gameObject == mainScreenUI)
                continue;
            gameObject.SetActive(false);
        }
        SceneManager.LoadScene(SceneName.PvP_BossFight, UnityEngine.SceneManagement.LoadSceneMode.Additive, callback: OnLoadSceneCompleted);

        void OnLoadSceneCompleted(SceneManager.LoadSceneResponse loadSceneResponse)
        {
            mainScreenUI?.SetActive(false);
            var previousBackgroundLoadingPriority = Application.backgroundLoadingPriority;
            Application.backgroundLoadingPriority = ThreadPriority.Low;
            SceneManager.UnloadSceneAsync(activeScene.name).onCompleted += () =>
            {
                Application.backgroundLoadingPriority = previousBackgroundLoadingPriority;
            };
        }

        if (!_pPrefBoolFTUEBattleBossFightingUI)
        {
            var bossIndex = currentBossList.IndexOf(bossSO);
            if (bossIndex == currentBossIndex)
            {
                _pPrefBoolFTUEBattleBossFightingUI.value = true;
            }
        }
        bossSO.IsFirstTimeFighting = true;

        GameEventHandler.Invoke(BossEventCode.BossFightStreak, currentChapterSO);
    }

    void OnClaimButtonClicked()
    {
        TryClaim();
        UpdateView();
    }

    private void OnRewardGrantedRevenge(RVButtonBehavior.RewardGrantedEventData data)
    {
        StartCoroutine(CommonCoroutine.Delay(0, false, () =>
        {
            OnPlayButtonClicked();

            #region MonetizationEventCode
            string bossName = bossSO.chassisSO.GetModule<NameItemModule>().displayName;
            string adsLocation = "BossUI";
            string bossID = $"{currentBossIndex + 1}";
            GameEventHandler.Invoke(MonetizationEventCode.TrophyBypass_BossUI, bossName, adsLocation, bossID);
            #endregion
        }));
    }

    public void TryClaim()
    {
        var cards = GetCards();
        if (bossSO.IsClaimed) return;
        foreach (var card in cards)
        {
            if (card is GachaCard_Currency gachaCard_Currency)
            {
                gachaCard_Currency.ResourceLocationProvider = new ResourceLocationProvider(ResourceLocation.BossFight, $"Boss{currentBossIndex}");
            }
        }
        List<GachaPack> gachaPacks = new List<GachaPack>();
        foreach (var item in bossSO.bossMapRewardGroupInfo.generalItems)
        {
            if (item.Key is GachaPack)
            {
                gachaPacks.Add((GachaPack)item.Key);
            }
        }

        GameEventHandler.Invoke(UnpackEventCode.OnUnpackStart, cards, gachaPacks, null, true);
        GameEventHandler.Invoke(BossFightEventCode.OnClaimReward);
        bossSO.IsClaimed = true;

        #region DesignEvent
        string openStatus = "NoTimer";
        string location = "BossUI";
        GameEventHandler.Invoke(DesignEvent.OpenBox, openStatus, location);
        #endregion

        #region Firebase Event
        for (int i = 0; i < gachaPacks.Count; i++)
        {
            if (gachaPacks[i] != null)
            {
                GameEventHandler.Invoke(LogFirebaseEventCode.BoxOpen, gachaPacks[i], "free");
            }

        }

        #endregion
    }

    public List<GachaCard> GetCards()
    {
        if (bossSO == null)
        {
            return null;
        }
        return (GachaCardGenerator.Instance as PBGachaCardGenerator).Generate(bossSO.bossMapRewardGroupInfo);
    }

    public void Setup(BossSO bossSO)
    {
        this.bossSO = bossSO;
        avatarImg.sprite = bossSO.botInfo.avatar;
        bossNameTxt.text = bossSO.botInfo.name;

        SetupRewardSlots();

        //Calculate overall score
        overallScoreTxt.text = bossSO.chassisSO.GetTotalStatsScore().ToRoundedText();
        secondOverallScoreTxt.text = bossSO.chassisSO.GetTotalStatsScore().ToRoundedText();
        lockedTrophyTxt.SetParameterValue("Value", $"{this.bossSO.unlockedTrophyAmount}{trophySpriteString}");
        UpdateView();
    }

    void UpdateView()
    {
        var hasUnlocked = m_HighestAchievedMedalsVariable.value >= bossSO.unlockedTrophyAmount;
        var bossIndex = currentBossList.IndexOf(bossSO);

        #region Design Event
        if (hasUnlocked)
        {
            BossChapterSO bossChapterSO = BossFightManager.Instance.bossMapSO.GetBossChapterDefault;
            int bossID = bossChapterSO.bossList.FindIndex(v => v == bossSO) + 1;
            string keyCallTheFirtstTime = $"DesignEvent-BossFightUnlock-BossID{bossID}";
            if (!PlayerPrefs.HasKey(keyCallTheFirtstTime))
            {
                GameEventHandler.Invoke(DesignEvent.BossFightUnlock, bossID);
                PlayerPrefs.SetInt(keyCallTheFirtstTime, 1);
            }
        }
        #endregion

        if (bossIndex > currentBossIndex || (!IS_CHALLENGEABLE && !hasUnlocked))
        {
            //v1.5.1: Disable revenge Boss
            // revengeNotAdsBtn.gameObject.SetActive(false);

            // revengeBtn.gameObject.SetActive(false);
            DisableAllButtons();
            unlockedProgressBar.SetActive(!hasUnlocked);
            lockedPreviousBossTxt.SetActive(hasUnlocked);
            lockedTrophyTxt.gameObject.SetActive(!hasUnlocked);
            claimedGroup.gameObject.SetActive(false);
            canFightBGImg.SetActive(false);
            canNotFightBGImg.SetActive(true);
            lockedGroup.SetActive(true);
            m_RPSBarImg.SetActive(false);
            graphicColorController.SetDarkCover(false);
            graphicColorController.SetGrayscale(true);
            slot1.SetClaimed(false);
            slot2.SetClaimed(false);
            if (!hasUnlocked && bossSO.unlockedTrophyAmount < lowestTrophyAmountToUnlock)
            {
                lowestTrophyAmountToUnlock = bossSO.unlockedTrophyAmount;
                float previousBossUnlockedTrophyAmount = 0f;
                if (bossIndex > 0)
                {
                    previousBossUnlockedTrophyAmount = currentBossList[bossIndex - 1].unlockedTrophyAmount;
                }
                coreProgressImg.gameObject.SetActive(true);
                coreProgressImg.fillAmount = (m_HighestAchievedMedalsVariable.value - previousBossUnlockedTrophyAmount) / (bossSO.unlockedTrophyAmount - previousBossUnlockedTrophyAmount);
            }
            else
            {
                coreProgressImg.gameObject.SetActive(false);
            }
        }
        else if (bossIndex == currentBossIndex)
        {
            //v1.5.1: Disable revenge Boss
            // revengeNotAdsBtn.gameObject.SetActive(currentBossIndex <= 0 && currentChapterIndex <= 0 ? false : bossSO.IsFirstTimeFighting);
            // revengeBtn.gameObject.SetActive(false);
            //revengeBtn.gameObject.SetActive(currentBossIndex <= 0 && currentChapterIndex <= 0 ? false : bossSO.IsFirstTimeFighting);
            // fightBtn.gameObject.SetActive(currentBossIndex <= 0 && currentChapterIndex <= 0 ? true : !bossSO.IsFirstTimeFighting);

            claimedGroup.gameObject.SetActive(false);
            canFightBGImg.SetActive(true);
            canNotFightBGImg.SetActive(false);
            lockedGroup.SetActive(false);
            m_RPSBarImg.SetActive(true);
            graphicColorController.SetGrayscale(false);
            graphicColorController.SetDarkCover(false);
            slot1.SetClaimed(false);
            slot2.SetClaimed(false);
            var rpsData = m_RPSCalculatorSO.CalcRPSValue(playerPBRobotStatsSO, bossPBRobotStatsSO);
            stateText.text = rpsData.stateLabel;
            m_Arrow.anchoredPosition = new Vector2(m_CurveX.Evaluate(rpsData.rpsInverseLerp), m_Arrow.anchoredPosition.y);
            if (!hasUnlocked)
            {
                unlockedProgressBar.SetActive(true);
                EnableAButton(revengeBtn.gameObject);
                float previousBossUnlockedTrophyAmount = 0f;
                if (bossIndex > 0)
                {
                    previousBossUnlockedTrophyAmount = currentBossList[bossIndex - 1].unlockedTrophyAmount;
                }
                coreProgressImg.gameObject.SetActive(true);
                coreProgressImg.fillAmount = (m_HighestAchievedMedalsVariable.value - previousBossUnlockedTrophyAmount) / (bossSO.unlockedTrophyAmount - previousBossUnlockedTrophyAmount);
            }
            else
            {
                unlockedProgressBar.SetActive(false);
                EnableAButton(fightBtn.gameObject);
            }
        }
        else
        {
            EnableAButton(bossSO.IsClaimed ? claimedGroup.gameObject : claimBtn.gameObject);
            canFightBGImg.SetActive(false);
            canNotFightBGImg.SetActive(true);
            unlockedProgressBar.SetActive(false);
            lockedGroup.SetActive(false);
            m_RPSBarImg.SetActive(false);
            graphicColorController.SetGrayscale(false);
            graphicColorController.SetDarkCover(bossSO.IsClaimed, 0.4f);
            slot1.SetClaimed(bossSO.IsClaimed);
            slot2.SetClaimed(bossSO.IsClaimed);
        }

        //v1.5.1: Disable revenge Boss
        revengeNotAdsBtn.gameObject.SetActive(false);

        void EnableAButton(GameObject btn)
        {
            DisableAllButtons();
            btn.SetActive(true);
        }

        void DisableAllButtons()
        {
            claimBtn.gameObject.SetActive(false);
            fightBtn.gameObject.SetActive(false);
            revengeBtn.gameObject.SetActive(false);
            claimedGroup.gameObject.SetActive(false);
        }
    }

    void SetupRewardSlots()
    {
        int maxSlotAmount = 2;
        int slotCount = 0;
        foreach (var item in bossSO.bossMapRewardGroupInfo.generalItems)
        {
            var slot = slotCount == 0 ? slot1 : slot2;
            slot.Setup(item.Key.GetThumbnailImage(), item.Value.value, false);
            slotCount++;
            if (slotCount >= maxSlotAmount)
            {
                return;
            }
        }
        foreach (var item in bossSO.bossMapRewardGroupInfo.currencyItems)
        {
            var slot = slotCount == 0 ? slot1 : slot2;
            var currencySO = CurrencyManager.Instance.GetCurrencySO(item.Key);
            slot.Setup(currencySO.icon, item.Value.value, true);
            slotCount++;
            if (slotCount >= maxSlotAmount)
            {
                return;
            }
        }
    }
}