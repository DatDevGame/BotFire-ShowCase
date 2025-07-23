using System;
using System.Collections.Generic;
using System.Linq;
using GachaSystem.Core;
using HyrphusQ.Events;
using HyrphusQ.SerializedDataStructure;
using LatteGames.Monetization;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class LadderedOfferCell : MonoBehaviour
{
    [SerializeField, BoxGroup("UI Ref")] private GameObject _lockedGO, _claimedGO, _selectedGO, _bgGO;
    [SerializeField, BoxGroup("UI Ref")] private Button _freeBtn;
    [SerializeField, BoxGroup("UI Ref")] private RVButtonBehavior _claimRVBtn;
    [SerializeField, BoxGroup("UI Ref")] private LG_IAPButton _iapBtn;
    [SerializeField, BoxGroup("UI Ref")] private LadderedOfferRewardItemCell _itemCellTemplate;
    [SerializeField, BoxGroup("UI Ref")] private Transform _itemRoots;
    [SerializeField, BoxGroup("UI Ref")] private MultiImageTargetGraphics btns;
    [SerializeField, BoxGroup("Data")] private PBGachaPackManagerSO _gachaPackManagerSO;
    [SerializeField, BoxGroup("Asset")] RaritySpriteSO _raritySprite;
    [SerializeField, BoxGroup("Config")] Color _normalBtnColor;
    [SerializeField, BoxGroup("Config")] Color _lockedBtnColor;

    #region Resource Provider
    [TitleGroup("IAP Settings"), GUIColor(0.8f, 0.8f, 1f)]
    [FoldoutGroup("IAP Settings/Source"), SerializeField]
    private ResourceLocationProvider m_IAPSourceProvider;

    [TitleGroup("WatchRV Settings"), GUIColor(0.7f, 1f, 0.7f)]
    [FoldoutGroup("WatchRV Settings/Source"), SerializeField]
    private ResourceLocationProvider m_RVSourceProviderCoin;

    [FoldoutGroup("WatchRV Settings/Source"), SerializeField]
    private ResourceLocationProvider m_RVSourceProviderGem;

    [FoldoutGroup("WatchRV Settings/Source"), SerializeField]
    private ResourceLocationProvider m_RVSourceProviderRVTicket;

    [TitleGroup("Free Settings"), GUIColor(1f, 0.9f, 0.6f)]
    [FoldoutGroup("Free Settings/Source"), SerializeField]
    private ResourceLocationProvider m_FreeSourceProviderCoin;

    [FoldoutGroup("Free Settings/Source"), SerializeField]
    private ResourceLocationProvider m_FreeSourceProviderGem;

    [FoldoutGroup("Free Settings/Source"), SerializeField]
    private ResourceLocationProvider m_FreeSourceProviderRVTicket;

    [TitleGroup("Skill Settings"), GUIColor(1f, 0.2f, 0.6f)]
    [FoldoutGroup("Skill Settings/Source"), SerializeField]
    private ResourceLocationProvider m_SkillCardSourceProviderRV;

    [FoldoutGroup("Skill Settings/Source"), SerializeField]
    private ResourceLocationProvider m_SkillCardSourceProviderPurchase;

    [FoldoutGroup("Skill Settings/Source"), SerializeField]
    private ResourceLocationProvider m_SkillCardSourceProviderFree;
    #endregion

    public Vector3 globalCenter => _bgGO.transform.position;

    private LadderedOfferReward _reward = null;
    private int _myIndex;
    private List<LadderedOfferRewardItemCell> _itemCells = new();
    private List<LadderedOfferRewardItemCell> _disabledItemCell = new();

    private void Awake()
    {
        _freeBtn.onClick.AddListener(OnClickFreeBtn);
        GameEventHandler.AddActionEvent(IAPEventCode.OnPurchaseItemCompleted, OnPurchaseCompleted);
        _claimRVBtn.OnRewardGranted += OnRewardGrantedClaim;
        EnableOnly(null);
    }

    private void OnDestroy()
    {
        _freeBtn.onClick.RemoveListener(OnClickFreeBtn);
        GameEventHandler.RemoveActionEvent(IAPEventCode.OnPurchaseItemCompleted, OnPurchaseCompleted);
        _claimRVBtn.OnRewardGranted -= OnRewardGrantedClaim;
    }

    public void Init(LadderedOfferReward reward, int myIndex)
    {
        //TODO: show reward
        _reward = reward;
        _myIndex = myIndex;
        _freeBtn.gameObject.SetActive(true);
        switch (_reward.rewardClaimType)
        {
            case LadderedOfferRewardClaimType.IAP:
                EnableOnly(_iapBtn.gameObject);
                _iapBtn.OverrideSetup(reward.iapProductSO);
                break;
            case LadderedOfferRewardClaimType.Free:
                EnableOnly(_freeBtn.gameObject);
                break;
            case LadderedOfferRewardClaimType.WatchRV:
                EnableOnly(_claimRVBtn.gameObject);
                break;
        }

        RewardGroupInfo rewardGroupInfo = _reward.GetReward(_gachaPackManagerSO);
        RemoveAllItemCell();
        if (rewardGroupInfo.currencyItems != null && rewardGroupInfo.currencyItems.Count > 0)
        {
            foreach (var currencyPair in rewardGroupInfo.currencyItems)
            {
                var itemCell = GetItemCell();
                itemCell.thumpnail.sprite = CurrencyManager.Instance.GetCurrencySO(currencyPair.Key).icon;
                itemCell.amountTxt.text = currencyPair.Value.value.ToRoundedText();
                itemCell.rarityOutline.sprite = _raritySprite.GetRaritySprite(currencyPair.Key);
                itemCell.transform.SetAsLastSibling();
                itemCell.gameObject.SetActive(true);
                _itemCells.Add(itemCell);
            }
        }
        if (rewardGroupInfo.generalItems != null && rewardGroupInfo.generalItems.Count > 0)
        {
            foreach (var generalPair in rewardGroupInfo.generalItems)
            {
                var itemCell = GetItemCell();
                itemCell.thumpnail.sprite = generalPair.Key.GetThumbnailImage();
                itemCell.amountTxt.text = generalPair.Value.value.ToRoundedText();
                itemCell.rarityOutline.sprite = _raritySprite.GetRaritySprite(generalPair.Key.GetRarityType());
                itemCell.transform.SetAsLastSibling();
                itemCell.gameObject.SetActive(true);
                _itemCells.Add(itemCell);
            }
        }
    }

    private void RemoveAllItemCell()
    {
        foreach (LadderedOfferRewardItemCell itemCell in _itemCells)
        {
            _disabledItemCell.Add(itemCell);
            itemCell.gameObject.SetActive(false);
        }
        _itemCells.Clear();
    }

    private LadderedOfferRewardItemCell GetItemCell()
    {
        LadderedOfferRewardItemCell itemCell;
        if (_disabledItemCell.Count > 0)
        {
            itemCell = _disabledItemCell[_disabledItemCell.Count - 1];
            _disabledItemCell.RemoveAt(_disabledItemCell.Count - 1);
            return itemCell;
        }
        itemCell = Instantiate(_itemCellTemplate, _itemRoots);
        return itemCell;
    }

    public void UpdateStatus(int currentIndex)
    {
        if (_myIndex < currentIndex)
        {
            EnableOnly(null);
        }
        foreach(var graphic in btns.GetTargetGraphics)
        {
            graphic.color = _myIndex > currentIndex ? _lockedBtnColor : _normalBtnColor;
        }
        _selectedGO.SetActive(_myIndex == currentIndex);
        _lockedGO.SetActive(_myIndex > currentIndex);
        _claimedGO.SetActive(_myIndex < currentIndex);
    }

    private void EnableOnly(GameObject theOnly)
    {
        _freeBtn.gameObject.SetActive(theOnly == _freeBtn.gameObject);
        _claimRVBtn.gameObject.SetActive(theOnly == _claimRVBtn.gameObject);
        _iapBtn.gameObject.SetActive(theOnly == _iapBtn.gameObject);
    }

    private void OnClickFreeBtn()
    {
        if (_myIndex == LadderedOfferPopup.Instance.ladderedOfferSO.ladderStepIndex &&
            _reward != null && _reward.rewardClaimType == LadderedOfferRewardClaimType.Free)
        {
            TakeReward();

            #region Source Event
            LogSourceEvent(m_SkillCardSourceProviderFree);
            #endregion
        }
    }

    private void OnPurchaseCompleted(params object[] objects)
    {
        if (_myIndex == LadderedOfferPopup.Instance.ladderedOfferSO.ladderStepIndex &&
            _reward != null && _reward.rewardClaimType == LadderedOfferRewardClaimType.IAP &&
            objects[0] is LG_IAPButton lG_IAPButton && lG_IAPButton.IAPProductSO == _reward.iapProductSO)
        {
            TakeReward();

            #region Source Event
            LogSourceEvent(m_SkillCardSourceProviderPurchase);
            #endregion
        }
    }

    void OnRewardGrantedClaim(RVButtonBehavior.RewardGrantedEventData data)
    {
        if (_myIndex == LadderedOfferPopup.Instance.ladderedOfferSO.ladderStepIndex &&
            _reward != null && _reward.rewardClaimType == LadderedOfferRewardClaimType.WatchRV)
        {
            TakeReward();

            #region MonetizationEventCode
            try
            {
                string setNumber = _reward.Set;
                string packValue = _reward.Pack;
                GameEventHandler.Invoke(MonetizationEventCode.LadderedOffer, setNumber, packValue);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                Debug.LogError($"{LogErrorKeyGA.Key}-{ex}");
            }
            #endregion

            #region Source Event
            LogSourceEvent(m_SkillCardSourceProviderRV);
            #endregion
        }
    }

    private void TakeReward()
    {
        RewardGroupInfo rewardGroupInfo = _reward.GetReward(_gachaPackManagerSO);
        ((PBGachaCardGenerator)GachaCardGenerator.Instance).GenerateRewards(
                new List<RewardGroupInfo>() { rewardGroupInfo },
                out List<GachaCard> gachaCards,
                out List<GachaPack> gachaPacks);


        if (rewardGroupInfo.generalItems != null)
        {
            ItemSO itemSO = rewardGroupInfo.generalItems.FirstOrDefault().Key;
            if (itemSO != null)
            {
                if (rewardGroupInfo.generalItems.FirstOrDefault().Key is GachaCard_RandomActiveSkill gachaCard_RandomActiveSkill)
                {
                    List<GachaCard> gachaSkillCards = (GachaCardGenerator.Instance as PBGachaCardGenerator).Generate(rewardGroupInfo);
                    if (gachaSkillCards != null && gachaSkillCards.Count > 0)
                        gachaCards = gachaSkillCards;
                }
            }
        }

        //Add Resource Provider
        try
        {
            if (gachaCards != null)
            {
                gachaCards.ForEach(card =>
                {
                    if (card == null) return;
                    if (card is GachaCard_Currency gachaCard_Currency)
                    {
                        gachaCard_Currency.ResourceLocationProvider = _reward.rewardClaimType switch
                        {
                            LadderedOfferRewardClaimType.Free => gachaCard_Currency.CurrencyType switch 
                            {
                                CurrencyType.Standard => m_FreeSourceProviderCoin,
                                CurrencyType.Premium => m_FreeSourceProviderGem,
                                CurrencyType.RVTicket => m_FreeSourceProviderRVTicket,
                                _ => m_FreeSourceProviderCoin
                            },
                            LadderedOfferRewardClaimType.WatchRV => gachaCard_Currency.CurrencyType switch
                            {
                                CurrencyType.Standard => m_RVSourceProviderCoin,
                                CurrencyType.Premium => m_RVSourceProviderGem,
                                CurrencyType.RVTicket => m_RVSourceProviderRVTicket,
                                _ => m_RVSourceProviderCoin
                            },
                            LadderedOfferRewardClaimType.IAP => m_IAPSourceProvider,
                            _ => new ResourceLocationProvider(ResourceLocation.None, "")
                        };
                    }
                });
            }
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            Debug.LogError($"{LogErrorKeyGA.Key}-{ex}");
        }

        GameEventHandler.Invoke(UnpackEventCode.OnUnpackStart, gachaCards, gachaPacks, null);
        LadderedOfferPopup.Instance.ladderedOfferSO.UnlockNextStep();
        LadderedOfferPopup.Instance.UpdateCellsStatus();

        #region Progression Event
        try
        {
            string status = "Start";
            string setNumber = _reward.Set;
            string packValue = _reward.Pack;
            GameEventHandler.Invoke(ProgressionEvent.LadderedOffer, status, setNumber, packValue);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            Debug.LogError($"{LogErrorKeyGA.Key}-{ex}");
        }
        #endregion

        #region Log Event
        try
        {
            if (gachaPacks != null)
            {
                for (int i = 0; i < gachaPacks.Count; i++)
                {
                    if (gachaPacks[i] != null)
                    {
                        #region DesignEvent
                        string openStatus = "NoTimer";
                        string location = "LadderedOffer";
                        GameEventHandler.Invoke(DesignEvent.OpenBox, openStatus, location);
                        #endregion

                        #region Firebase Event
                        string openType = "RV";
                        GameEventHandler.Invoke(LogFirebaseEventCode.BoxOpen, gachaPacks[i], openType);
                        #endregion
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            Debug.LogError($"{LogErrorKeyGA.Key}-{ex}");
        }
        #endregion
    }

    #region Source Event
    private void LogSourceEvent(ResourceLocationProvider resourceLocationProvider)
    {
        try
        {
            LadderedOfferSO ladderedOfferSO = LadderedOfferPopup.Instance.ladderedOfferSO;
            for (int i = 0; i < _reward.Rewards.Count; ++i)
            {
                Reward reward = _reward.Rewards[i];
                if (reward.type == RewardType.SkillCard)
                {
                    float skillCardCount = reward.amount;
                    GameEventHandler.Invoke(LogSinkSource.SkillCard, skillCardCount, resourceLocationProvider);
                }
            }

        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            Debug.LogError($"{LogErrorKeyGA.Key}-{ex}");
        }

    }
    #endregion
}