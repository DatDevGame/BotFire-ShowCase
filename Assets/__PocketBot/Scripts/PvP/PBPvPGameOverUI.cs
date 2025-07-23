using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using DG.Tweening;
using GachaSystem.Core;
using HyrphusQ.Events;
using LatteGames;
using LatteGames.Monetization;
using LatteGames.PvP;
using LatteGames.Template;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PBPvPGameOverUI : MonoBehaviour, IResourceLocationProvider
{
    public static float BATTLE_DELAY_SHOW_GAME_OVER_UI = 0.5f;
    [SerializeField]
    float m_DelayTimeWhenTimesUp = 3f;
    [SerializeField]
    float m_DelayTimeToShowUI = 1f;
    [SerializeField]
    Sprite m_WinBannerSprite;
    [SerializeField]
    Sprite m_LoseBannerSprite;
    [SerializeField]
    Image m_BannerImage;
    [SerializeField]
    Material m_Top1Material;
    [SerializeField]
    Material m_NotTop1Material;
    [SerializeField]
    TextMeshProUGUI m_BannerTitleText;
    [SerializeField]
    TextMeshProUGUI m_OutlineBannerTitleText;
    [SerializeField]
    GameObject m_WinBoard;
    [SerializeField]
    GameObject m_LoseBoard;
    [SerializeField]
    TextMeshProUGUI m_ContinueButtonText;
    [SerializeField]
    Button m_ContinueButton;
    [SerializeField]
    PPrefIntVariable cumulatedWinMatch;
    [SerializeField]
    IntVariable numberOfWinToGrantReward;
    [SerializeField]
    PBPvPEarnBoxProgressUI earnBoxProgressUI;
    [SerializeField]
    PBMultiplyRewardArc multiplyRewardArc;
    [SerializeField]
    RVButtonBehavior multiplyRVButton;
    [SerializeField]
    GameObject m_BattleEntryFeeGetBackGroup;
    [SerializeField]
    RVButtonBehavior m_BattleEntryFeeGetBackButton;
    [SerializeField]
    TMP_Text m_BattleEntryFeeTMP;
    [SerializeField]
    Button m_LoseBattleEntryFeeButton;

    [Header("Text Mesh Pro")]
    [SerializeField]
    List<TMP_Text> multiplyRewardTxtList;
    [SerializeField]
    EZAnimTextCount m_WinPrizeText;
    [SerializeField]
    EZAnimTextCount m_WinMedalText;
    [SerializeField]
    EZAnimTextCount m_LoseMedalText;
    [SerializeField]
    EZAnimTextCount m_LosePrizeText;
    [SerializeField]
    EZAnimSequence showUIAnimSequence;
    [SerializeField]
    EZAnimBase showContinueBtnAnim;
    [SerializeField]
    EZAnimSequence multiplyRewardAnimSequence;
    [SerializeField]
    Color m_WinTrophyRewardTextColor;
    [SerializeField]
    Color m_LoseTrophyRewardTextColor;
    [SerializeField]
    TextMeshProUGUI m_TrophyRewardText;

    [SerializeField]
    Color multiplierTextColorHard;
    [SerializeField]
    Color multiplierTextColorMedium;
    [SerializeField]
    Color multiplierTextColorEasy;

    [SerializeField, BoxGroup("Text Mesh Pro")] private EZAnimTextCount m_WinMedalMinus;
    [SerializeField, BoxGroup("Text Mesh Pro")] private EZAnimTextCount m_LosePrizePlus;
    [SerializeField, BoxGroup("Text Mesh Pro")] private EZAnimTextCount m_LoseMedalPlus;
    [SerializeField, BoxGroup("Ref")] private EZAnimSequence m_HeadeEZAnimSequence;
    [SerializeField, BoxGroup("Ref")] private CanvasGroupVisibility m_BackGroundCanvasGroupVisibility;
    [SerializeField, BoxGroup("Ref")] private CanvasGroupVisibility m_MainCanvasGroupVisibility;
    [SerializeField, BoxGroup("Ref")] private CanvasGroupVisibility m_CanvasGroupVisibilityMultiply;
    [SerializeField, BoxGroup("Ref")] private CanvasGroupVisibility m_CanvasGroupVisibilitySkipAddGroup;
    [SerializeField, BoxGroup("Ref")] private TMP_Text m_MultiplierRewardsText;
    [SerializeField, BoxGroup("Ref")] private TMP_Text m_MultiplierText;
    [SerializeField, BoxGroup("Ref")] private GameObject m_RewardBoxWin;
    [SerializeField, BoxGroup("Ref")] private GameObject m_RewardBoxLose;
    [SerializeField, BoxGroup("Ref")] private RectTransform m_RewardGroupRect;
    [SerializeField, BoxGroup("Ref")] private EZAnim<Vector3> m_MultiplyEZ;
    [SerializeField, BoxGroup("Ref")] private EZAnim<Vector3> titleYourNextBox;
    [SerializeField, BoxGroup("Ref")] private EZAnim<Vector3> titleYourGotABox;
    [SerializeField, BoxGroup("Ref")] private EZAnim<Vector3> boxUnlockEzAnim;
    [SerializeField, BoxGroup("Ref")] private GameObject progressBarBox;
    [SerializeField, BoxGroup("Ref")] private WinStreakBanner m_WinStreakBanner;

    [SerializeField, BoxGroup("Data")] private HighestAchievedPPrefFloatTracker m_TrophyHighestAchievedPPrefFloatTracker;
    [SerializeField, BoxGroup("Data")] private PPrefBoolVariable activeBoxSlotTheFirstTime;
    [SerializeField, BoxGroup("Data")] private MultiplierRewardsDataSO m_MultiplierRewardsDataSO;
    [SerializeField, BoxGroup("Data")] private GachaPack FTUE_GachaPack;
    [SerializeField, BoxGroup("Data")] private PBGachaPackManagerSO pBGachaPackManagerSO;
    [SerializeField, BoxGroup("Data")] private BattleBetArenaVariable m_BattleBetArenaVariable;
    [SerializeField, BoxGroup("Data")] private IntVariable _ultimatePackCumulatedWinMatchThisSection;
    [SerializeField, BoxGroup("Data")] private PVPGameOverConfigSO m_PVPGameOverConfigSO;

    [SerializeField, BoxGroup("Event Config")] private ResourceLocationProvider m_GetBackResourceProvider;

    [SerializeField, BoxGroup("Data Mode")]
    private ModeVariable _currentChosenModeVariable;
    [SerializeField, TitleGroup("WIN VFX")]
    private ParticleSystem _winVFX;

    private float m_moneyEndMatch = 0;
    bool m_IsVictory;
    bool m_IsFTUE;
    bool m_IsMultiplyMoney = false;
    bool m_IsShowMultiplyRewards = false;
    PvPArenaSO m_ArenaSO;
    Coroutine m_PlaySoundCoroutine;
    PBScriptedGachaPacks scriptedGachaPacks;
    IUIVisibilityController m_UIVisibilityController;
    private GachaPack m_CachedGachaPack;
    private RectTransform _rewardBoxWinTrs;

    CurrencyType rewardMoneyType => CurrencyType.Standard;
    CurrencyType rewardMedalType => CurrencyType.Medal;
    bool canGetBox => cumulatedWinMatch.value >= numberOfWinToGrantReward;
    // bool isEnoughTrophy => m_TrophyHighestAchievedPPrefFloatTracker.value >= m_EnoughTrophyShowingMultiply;

    private void Awake()
    {
        //TODO: Hide IAP & Popup
        //m_UIVisibilityController = GetComponent<IUIVisibilityController>();
        //m_BattleEntryFeeGetBackButton.OnRewardGranted += OnBattleEnterFeeGetBackRVRewardGranted;
        //m_LoseBattleEntryFeeButton.onClick.AddListener(OnLoseEntryFeeButtonClicked);
        //multiplyRVButton.OnRewardGranted += OnMultiplyRVRewardGranted;
        //multiplyRVButton.OnStartWatchAds += OnMultiplyRVStartWatchAds;
        //multiplyRewardArc.OnChangeMultiplier += OnChangeMultiplier;
        //m_ContinueButton.onClick.AddListener(OnContinueBtnClicked);
        //earnBoxProgressUI.OnStartAnimBox.AddListener(OnStartAnimBoxProgress);
        //earnBoxProgressUI.OnEndAnimBox.AddListener(OnEndAnimBoxProgress);
        //GameEventHandler.AddActionEvent(PBPvPEventCode.OnFinalRoundCompleted, OnFinalRoundCompleted);
        //earnBoxProgressUI.OnEndUnbox += EndOpenBoxNow;
        //m_BattleEntryFeeGetBackGroup.SetActive(false);

        //_rewardBoxWinTrs = m_RewardBoxWin.transform as RectTransform;
    }

    private void OnDestroy()
    {
        //TODO: Hide IAP & Popup
        //m_BattleEntryFeeGetBackButton.OnRewardGranted -= OnBattleEnterFeeGetBackRVRewardGranted;
        //m_LoseBattleEntryFeeButton.onClick.RemoveListener(OnLoseEntryFeeButtonClicked);
        //multiplyRVButton.OnRewardGranted -= OnMultiplyRVRewardGranted;
        //multiplyRVButton.OnStartWatchAds -= OnMultiplyRVStartWatchAds;
        //multiplyRewardArc.OnChangeMultiplier -= OnChangeMultiplier;
        //m_ContinueButton.onClick.RemoveListener(OnContinueBtnClicked);
        //earnBoxProgressUI.OnStartAnimBox.RemoveListener(OnStartAnimBoxProgress);
        //earnBoxProgressUI.OnEndAnimBox.RemoveListener(OnEndAnimBoxProgress);
        //earnBoxProgressUI.OnEndUnbox -= EndOpenBoxNow;
        //GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnFinalRoundCompleted, OnFinalRoundCompleted);
        //if (m_PlaySoundCoroutine != null)
        //{
        //    StopCoroutine(m_PlaySoundCoroutine);
        //}
    }

    private void OnMultiplyRVStartWatchAds()
    {
        multiplyRVButton.gameObject.SetActive(false);
        multiplyRewardArc.StopRun();
    }

    private void OnChangeMultiplier(float value)
    {
        Color colorMultiply = value switch
        {
            var multiplier when multiplier <= multiplyRewardArc.GetSegmentInfos[0].multiplier => multiplierTextColorEasy,
            var multiplier when multiplier <= multiplyRewardArc.GetSegmentInfos[1].multiplier => multiplierTextColorMedium,
            var multiplier when multiplier <= multiplyRewardArc.GetSegmentInfos[2].multiplier => multiplierTextColorHard,
            _ => multiplierTextColorHard
        };

        foreach (var multiplyRewardTxt in multiplyRewardTxtList)
        {
            multiplyRewardTxt.text = $"x{multiplyRewardArc.MultiplierResult}";
            multiplyRewardTxt.color = colorMultiply;
        }
        m_MultiplierRewardsText.gameObject.SetActive(true);
        m_MultiplierRewardsText.SetText($"x{value}");
        m_MultiplierText.SetText($"X{value} Rewards");
    }

    private void OnBattleEnterFeeGetBackRVRewardGranted(RVButtonBehavior.RewardGrantedEventData eventData)
    {
        if (m_ArenaSO?.TryGetEntryRequirement(out Requirement_Currency coinEntry, coinEntry => coinEntry.currencyType == CurrencyType.Standard) ?? false)
        {
            // TODO: add tracking event
            #region Monetization Event
            int chosenArena = m_BattleBetArenaVariable.value.index + 1;
            string location = "GameOverUI";
            GameEventHandler.Invoke(MonetizationEventCode.GetBack, chosenArena, location);
            #endregion

            float currentCoin = 0;

            if (m_GetBackResourceProvider.GetLocation() == ResourceLocation.None || m_GetBackResourceProvider.GetItemId() == "")
                Debug.LogWarning($"GetBack ResourceProvider Is Null");

            CurrencyManager.Instance.Acquire(CurrencyType.Standard, coinEntry.requiredAmountOfCurrency, m_GetBackResourceProvider.GetLocation(), m_GetBackResourceProvider.GetItemId());
            CurrencyManager.Instance.PlayAcquireAnimation(
                CurrencyType.Standard, coinEntry.requiredAmountOfCurrency,
                m_BattleEntryFeeGetBackButton.transform.position,
                m_LosePrizeText.transform.position,
                () =>
                {
                    m_BattleEntryFeeGetBackGroup.SetActive(false);
                    m_ContinueButton.gameObject.SetActive(true);
                },
                null,
                (coinAdd) =>
                {
                    m_LosePrizeText.gameObject.SetActive(false);
                    m_LosePrizePlus.gameObject.SetActive(true);
                    m_LosePrizePlus.From = currentCoin;
                    m_LosePrizePlus.To = currentCoin + coinAdd;
                    m_LosePrizePlus.Play();
                    currentCoin += coinAdd;
                }
            );
        }
    }

    private void OnLoseEntryFeeButtonClicked()
    {
        OnContinueBtnClicked();
    }

    private void OnMultiplyRVRewardGranted(RVButtonBehavior.RewardGrantedEventData eventData)
    {
        m_CanvasGroupVisibilityMultiply.Hide();
        ReduceRewradBoxHeight(0);
        m_CanvasGroupVisibilitySkipAddGroup.Hide();
        m_IsMultiplyMoney = true;

        m_WinPrizeText.From = m_WinPrizeText.To;
        m_WinPrizeText.To = Mathf.RoundToInt(m_WinPrizeText.To * multiplyRewardArc.MultiplierResult);

        m_LosePrizePlus.From = m_LosePrizePlus.To;
        m_LosePrizePlus.To = m_LosePrizePlus.To * multiplyRewardArc.MultiplierResult;
        m_LosePrizePlus.Play();

        Color colorMultiply = multiplyRewardArc.MultiplierResult switch
        {
            var multiplier when multiplier <= multiplyRewardArc.GetSegmentInfos[0].multiplier => multiplierTextColorEasy,
            var multiplier when multiplier <= multiplyRewardArc.GetSegmentInfos[1].multiplier => multiplierTextColorMedium,
            var multiplier when multiplier <= multiplyRewardArc.GetSegmentInfos[2].multiplier => multiplierTextColorHard,
            _ => multiplierTextColorHard
        };

        foreach (var multiplyRewardTxt in multiplyRewardTxtList)
        {
            multiplyRewardTxt.text = $"x{multiplyRewardArc.MultiplierResult}";
            multiplyRewardTxt.color = colorMultiply;
        }
        multiplyRVButton.gameObject.SetActive(false);
        multiplyRewardAnimSequence.Play();
        m_ContinueButton.GetComponent<LayoutElement>().flexibleWidth = 1f;
        m_ContinueButtonText.SetText(I2LHelper.TranslateTerm(I2LTerm.Text_Continue));
        StartCoroutine(CommonCoroutine.Delay(AnimationDuration.TINY, false,
            () => { SoundManager.Instance.PlaySFX(SFX.Multiplier); }));

        #region MonetizationEventCode
        string set = multiplyRewardArc.IsPromoted ? "Promoted" : "Normal";
        string location = _currentChosenModeVariable.value
        switch
        {
            Mode.Normal => "SinglePvP",
            Mode.Battle => "BattlePvP",
            _ => "None"
        };
        GameEventHandler.Invoke(MonetizationEventCode.MultiplierRewards, set, location, _currentChosenModeVariable.value);
        #endregion

        #region Resource Event
        string typeResource = multiplyRewardArc.IsPromoted ? "Promoted" : "Normal";

        float amountWin = m_moneyEndMatch;
        float totalAmount = Mathf.RoundToInt(amountWin * (m_IsMultiplyMoney ? multiplyRewardArc.MultiplierResult : 1));
        float amountMultiplier = totalAmount - amountWin;
        CurrencyManager.Instance.Acquire(rewardMoneyType, Mathf.RoundToInt(amountWin), _currentChosenModeVariable.value == Mode.Normal ? ResourceLocation.SinglePvP : ResourceLocation.BattlePvP, $"{Const.ResourceItemId.PvPVictory}{m_ArenaSO.index + 1}");
        CurrencyManager.Instance.Acquire(rewardMoneyType, Mathf.RoundToInt(amountMultiplier), ResourceLocation.RV, $"Multiplier{typeResource}");
        #endregion
    }

    private void OnContinueBtnClicked()
    {
        m_ContinueButton.interactable = false;
        multiplyRVButton.interactable = false;

        if (m_IsFTUE)
        {
            PackDockManager.Instance.TryToAddPack(FTUE_GachaPack);

            var amountOfMoneyReward = 0f;
            var moneyRewardModule = m_ArenaSO.GetReward<CurrencyRewardModule>(item => item.CurrencyType == rewardMoneyType);

            if (m_IsVictory)
            {
                amountOfMoneyReward = Mathf.RoundToInt(moneyRewardModule.Amount);
                CurrencyManager.Instance.Acquire(rewardMoneyType, amountOfMoneyReward, _currentChosenModeVariable.value == Mode.Normal ? ResourceLocation.SinglePvP : ResourceLocation.BattlePvP, $"{Const.ResourceItemId.PvPVictory}1");
            }
        }
        else
        {
            if (m_IsVictory)
            {
                if (!m_IsMultiplyMoney && m_IsShowMultiplyRewards)
                    m_MultiplierRewardsDataSO.promotedSetCountVar.value++;

                if (!m_IsMultiplyMoney)
                {
                    CurrencyManager.Instance.Acquire(rewardMoneyType, Mathf.RoundToInt(m_moneyEndMatch), _currentChosenModeVariable.value == Mode.Normal ? ResourceLocation.SinglePvP : ResourceLocation.BattlePvP, $"{Const.ResourceItemId.PvPVictory}{m_ArenaSO.index + 1}");
                }
            }
        }

        OnEarningAnimCompleted();

        void OnEarningAnimCompleted()
        {
            if (m_IsVictory && !m_IsFTUE)
            {
                ObjectFindCache<PBPvPStateGameController>.Get().LeaveGame();
            }

            var matchManager = ObjectFindCache<PBPvPMatchManager>.Get();
            if (matchManager != null)
            {
                var matchOfPlayer = matchManager.GetCurrentMatchOfPlayer();
                matchManager.EndMatch(matchOfPlayer);
            }

            m_UIVisibilityController.Hide();
            SoundManager.Instance.PlaySFX(GeneralSFX.UIExitButton);
        }
    }
    public void ShowHeader()
    {
        m_BannerImage.GetComponent<CanvasGroup>().alpha = 1;
        m_BannerImage.GetComponent<CanvasGroup>().interactable = true;
        m_BannerImage.GetComponent<CanvasGroup>().blocksRaycasts = true;
        m_MainCanvasGroupVisibility.Show();
        m_BackGroundCanvasGroupVisibility.Hide();
        showUIAnimSequence.SetToStart();
        m_HeadeEZAnimSequence.Play();

        m_BannerImage.sprite = m_WinBannerSprite;
        if (_winVFX != null)
            _winVFX.Play();
    }
    private void OnFinalRoundCompleted(object[] parameters)
    {
        if (parameters[0] is not PBPvPMatch matchOfPlayer)
            return;
        if (!matchOfPlayer.isAbleToComplete)
            return;
        var arenaSO = matchOfPlayer.arenaSO;
        var isVictory = matchOfPlayer.isVictory;
        var cameraShake = ObjectFindCache<CameraShake>.Get();
        if (cameraShake != null)
        {
            var robots = PBRobot.allFightingRobots;
            cameraShake.ShakeCameraIgnoreCondition(robots[0].ChassisInstance, robots[1].ChassisInstance, 5f, true);
        }

        if (isVictory)
            SoundManager.Instance.PlaySFX(PBSFX.UIWinGuitar);
        else
            SoundManager.Instance.PlaySFX(PBSFX.UILoseGuitar);

        CharacterRoomPVP characterRoomPVP = ObjectFindCache<CharacterRoomPVP>.Get();
        if (characterRoomPVP != null)
            characterRoomPVP.OnFinalMatchViewCharacter += ShowHeader;

        if (matchOfPlayer.mode == Mode.Normal)
        {
            if (characterRoomPVP != null)
            {
                StartCoroutine(CommonCoroutine.Delay(m_PVPGameOverConfigSO.TimeEndWaitWinCamera + m_PVPGameOverConfigSO.TimeMoveWinCamDuration + m_PVPGameOverConfigSO.TimeWaitCharacter, false, () =>
                {
                    characterRoomPVP.OnStartCameraFinalMatch.Invoke(ShowGameOverUI);
                }));
            }
        }
        else if (matchOfPlayer.mode == Mode.Battle)
        {
            //var battleRoyaleLeaderboardUI = ObjectFindCache<BattleRoyaleLeaderboardUI>.Get();
            //if (battleRoyaleLeaderboardUI != null && characterRoomPVP != null)
            //{
            //    battleRoyaleLeaderboardUI.onContinue += () =>
            //    {
            //        characterRoomPVP.OnStartCameraFinalMatch.Invoke(ShowGameOverUI);
            //    };
            //}
            ShowGameOverUI();
        }
        else
        {
            ShowHeader();
            ShowGameOverUI();
        }

        void ShowHeader()
        {
            var mode = matchOfPlayer?.mode ?? Mode.Normal;
            m_MainCanvasGroupVisibility.Show();
            m_BackGroundCanvasGroupVisibility.Hide();
            showUIAnimSequence.SetToStart();
            m_HeadeEZAnimSequence.Play();

            if (isVictory)
            {
                m_BannerImage.sprite = m_WinBannerSprite;
                m_BannerTitleText.fontMaterial = m_Top1Material;
                if (_winVFX != null)
                    _winVFX.Play();

                StartCoroutine(CommonCoroutine.Delay(0.5f, false, () =>
                {
                    SoundManager.Instance.PlaySFX(PBSFX.UIWinner);
                    SoundManager.Instance.PlaySFX(PBSFX.UIWinFlameThrower);
                }));
            }
            else
            {
                m_BannerImage.sprite = m_LoseBannerSprite;
                m_BannerTitleText.fontMaterial = m_NotTop1Material;
                StartCoroutine(CommonCoroutine.Delay(0.5f, false, () =>
                {
                    SoundManager.Instance.PlaySFX(PBSFX.UIYouLose);
                }));
            }

            m_BannerTitleText.SetText(
                mode == Mode.Battle ?
                I2LHelper.TranslateTerm(I2LTerm.PvP_GameOverUI_Top).Replace("{[Rank]}", matchOfPlayer.rankOfMine.ToString()) :
                (isVictory ? I2LHelper.TranslateTerm(I2LTerm.Text_Winner) :
                    I2LHelper.TranslateTerm(I2LTerm.Text_Lose)));
            m_OutlineBannerTitleText.SetText(m_BannerTitleText.text);
        }

        void ShowGameOverUI()
        {
            bool isShowingMultiply = false;
            if (isVictory && m_MultiplierRewardsDataSO.IsEnoughTrophy())
            {
                if (m_MultiplierRewardsDataSO.IsAbleToShowMultiplierRewards())
                {
                    isShowingMultiply = true;
                }
                m_MultiplierRewardsDataSO.normalSetCountVar.value = (m_MultiplierRewardsDataSO.normalSetCountVar.value + 1) % m_MultiplierRewardsDataSO.config.normalSetFrequency;
            }

            Show(arenaSO, isVictory, isShowingMultiply, resourceLocationProvider: this, isTimesUp: matchOfPlayer.pbEndgameData.isTimesUp);
        }
    }

    public void Show(PvPArenaSO arenaSO, bool isWin, bool isShowMultiplyMoneyBtn = true, bool isFTUE = false,
        IResourceLocationProvider resourceLocationProvider = null, bool isTimesUp = false)
    {
        if (m_WinStreakBanner != null && !m_IsFTUE)
        {
            if (_currentChosenModeVariable.value == Mode.Normal)
                m_WinStreakBanner.Show();
            else
                m_WinStreakBanner.Hide();
        }

        //Get input parameters
        var bonusDelayTime = isTimesUp ? m_DelayTimeWhenTimesUp : 0;
        var playSoundDelayTime = isTimesUp ? 0 : 1;
        var matchOfPlayer = ObjectFindCache<PBPvPMatchManager>.Get()?.GetCurrentMatchOfPlayer() as PBPvPMatch;
        var mode = matchOfPlayer?.mode ?? Mode.Normal;
        var rankOfMine = matchOfPlayer?.rankOfMine ?? 1;
        var isFullBoxSlot = PackDockManager.Instance.IsFull;
        m_ArenaSO = arenaSO;
        m_IsVictory = isWin;
        m_IsFTUE = isFTUE;
        m_IsShowMultiplyRewards = isShowMultiplyMoneyBtn;
        if (!isShowMultiplyMoneyBtn)
        {
            ReduceRewradBoxHeight(0);
        }

        if (mode == Mode.Boss)
        {
            m_RewardBoxWin.SetActive(false);
            m_RewardBoxLose.SetActive(false);
            m_CanvasGroupVisibilityMultiply.Hide();
            m_CanvasGroupVisibilitySkipAddGroup.Hide();
        }

        var trophyPrize =
            m_IsVictory ? arenaSO.TryGetReward(out CurrencyRewardModule medalReward, item => item.CurrencyType == rewardMedalType) ?
            medalReward.Amount * GetRewardMultiplier(mode, CurrencyType.Medal, rankOfMine) : 0f :
            arenaSO.TryGetPunishment(out CurrencyPunishmentModule medalPunishment) ? medalPunishment.amount * GetLoseTrophyMultiplier(matchOfPlayer.mode, rankOfMine) : 0f;

        var moneyBetAmount = mode == Mode.Battle ? (arenaSO as PBPvPArenaSO).GetBattleRoyaleCoinReward(rankOfMine) : m_IsVictory ?
            arenaSO.TryGetReward(out CurrencyRewardModule moneyReward, item => item.CurrencyType == rewardMoneyType) ?
            moneyReward.Amount * GetRewardMultiplier(mode, CurrencyType.Standard, rankOfMine) : 0f : 0f;

        m_moneyEndMatch = moneyBetAmount;
        m_ArenaSO.GetReward<CurrencyRewardModule>(item => item.CurrencyType == rewardMoneyType)
            .resourceLocationProvider = resourceLocationProvider;
        m_BattleEntryFeeTMP.SetText((arenaSO as PBPvPArenaSO).BattleRoyaleEntryFee.ToRoundedText());

        m_BattleEntryFeeGetBackGroup.SetActive(false);
        m_ContinueButton.gameObject.SetActive(true);

        if (!m_IsFTUE)
        {
            if (isWin)
            {
                _ultimatePackCumulatedWinMatchThisSection.value++;
            }

            scriptedGachaPacks = m_ArenaSO.Cast<PBPvPArenaSO>().ScriptedGachaBoxes;
            scriptedGachaPacks.resourceLocationProvider = this;

            if (mode == Mode.Battle)
            {
                bool isWinBattle = isWin;
                m_BattleEntryFeeGetBackGroup.SetActive(!isWinBattle);
                m_ContinueButton.gameObject.SetActive(isWinBattle);

                if (matchOfPlayer.rankOfMine == 1)
                {
                    m_RewardBoxWin.SetActive(true);
                    m_RewardBoxLose.SetActive(false);
                }
                else
                {
                    m_RewardBoxWin.SetActive(false);
                    m_RewardBoxLose.SetActive(true);
                }

                if (!isWinBattle)
                {
                    m_CanvasGroupVisibilityMultiply.Hide();
                }
                m_CanvasGroupVisibilitySkipAddGroup.Hide();

                earnBoxProgressUI.gameObject.SetActive(false);

                var excludingRewards = arenaSO.rewardItems.Where(reward => reward is RandomGachaPackRewardModule);
                var excludingPackRewards = excludingRewards.Cast<RandomGachaPackRewardModule>();
                var rewards = arenaSO.rewardItems.Except(excludingRewards);
                if (matchOfPlayer.rankOfMine == 1)
                {
                    if (scriptedGachaPacks.isRemainPack)
                    {
                        earnBoxProgressUI.Setup(0, GetRandomGachaPack(arenaSO), mode == Mode.Battle);
                        scriptedGachaPacks.GrantReward();
                    }
                    else
                    {
                        var gachaPack = GetRandomGachaPack(arenaSO);
                        earnBoxProgressUI.Setup(0, gachaPack, mode == Mode.Battle);
                        PackDockManager.Instance.TryToAddPack(gachaPack);
                    }
                    earnBoxProgressUI.gameObject.SetActive(true);
                }
                else
                {
                    earnBoxProgressUI.gameObject.SetActive(false);
                }
            }
            else if (mode == Mode.Normal)
            {
                bool isWinSingle = isWin;
                m_RewardBoxWin.SetActive(true);
                m_RewardBoxLose.SetActive(false);

                if (!isWinSingle)
                {
                    m_CanvasGroupVisibilityMultiply.Hide();
                    if (isShowMultiplyMoneyBtn)
                    {
                        ReduceRewradBoxHeight(0);
                    }
                }
                m_CanvasGroupVisibilitySkipAddGroup.Hide();

                var previousCumulatedWinMatchValue = cumulatedWinMatch.value;
                if (m_IsVictory)
                {
                    cumulatedWinMatch.value++;
                    cumulatedWinMatch.value = Mathf.Min(cumulatedWinMatch.value, numberOfWinToGrantReward);

                    if (canGetBox)
                    {
                        activeBoxSlotTheFirstTime.value = true;
                    }
                }

                var excludingDualRewards = arenaSO.rewardItems.Where(reward => reward is RandomGachaPackRewardModule);
                var excludingDualPackRewards = excludingDualRewards.Cast<RandomGachaPackRewardModule>();
                var dualRewards = arenaSO.rewardItems.Except(excludingDualRewards)
                    .Where(reward => CheckRewardIsMoneyReward(reward) == false);

                if (scriptedGachaPacks.isRemainPack)
                {
                    earnBoxProgressUI.Setup(previousCumulatedWinMatchValue, scriptedGachaPacks.currentManualGachaPack, mode == Mode.Battle);
                    if (canGetBox)
                    {
                        scriptedGachaPacks.GrantReward();
                    }
                }
                else
                {
                    var gachaPack = excludingDualPackRewards.First().gachaPackCollection.GetRandom();
                    earnBoxProgressUI.Setup(previousCumulatedWinMatchValue, gachaPack, mode == Mode.Battle);
                    if (canGetBox)
                    {
                        PackDockManager.Instance.TryToAddPack(gachaPack);
                    }
                }

                earnBoxProgressUI.gameObject.SetActive(true);

                bool CheckRewardIsMoneyReward(IReward reward)
                {
                    if (m_IsFTUE == true)
                        return false;

                    if (reward is CurrencyRewardModule currencyRewardModule)
                    {
                        return currencyRewardModule.CurrencyType == rewardMoneyType;
                    }
                    return false;
                }


            }
        }
        else
        {
            earnBoxProgressUI.gameObject.SetActive(false);
        }

        if (m_IsVictory)
        {
            CurrencyManager.Instance.Acquire(rewardMedalType, trophyPrize, _currentChosenModeVariable.value == Mode.Normal ? ResourceLocation.SinglePvP : ResourceLocation.BattlePvP, Const.ResourceItemId.PvPLose);
        }
        else
        {
            CurrencyManager.Instance.Spend(rewardMedalType, trophyPrize, _currentChosenModeVariable.value == Mode.Normal ? ResourceLocation.SinglePvP : ResourceLocation.BattlePvP, Const.ResourceItemId.PvPLose);
        }

        if (m_IsFTUE)
        {
            earnBoxProgressUI.gameObject.SetActive(true);
            progressBarBox.SetActive(false);
            titleYourNextBox.Play();
            titleYourGotABox.Play();
            boxUnlockEzAnim.Play();
            earnBoxProgressUI.Setup(3, FTUE_GachaPack, false, true);
        }

        StartCoroutine(CommonCoroutine.Delay(mode == Mode.Battle ? BATTLE_DELAY_SHOW_GAME_OVER_UI : bonusDelayTime, false, () =>
        {
            if (m_IsVictory)
            {
                if (mode == Mode.Battle)
                {
                    m_WinMedalText.gameObject.SetActive(true);
                    m_LoseMedalText.gameObject.SetActive(false);
                    m_LosePrizeText.gameObject.SetActive(false);
                    m_LosePrizePlus.gameObject.SetActive(true);
                    m_LoseMedalPlus.gameObject.SetActive(true);
                    m_WinMedalText.From = 0;
                    m_LosePrizePlus.To = moneyBetAmount;
                    m_LoseMedalPlus.To = trophyPrize;
                    m_LoseMedalPlus.gameObject.GetComponent<TMP_Text>().color = m_WinTrophyRewardTextColor;
                    m_LosePrizePlus.Play();
                    m_LoseMedalPlus.Play();
                }

                m_WinPrizeText.From = 0;
                m_WinPrizeText.To = moneyBetAmount;
                m_WinMedalText.From = 0;
                m_WinMedalText.To = trophyPrize;
                m_TrophyRewardText.color = m_WinTrophyRewardTextColor;

            }
            else
            {

                if (mode == Mode.Normal)
                {
                    m_WinPrizeText.From = 0;
                    m_WinPrizeText.To = 0;
                    m_WinMedalText.gameObject.SetActive(false);
                    m_WinMedalMinus.gameObject.SetActive(true);
                    m_WinMedalMinus.From = 0;
                    m_WinMedalMinus.To = trophyPrize;
                    m_WinMedalMinus.gameObject.GetComponent<TMP_Text>().color = m_LoseTrophyRewardTextColor;
                    m_WinMedalMinus.Play();
                }

                m_LoseMedalText.gameObject.SetActive(true);
                m_LoseMedalPlus.gameObject.SetActive(false);
                m_LoseMedalText.From = 0;
                m_LoseMedalText.To = trophyPrize;

                m_TrophyRewardText.color = (mode == Mode.Battle && rankOfMine <= 3) ? m_WinTrophyRewardTextColor :
                    m_LoseTrophyRewardTextColor;
            }


            //m_WinBoard.SetActive(isWin);
            //m_LoseBoard.SetActive(!isWin);


            //m_RewardGroupRect.sizeDelta = new Vector2(m_RewardGroupRect.sizeDelta.x, !isShowMultiplyMoneyBtn ? 160 : 500);
            if (isShowMultiplyMoneyBtn)
            {
                m_ContinueButtonText.SetText(I2LHelper.TranslateTerm(I2LTerm.Text_Next));
                //v1.5.1: Set false multiplyRVButton, multiplyRewardArc
                multiplyRVButton.gameObject.SetActive(true);
                multiplyRewardArc.gameObject.SetActive(true);
            }
            else
            {
                // m_ContinueButton.GetComponent<LayoutElement>().flexibleWidth = 1f;
                m_ContinueButtonText.SetText(I2LHelper.TranslateTerm(I2LTerm.Text_Continue));
                multiplyRVButton.gameObject.SetActive(false);
                multiplyRewardArc.gameObject.SetActive(false);
            }

            m_UIVisibilityController.ShowImmediately();

            m_BackGroundCanvasGroupVisibility.Show();
            showUIAnimSequence.Play(() =>
            {
                if (isShowMultiplyMoneyBtn)
                {
                    //v1.5.1: Comment Run multiplyRewardArc
                    multiplyRewardArc.StartRun();

                    if (multiplyRewardArc.IsPromoted)
                        m_MultiplierRewardsDataSO.promotedSetCountVar.value = 0;

                    //TODO: Hide IAP & Popup
                    m_CanvasGroupVisibilityMultiply.Hide();
                    m_CanvasGroupVisibilitySkipAddGroup.Hide();                    
                    //m_CanvasGroupVisibilityMultiply.Show();
                    //m_CanvasGroupVisibilitySkipAddGroup.Show();

                    #region Design Events
                    if (_currentChosenModeVariable.value == Mode.Normal || _currentChosenModeVariable.value == Mode.Battle)
                    {
                        string set = multiplyRewardArc.IsPromoted ? "Promoted" : "Normal";
                        string rvName = $"MultiplierRewards_{set}";
                        string location = _currentChosenModeVariable.value == Mode.Normal ? "SinglePvP" : "BattlePvP";
                        GameEventHandler.Invoke(DesignEvent.RVShow);
                    }
                    #endregion
                }
                if (!m_IsFTUE)
                {
                    PiggyBankManager.Instance.CalcPerKill();
                    if (mode == Mode.Normal && isWin)
                    {
                        earnBoxProgressUI.OnEndAnimBox.AddListener(OnEarnBoxProgressEndAnim);
                        earnBoxProgressUI.PlayIncreaseAnimation(cumulatedWinMatch.value);
                    }
                    else if (mode == Mode.Battle && matchOfPlayer.rankOfMine == 1)
                    {
                        earnBoxProgressUI.OnEndAnimBox.AddListener(OnEarnBoxProgressEndAnim);
                        earnBoxProgressUI.PlayShowingBoxAnim();
                    }
                }

                //Win Streak
                if (_currentChosenModeVariable == Mode.Normal && !m_IsFTUE)
                {
                    GameEventHandler.Invoke(WinStreakPopup.CheckGameOver, isWin);
                }
                showContinueBtnAnim.Play();
            });

            GameEventHandler.Invoke(PBPvPEventCode.OnShowGameOverUI, matchOfPlayer);

            void OnEarnBoxProgressEndAnim()
            {
                earnBoxProgressUI.OnEndAnimBox.RemoveListener(OnEarnBoxProgressEndAnim);
                earnBoxProgressUI.ShowGachaPack(isFullBoxSlot);
            }
        }));
    }

    private void ReduceRewradBoxHeight(float amount)
    {
        _rewardBoxWinTrs.sizeDelta -= new Vector2(0, amount);
    }

    public string GetItemId()
    {
        return m_ArenaSO != null ?
            Const.ResourceItemId.PvPVictory.Replace("{ArenaType}", $"Arena {m_ArenaSO.index + 1}") :
            Const.ResourceItemId.PvPVictory.Replace("{ArenaType}", "Arena 1");
    }

    public ResourceLocation GetLocation()
    {
        return ResourceLocation.PvP;
    }

    private void OnStartAnimBoxProgress()
    {
        m_ContinueButton.interactable = false;
        multiplyRVButton.interactable = false;
    }

    private void OnEndAnimBoxProgress()
    {
        m_ContinueButton.interactable = true;
        multiplyRVButton.interactable = true;
    }

    private void EndOpenBoxNow()
    {
        ReduceRewradBoxHeight(0);
    }

    public GachaPack GetRandomGachaPack(PvPArenaSO arenSO)
    {
        if (m_CachedGachaPack == null)
        {
            var scriptedGachaPacks = arenSO.Cast<PBPvPArenaSO>().ScriptedGachaBoxes;
            if (scriptedGachaPacks.isRemainPack)
            {
                m_CachedGachaPack = scriptedGachaPacks.currentManualGachaPack;
            }
            else
            {
                m_CachedGachaPack = arenSO.GetReward<RandomGachaPackRewardModule>().gachaPackCollection.GetRandom();
            }
        }
        return m_CachedGachaPack;
    }

    //This function is called in the OnEnoughGemClicked event of PackDockOpenByGemButton out the inspector, check it in the inspector
    public virtual void OnOpenNowClicked()
    {
        if (scriptedGachaPacks.isRemainPack)
        {
            scriptedGachaPacks.Next();
        }
    }

    public static float GetLoseTrophyMultiplier(Mode mode, int rank)
    {
        if (mode == Mode.Normal)
            return 1f;
        switch (rank)
        {
            case 4:
                return 0.5f;
            case 5:
                return 1f;
            case 6:
                return 1f;
            default:
                return 0f;
        }
    }

    public static float GetRewardMultiplier(Mode mode, CurrencyType currencyType, int rank)
    {
        if (mode == Mode.Normal)
            return 1f;
        if (currencyType == CurrencyType.Medal)
        {
            switch (rank)
            {
                case 1:
                    return 2f;
                case 2:
                    return 1f;
                case 3:
                    return 0f;
                default:
                    return 0f;
            }
        }
        else
        {
            switch (rank)
            {
                case 1:
                    return 2f;
                case 2:
                    return 1f;
                case 3:
                    return 0.5f;
                default:
                    return 0f;
            }
        }
    }
}