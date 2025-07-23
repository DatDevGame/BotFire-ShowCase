using System;
using System.Collections;
using System.Collections.Generic;
using GachaSystem.Core;
using HyrphusQ.Events;
using HyrphusQ.GUI;
using UnityEngine;
using UnityEngine.UI;

public class ActiveSkillCardFreeOffer : MonoBehaviour
{
    [SerializeField]
    private int m_CardQuantity = 1;
    [SerializeField]
    private Image m_IconImage;
    [SerializeField]
    private TextAdapter m_CardQuantityText;
    [SerializeField]
    private Button m_ClaimButton;

    private ActiveSkillSO m_ActiveSkillSO;
    private Action m_OnClaimCompleted;

    public Button button => m_ClaimButton;

    private void Start()
    {
        m_ClaimButton.onClick.AddListener(() =>
        {
            List<GachaCard> gachaCards = (PBGachaCardGenerator.Instance as PBGachaCardGenerator).Generate(new RewardGroupInfo()
            {
                generalItems = new Dictionary<ItemSO, ShopProductSO.DiscountableValue>()
            {
                { m_ActiveSkillSO, new ShopProductSO.DiscountableValue() { value = m_CardQuantity } }
            }
            });
            GameEventHandler.Invoke(UnpackEventCode.OnUnpackStart, gachaCards, null, null);
            m_OnClaimCompleted.Invoke();
        });
    }

    public void Initialize(ActiveSkillSO activeSkillSO, Action onClaimCompleted)
    {
        m_ActiveSkillSO = activeSkillSO;
        m_OnClaimCompleted = onClaimCompleted;
        m_IconImage.sprite = activeSkillSO.GetThumbnailImage();
        m_CardQuantityText.SetText(m_CardQuantityText.blueprintText.Replace(Const.StringValue.PlaceholderValue, m_CardQuantity.ToString()));
    }
}