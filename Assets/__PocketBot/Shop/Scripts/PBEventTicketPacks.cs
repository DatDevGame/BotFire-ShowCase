using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HyrphusQ.Events;

public class PBEventTicketPacks : MonoBehaviour
{
    [SerializeField]
    private HighestAchievedPPrefFloatTracker _highestAchivedTrophy;
    [SerializeField]
    private IntVariable _trophyToUnlockBattleMode;
    [SerializeField]
    private List<ShopProductSO> _currencyProductSOs;

    private List<ShopBuyButton> _currencyPackButtons;

    private void Start()
    {      
        Initialize();
        if (_highestAchivedTrophy.value < _trophyToUnlockBattleMode)
        {
            gameObject.SetActive(false);
            CurrencyManager.Instance[CurrencyType.Medal].onValueChanged +=OnTrophyChanged;
        }
    }

    private void OnDestroy()
    {
        if (CurrencyManager.Instance)
            CurrencyManager.Instance[CurrencyType.Medal].onValueChanged -=OnTrophyChanged;
    }

    private void Initialize()
    {
        _currencyPackButtons = GetComponentsInChildren<ShopBuyButton>().ToList();
        for (int i = 0; i < _currencyProductSOs.Count; i++)
        {
            var currencyProductSO = _currencyProductSOs[i] as PBCurrencyProductSO;
            _currencyPackButtons[i].shopProductSO = currencyProductSO;
            _currencyPackButtons[i].OverrideSetup(currencyProductSO);
        }
    }

    private void OnTrophyChanged(ValueDataChanged<float> dataChanged)
    {
        if (dataChanged.newValue >= _trophyToUnlockBattleMode.value)
        {
            gameObject.SetActive(true);
            if (CurrencyManager.Instance)
                CurrencyManager.Instance[CurrencyType.Medal].onValueChanged -=OnTrophyChanged;
        }
    }
}