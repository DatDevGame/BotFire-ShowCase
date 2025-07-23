using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using HyrphusQ.Events;

public class TrackingPackEvent : MonoBehaviour
{
    [SerializeField, BoxGroup("Ref")] private RectTransform m_Content;
    [SerializeField, BoxGroup("Ref")] private RectTransform m_ScrollViewViewport;
    [SerializeField, BoxGroup("Ref")] private RectTransform m_RectTransformHotOffersPopupUI;
    [SerializeField, BoxGroup("Ref")] private HotOffersPopupUI m_HotOffersPopupUI;

    private bool m_IsTrackHotOffer = false;
    private void Awake()
    {
        GameEventHandler.AddActionEvent(MainSceneEventCode.OnClickButtonMain, OnClickButtonMain);
        GameEventHandler.AddActionEvent(MainSceneEventCode.OnClickButtonShop, OnClickButtonShop);
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(MainSceneEventCode.OnClickButtonMain, OnClickButtonMain);
        GameEventHandler.RemoveActionEvent(MainSceneEventCode.OnClickButtonShop, OnClickButtonShop);
    }

    private void Update()
    {
        if (true)
            return;

        //HotOffer
        #region HotOffer
        if (m_IsTrackHotOffer)
        {
            if (m_HotOffersPopupUI == null || m_RectTransformHotOffersPopupUI == null) return;
            if (!IsRectTransformInViewport(m_RectTransformHotOffersPopupUI)) return;

            m_IsTrackHotOffer = false;
            m_HotOffersPopupUI.PackRewardUIs.ForEach((v) =>
            {
                if (v is HotOffersFreeCoinsUI hotOffersFreeCoinsUI)
                {
                    if (hotOffersFreeCoinsUI.PackRewardState == PackReward.PackRewardState.Ready)
                    {
                        #region Design Events
                        string offer = "FreeCoinAvailable";
                        string rvName = $"HotOffers_{offer}";
                        string location = "Shop";
                        GameEventHandler.Invoke(DesignEvent.RVShow);
                        #endregion
                    }
                }

                if (v is HotOffersFreeGemsUI hotOffersFreeGemsUI)
                {
                    if (hotOffersFreeGemsUI.PackRewardState == PackReward.PackRewardState.Ready)
                    {
                        #region Design Events
                        string offer = "FreeGemAvailable";
                        string rvName = $"HotOffers_{offer}";
                        string location = "Shop";
                        GameEventHandler.Invoke(DesignEvent.RVShow);
                        #endregion
                    }
                }

                if (v is HotOffersItemUI hotOffersItem)
                {
                    HotOffersItemSO hotOffersItemSO = hotOffersItem.HotOffersItemSO;
                    if (hotOffersItemSO == null) return;
                    if (hotOffersItemSO.ReducedValue.RequirementsPack == PackReward.RequirementsPack.Ads)
                    {
                        #region Design Events
                        if (hotOffersItem.PartCard == null) return;
                        string offer = hotOffersItem.PartCard.groupType switch
                        {
                            DrawCardProcedure.GroupType.NewAvailable => "NewGroup",
                            DrawCardProcedure.GroupType.Duplicate => "DuplicateGroup",
                            DrawCardProcedure.GroupType.InUsed => "InUsedGroup",
                            _ => "None"
                        };

                        string rvName = $"HotOffers_{offer}";
                        string location = "Shop";
                        GameEventHandler.Invoke(DesignEvent.RVShow);
                        #endregion
                    }
                }
            });
        }
        #endregion
    }

    private void OnClickButtonMain()
    {
        m_IsTrackHotOffer = false;
    }

    private void OnClickButtonShop()
    {
        m_IsTrackHotOffer = true;
    }

    private bool IsRectTransformInViewport(RectTransform target)
    {
        Vector3[] viewportCorners = new Vector3[4];
        Vector3[] targetCorners = new Vector3[4];

        m_ScrollViewViewport.GetWorldCorners(viewportCorners);
        target.GetWorldCorners(targetCorners);


        bool isInside = true;
        foreach (Vector3 corner in targetCorners)
        {
            if (corner.x < viewportCorners[0].x || corner.x > viewportCorners[2].x ||
                corner.y < viewportCorners[0].y || corner.y > viewportCorners[2].y)
            {
                isInside = false;
                break;
            }
        }
        return isInside;
    }
}
