using System;
using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using LatteGames.Template;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeGearButton : MonoBehaviour
{
    public Action OnCompletedUpgrade = delegate { };

    public PBPartSO partSO;
    [SerializeField] PPrefBoolVariable Upgrade;
    [SerializeField] GameObject ftueHand;
    [SerializeField] GearInfoPopup GearInfoPopup;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(HandleOnUpgrade);
    }

    private void Start()
    {
        if (Upgrade.value) return;
        ftueHand.SetActive(true);
    }

    void HandleOnUpgrade()
    {
        if (partSO != null)
        {
            if (partSO.IsUpgradable())
            {
                #region Firebase Event
                int coinSpend = 0;
                if (partSO.GetModule<PBUpgradableItemModule>().TryGetCurrentUpgradeRequirement(out Requirement_Currency requirement_Currency))
                    coinSpend = (int)requirement_Currency.requiredAmountOfCurrency;
                GameEventHandler.Invoke(LogFirebaseEventCode.ItemUpgrade, partSO, coinSpend);
                #endregion

                PBUpgradableItemModule upgradableItemModule = partSO.GetModule<PBUpgradableItemModule>();
                upgradableItemModule.TryUpgrade();
                SoundManager.Instance.PlaySFX(PBSFX.UIUpgrade);

                OnCompletedUpgrade.Invoke();
                //FTUE
                if (Upgrade.value) return;
                GameEventHandler.Invoke(LogFTUEEventCode.EndUpgrade);
                GameEventHandler.Invoke(FTUEEventCode.OnFinishUpgradeFTUE);
                Upgrade.value = true;
                ftueHand.SetActive(false);
                GearInfoPopup.OnClose();
            }
            else
            {
                if (partSO.TryGetCurrentUpgradeRequirement(out Requirement_Currency requirement))
                {
                    var remainedAmountOfCurrency = requirement.requiredAmountOfCurrency - requirement.currentAmountOfCurrency;
                    GameEventHandler.Invoke(CurrencyExchangePopupEventCode.OnShowExchangeCurrencyPopupUI, remainedAmountOfCurrency, (Action<bool>)OnExchangeCompleted, partSO);

                    PBGemOfferPopup.Instance.WantToActiveOffer();
                }

                void OnExchangeCompleted(bool isSucceeded)
                {
                    if (isSucceeded)
                    {
                        HandleOnUpgrade();
                        SoundManager.Instance.PlaySFX(PBSFX.UIUpgrade);
                    }
                }
            }
        }
    }

    public void AddEvent()
    {
        GetComponent<Button>().onClick.RemoveAllListeners();
        GetComponent<Button>().onClick.AddListener(HandleOnUpgrade);
    }
}
