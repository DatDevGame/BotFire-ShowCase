using UnityEngine;
using LatteGames.PvP.TrophyRoad;
using System.Collections.Generic;
using HyrphusQ.Helpers;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;
using TMPro;
using GachaSystem.Core;
using HyrphusQ.Events;
using Sirenix.OdinInspector;

public class PBTrophyRoadRewardUI : TrophyRoadRewardUI
{
    [SerializeField] Sprite claimedBGSprite, claimableBGSprite, lockedBGSprite;
    [SerializeField] Image leftBoxImg, rightBoxImg;

    [SerializeField, BoxGroup("Data")] private PBTrophyRoadSO m_PBTrophyRoadSO;

    protected GachaPack gachaPack;

    public override void Setup(TrophyRoadSO.Milestone milestone)
    {
        gachaPack = null;
        foreach (var item in milestone.reward.generalItems)
        {
            if (item.Key is GachaPack gachaPackKey)
            {
                gachaPack = gachaPackKey;
                break;
            }
        }
        if (gachaPack != null)
        {
            leftBoxImg.sprite = gachaPack.GetThumbnailImage();
            rightBoxImg.sprite = gachaPack.GetThumbnailImage();
        }
        base.Setup(milestone);
    }

    public override void UpdateUIImmediately()
    {
        base.UpdateUIImmediately();
        leftBoxImg.transform.parent.gameObject.SetActive(gachaPack != null && !Claimed);
        rightBoxImg.transform.parent.gameObject.SetActive(gachaPack != null && Claimed);
    }

    protected override bool TryGenerateRandomCardsInfo(KeyValuePair<ItemSO, ShopProductSO.DiscountableValue> randomItemSO, out RarityType rarity, out int count)
    {
        base.TryGenerateRandomCardsInfo(randomItemSO, out rarity, out count);
        if (randomItemSO.Key is GachaCard_RandomPart gachaCard_RandomPart)
        {
            rarity = gachaCard_RandomPart.Rarity;
            cards.AddRange(PBGachaCardGenerator.Instance.GenerateRepeat<GachaCard_Part>(count, gachaCard_RandomPart.PartSO));
            return true;
        }
        return false;
    }

    protected override void GenerateCardsUI()
    {
        base.GenerateCardsUI();
        // Move the first cards to the front
        foreach (var anchor in leftCardAnchors)
        {
            anchor.SetSiblingIndex(0);
        }
        if (leftCardAnchorBase.TryGetComponentInParent<HorizontalLayoutGroup>(out var leftLayoutGroup))
        {
            leftLayoutGroup.reverseArrangement = true;
        }
        foreach (var anchor in rightCardAnchors)
        {
            anchor.SetSiblingIndex(0);
        }
        if (rightCardAnchorBase.TryGetComponentInParent<HorizontalLayoutGroup>(out var rightLayoutGroup))
        {
            rightLayoutGroup.reverseArrangement = true;
        }
    }

    protected override IEnumerator CRPlayUpdatingUIAnimation(float duration)
    {
        ClearTweens();

        var claimable = Unlocked && !Claimed;

        // Lerp tween
        var oldBackgroundImageColor = backgroundImage.color;
        var oldLeftBackgroundImageColor = leftBackgroundImage.color;
        var t = 0f;
        var tween = DOTween.To(() => t, value => t = value, 1f, duration).OnUpdate(tweenUpdate);
        void tweenUpdate()
        {
            var bgSprite = Claimed ? claimedBGSprite : (Unlocked ? claimableBGSprite : lockedBGSprite);
            backgroundImage.sprite = bgSprite;
            // backgroundImage.color = Color.Lerp(oldBackgroundImageColor, colorSetup.backgroundColor, t);
            // leftBackgroundImage.color = Color.Lerp(oldLeftBackgroundImageColor, colorSetup.leftColor, t);
            foreach (var cardController in cardControllers)
            {
                foreach (var img in cardController.GetComponentsInChildren<Image>())
                {
                    if (img.sprite == null) continue;
                    if (img.TryGetComponentInParent<TextMeshProUGUI>(out _)) continue;
                    img.color = Color.Lerp(lockedCardColor, Color.white, Unlocked ? t : (1f - t));
                }
            }
            leftBoxImg.color = Color.Lerp(lockedCardColor, Color.white, Unlocked ? t : (1f - t));
            lockImage.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, !Unlocked ? t : (1f - t));
            claimText.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, claimable ? t : (1f - t));
        }
        tweens.Add(tween);
        yield return new WaitWhile(tween.IsActive);

        // Update other views immediately
        canvasGroup.interactable = claimable;
        checkImage.enabled = Claimed;
        var cardAnchors = checkImage.enabled ? rightCardAnchors : leftCardAnchors;
        for (int i = 0; i < cardControllers.Count; i++)
        {
            var cardTransform = cardControllers[i].transform;
            cardTransform.SetParent(cardAnchors[i]);
            cardTransform.localPosition = Vector3.zero;
            cardTransform.localRotation = Quaternion.identity;
            cardTransform.localScale = cardScale * Vector3.one;
        }

        // FTUE
        if (claimable && PPrefFTUEClaimReward.value == false)
        {
            handInstance = Instantiate(tutorialHandPrefab, handContainer);
            handInstance.runtimeAnimatorController = animatorController;
            PPrefFTUEClaimReward.value = true;
        }
        else if (!claimable && handInstance != null)
        {
            Destroy(handInstance.gameObject);
        }
    }

    /// <summary>
    /// Need a Button or an EventTrigger component to call this
    /// </summary>
    public virtual void PBClaim()
    {
        if (milestone == null) return;
        if (!milestone.Unlocked) return;
        if (milestone.Claimed) return;
        foreach (var card in cards)
        {
            if (card is GachaCard_Currency gachaCard_Currency)
            {
                gachaCard_Currency.ResourceLocationProvider = new ResourceLocationProvider(ResourceLocation.TrophyRoad, m_PBTrophyRoadSO.GetCurrentMilestoneIndex().ToString());
            }
        }

        if (gachaPack != null)
        {
            string openType = "free";
            GameEventHandler.Invoke(LogFirebaseEventCode.BoxOpen, gachaPack, openType);
        }

        GameEventHandler.Invoke(UnpackEventCode.OnUnpackStart, cards, gachaPack == null ? null : new List<GachaPack> { gachaPack }, null);
        GameEventHandler.Invoke(PBTrophyRoadEventCode.OnClaimReward);
        milestone.Claimed = true;
    }
}
