using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LatteGames.Monetization;
using TMPro;
using UnityEngine;

namespace PackReward
{
    public class PackIAP : LG_IAPButton
    {
        [SerializeField] protected TMP_Text m_RewardValueText;

        protected virtual void Start()
        {
            if (shopProductSO.currencyItems.Count > 0)
            {
                m_RewardValueText.SetText(shopProductSO.currencyItems.ElementAt(0).Value.value.ToString());
                return;
            }

            if (shopProductSO.generalItems.Count > 0)
            {
                m_RewardValueText.SetText(shopProductSO.generalItems.ElementAt(0).Value.value.ToString());
                return;
            }
        }
    }
}

