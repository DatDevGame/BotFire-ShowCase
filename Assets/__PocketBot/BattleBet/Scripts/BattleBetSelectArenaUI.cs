using HyrphusQ.Events;
using LatteGames.PvP;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleBetSelectArenaUI : MonoBehaviour
{
    [SerializeField, BoxGroup("UI")] private Image _arenaLogo;
    [SerializeField, BoxGroup("UI")] private Image _pattern;
    [SerializeField, BoxGroup("UI")] private Image _arenaBackground;
    [SerializeField, BoxGroup("UI")] private TextMeshProUGUI _arenaName;
    [SerializeField, BoxGroup("UI")] private TextMeshProUGUI _rewardCoinAmount;
    [SerializeField, BoxGroup("UI")] private TextMeshProUGUI _rewardTrophyAmount;
    [SerializeField, BoxGroup("UI")] private TextMeshProUGUI _arenaUnlockTrophyAmount;
    [SerializeField, BoxGroup("UI")] private TextMeshProUGUI _entryFee;
    [SerializeField, BoxGroup("UI")] private TextMeshProUGUI _ticketAmount;
    [SerializeField, BoxGroup("UI")] private Button _enterButton;
    [SerializeField, BoxGroup("UI")] private GrayscaleUI _enterButtonGrayScale;
    [SerializeField, BoxGroup("UI")] private GameObject _unlockGO;
    [SerializeField, BoxGroup("UI")] private GameObject _handFUTE;
    [SerializeField, BoxGroup("Config")] private float _nonCenterScaleValue;
    [SerializeField, BoxGroup("Config")] private float _centerScaleRange;
    [SerializeField, BoxGroup("Config")] private AnimationCurve _scaleCurve;

    [SerializeField, BoxGroup("Data")] private CurrentHighestArenaVariable m_CurrentHighestArenaVariable;
    [SerializeField, BoxGroup("Data")] private PPrefBoolVariable _pPrefBoolBattleRoyal;

    private PvPArenaSO _pvpArenaSO;
    private bool _isEnableEffect;

    #region Unity Messages
    private void Awake()
    {
        _enterButton.onClick.AddListener(OnTryEnterBattle);
        DisableEffect();
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnNewArenaUnlocked, SetupLockStatus);
        GameEventHandler.AddActionEvent(BattleBetEventCode.OnBattleBetOpened, EnableEffect);
        GameEventHandler.AddActionEvent(BattleBetEventCode.OnBattleBetClosed, DisableEffect);
    }

    private void Update()
    {
        if (_isEnableEffect)
        {
            float distanceToCenter = Mathf.Abs(transform.position.x - Screen.width * 0.5f);
            float curveFactor = 1 - Mathf.Clamp01(distanceToCenter / _centerScaleRange);
            transform.localScale = Mathf.Lerp(_nonCenterScaleValue, 1, _scaleCurve.Evaluate(curveFactor)) * Vector3.one;
        }
    }

    private void OnDestroy()
    {
        _enterButton.onClick.RemoveListener(OnTryEnterBattle);
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnNewArenaUnlocked, SetupLockStatus);
        GameEventHandler.RemoveActionEvent(BattleBetEventCode.OnBattleBetOpened, EnableEffect);
        GameEventHandler.RemoveActionEvent(BattleBetEventCode.OnBattleBetClosed, DisableEffect);
    }
    #endregion

    public void OnTryEnterBattle()
    {
        ResourceLocationProvider EntryFreeResourceLocationProvider = new ResourceLocationProvider(ResourceLocation.BattlePvP, $"A{m_CurrentHighestArenaVariable.value.index + 1}");

        if (_pvpArenaSO == null || !_pvpArenaSO.IsUnlocked())
            return;
        if (CurrencyManager.Instance[CurrencyType.EventTicket].value <= 0 && _pPrefBoolBattleRoyal.value)
            EventTicketPopup.Instance?.Show();
        else if (_pvpArenaSO.TryGetEntryRequirement(out Requirement_Currency coinEntry, coinEntry => coinEntry.currencyType == CurrencyType.Standard) &&
            !coinEntry.IsMeetRequirement() && _pPrefBoolBattleRoyal.value)
            CoinPackPopup.Instance?.Show();
        else if ((CurrencyManager.Instance.Spend(
                coinEntry.currencyType,
                coinEntry.requiredAmountOfCurrency,
                EntryFreeResourceLocationProvider.GetLocation(),
                EntryFreeResourceLocationProvider.GetItemId()) || !_pPrefBoolBattleRoyal.value) &&
            (CurrencyManager.Instance.Spend(CurrencyType.EventTicket, 1, EntryFreeResourceLocationProvider.GetLocation(),
                EntryFreeResourceLocationProvider.GetItemId()) || !_pPrefBoolBattleRoyal.value))
        {
            GameEventHandler.Invoke(BattleBetEventCode.OnEnterBattle, _pvpArenaSO);
        }
    }

    public void SetupFTUE(bool enable)
    {
        _handFUTE.SetActive(enable);
    }

    public void InitData(PvPArenaSO pvpArenaSO)
    {
        _pvpArenaSO = pvpArenaSO;
        SetupUI();
        SetupLockStatus();
    }

    private void SetupUI()
    {
        if (_pvpArenaSO == null)
            return;
        PBPvPArenaSO pbPvPArenaSO = _pvpArenaSO as PBPvPArenaSO;
        _arenaLogo.sprite = pbPvPArenaSO.BattleBetArenaLogo;
        _arenaBackground.sprite = pbPvPArenaSO.BattleBetArenaBG;
        _pattern.sprite = pbPvPArenaSO.PatternSprite;
        _pattern.material = Instantiate(_pattern.material);
        _pattern.material.SetColor("_Color", pbPvPArenaSO.BattleBetPatternColor);
        _arenaName.SetText(_pvpArenaSO.TryGetModule(out NameItemModule nameModule) ? nameModule.displayName : string.Empty);
        _rewardCoinAmount.SetText("+" + pbPvPArenaSO.GetBattleRoyaleCoinReward(1));
        _rewardTrophyAmount.SetText(_pvpArenaSO.TryGetReward(out CurrencyRewardModule trophyReward, item => item.CurrencyType == CurrencyType.Medal) ?
        "+" + PBPvPGameOverUI.GetRewardMultiplier(Mode.Battle, CurrencyType.Medal, 1) * trophyReward.Amount : string.Empty);
        _arenaUnlockTrophyAmount.SetText(_pvpArenaSO.TryGetUnlockRequirement(out Requirement_Currency trophyUnlock, trophyUnlock => trophyUnlock.currencyType == CurrencyType.Medal) ?
        Mathf.RoundToInt(trophyUnlock.requiredAmountOfCurrency).ToString() : Const.IntValue.Zero.ToString());
        _entryFee.SetText(pbPvPArenaSO.BattleRoyaleEntryFee.ToString());
        //FIXME: after AB test complete, setup event ticket amount requirement in ArenaSO
        //_ticketAmount.SetText($"{_ticketCountProgressVariable.rangeProgress.value}/{_ticketCountProgressVariable.rangeProgress.maxValue}");
    }

    private void SetupLockStatus()
    {
        bool isUnlocked = _pvpArenaSO != null && _pvpArenaSO.IsUnlocked();
        _unlockGO.SetActive(!isUnlocked);
        _enterButton.interactable = isUnlocked;
        _enterButtonGrayScale.SetGrayscale(!isUnlocked);
    }

    private void EnableEffect()
    {
        _isEnableEffect = true;
    }

    private void DisableEffect()
    {
        _isEnableEffect = false;
    }
}
