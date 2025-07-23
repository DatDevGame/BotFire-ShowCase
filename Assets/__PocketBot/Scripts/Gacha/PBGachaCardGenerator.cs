using UnityEngine;
using GachaSystem.Core;
using System.Collections.Generic;
using System;
using System.Linq;

public class PBGachaCardGenerator : GachaCardGenerator
{
    public override List<GachaCard> Generate(ShopProductSO shopProductSO)
    {
        var result = new List<GachaCard>();
        result.AddRange(GenerateCurrencyCards(shopProductSO));
        result.AddRange(GenerateGeneralItemSOCards(shopProductSO));
        return result;
    }

    private IEnumerable<GachaCard> GenerateCurrencyCards(ShopProductSO shopProductSO)
    {
        if (shopProductSO.currencyItems == null) yield break;
        foreach (var item in shopProductSO.currencyItems)
        {
            yield return cardTemplates.Generate<GachaCard_Currency>(Mathf.RoundToInt(item.Value.value), item.Key);
        }
    }

    private List<GachaCard> GenerateGeneralItemSOCards(ShopProductSO shopProductSO)
    {
        if (shopProductSO.generalItems == null)
            return new List<GachaCard>();
        var gachaItemCards = new List<GachaCard>();
        foreach (var item in shopProductSO.generalItems)
        {
            if (item.Key is PBPartSO partSO)
            {
                gachaItemCards.AddRange(GenerateRepeat<GachaCard_Part>(item.Value.value.RoundToInt(), partSO));
            }
            else if (item.Key is SkinSO skinSO)
            {
                gachaItemCards.AddRange(GenerateRepeat<GachaCard_Skin>(item.Value.value.RoundToInt(), skinSO));
            }
            else if (item.Key is PBPartCardsSO partCardsSO)
            {
                gachaItemCards.AddRange(partCardsSO.GenerateCards(item.Value.value.RoundToInt()));
            }
            else if (item.Key is ActiveSkillSO activeSkillSO)
            {
                gachaItemCards.AddRange(GenerateRepeat<GachaCard_ActiveSkill>(item.Value.value.RoundToInt(), activeSkillSO));
            }
            else if (item.Key is GachaCard_RandomActiveSkill gachaCard_RandomActiveSkill)
            {
                for (int i = 0; i < item.Value.value.RoundToInt(); i++)
                {
                    gachaItemCards.AddRange(GenerateRepeat<GachaCard_ActiveSkill>(1, gachaCard_RandomActiveSkill.GetRandomActiveSkillSO()));
                }
            }
        }
        return gachaItemCards;
    }

    public List<GachaCard> Generate(RewardGroupInfo rewardGroupInfo)
    {
        var result = new List<GachaCard>();
        result.AddRange(GenerateCurrencyCards(rewardGroupInfo));
        result.AddRange(GenerateGeneralItemSOCards(rewardGroupInfo));
        return result;
    }

    private IEnumerable<GachaCard> GenerateCurrencyCards(RewardGroupInfo rewardGroupInfo)
    {
        if (rewardGroupInfo.currencyItems == null) yield break;
        foreach (var item in rewardGroupInfo.currencyItems)
        {
            yield return cardTemplates.Generate<GachaCard_Currency>(Mathf.RoundToInt(item.Value.value), item.Key);
        }
    }

    private List<GachaCard> GenerateGeneralItemSOCards(RewardGroupInfo rewardGroupInfo)
    {
        if (rewardGroupInfo.generalItems == null)
            return new List<GachaCard>();
        var gachaItemCards = new List<GachaCard>();
        foreach (var item in rewardGroupInfo.generalItems)
        {
            if (item.Key is PBPartSO partSO)
            {
                gachaItemCards.AddRange(GenerateRepeat<GachaCard_Part>(item.Value.value.RoundToInt(), partSO));
            }
            else if (item.Key is SkinSO skinSO)
            {
                gachaItemCards.AddRange(GenerateRepeat<GachaCard_Skin>(item.Value.value.RoundToInt(), skinSO));
            }
            else if (item.Key is PBPartCardsSO partCardsSO)
            {
                gachaItemCards.AddRange(partCardsSO.GenerateCards(item.Value.value.RoundToInt()));
            }
            else if (item.Key is ActiveSkillSO activeSkillSO)
            {
                gachaItemCards.AddRange(GenerateRepeat<GachaCard_ActiveSkill>(item.Value.value.RoundToInt(), activeSkillSO));
            }
            else if (item.Key is GachaCard_RandomActiveSkill gachaCard_RandomActiveSkill)
            {
                for (int i = 0; i < item.Value.value.RoundToInt(); i++)
                {
                    gachaItemCards.AddRange(GenerateRepeat<GachaCard_ActiveSkill>(1, gachaCard_RandomActiveSkill.GetRandomActiveSkillSO()));
                }
            }
        }
        return gachaItemCards;
    }

    public void GenerateRewards(List<RewardGroupInfo> rewardGroupInfos, out List<GachaCard> gachaCards, out List<GachaPack> gachaPacks)
    {
        var resultGachaCards = new List<GachaCard>();
        var resultGachaPacks = new List<GachaPack>();

        RewardGroupInfo currencyRewardGroupInfo = new RewardGroupInfo();
        currencyRewardGroupInfo.currencyItems = new();
        RewardGroupInfo skinRewardGroupInfo = new RewardGroupInfo();
        skinRewardGroupInfo.generalItems = new();
        RewardGroupInfo preBuildBotRewardGroupInfo = new RewardGroupInfo();
        preBuildBotRewardGroupInfo.generalItems = new();
        RewardGroupInfo partRewardGroupInfo = new RewardGroupInfo();
        partRewardGroupInfo.generalItems = new();

        foreach (var reward in rewardGroupInfos)
        {
            if (reward.currencyItems != null && reward.currencyItems.Count > 0)
            {
                foreach (var item in reward.currencyItems)
                {
                    if (!currencyRewardGroupInfo.currencyItems.ContainsKey(item.Key))
                    {
                        currencyRewardGroupInfo.currencyItems.Add(item.Key, new ShopProductSO.DiscountableValue() { value = 0, originalValue = 0 });
                    }
                    currencyRewardGroupInfo.currencyItems[item.Key].value += item.Value.value;
                    currencyRewardGroupInfo.currencyItems[item.Key].originalValue = currencyRewardGroupInfo.currencyItems[item.Key].value;
                }
            }
            if (reward.generalItems != null && reward.generalItems.Count > 0)
            {
                foreach (var item in reward.generalItems)
                {
                    if (item.Key is SkinSO skinSO)
                    {
                        if (!skinRewardGroupInfo.generalItems.ContainsKey(item.Key))
                        {
                            skinRewardGroupInfo.generalItems.Add(item.Key, new ShopProductSO.DiscountableValue() { value = 0, originalValue = 0 });
                        }
                        skinRewardGroupInfo.generalItems[item.Key].value += item.Value.value;
                        skinRewardGroupInfo.generalItems[item.Key].originalValue = skinRewardGroupInfo.generalItems[item.Key].value;
                    }
                    else if (item.Key is PBChassisSO chassisSO && chassisSO.IsSpecial)
                    {
                        if (!preBuildBotRewardGroupInfo.generalItems.ContainsKey(item.Key))
                        {
                            preBuildBotRewardGroupInfo.generalItems.Add(item.Key, new ShopProductSO.DiscountableValue() { value = 0, originalValue = 0 });
                        }
                        preBuildBotRewardGroupInfo.generalItems[item.Key].value += item.Value.value;
                        preBuildBotRewardGroupInfo.generalItems[item.Key].originalValue = preBuildBotRewardGroupInfo.generalItems[item.Key].value;
                    }
                    else if (item.Key is PBPartSO partSO)
                    {
                        if (!partRewardGroupInfo.generalItems.ContainsKey(item.Key))
                        {
                            partRewardGroupInfo.generalItems.Add(item.Key, new ShopProductSO.DiscountableValue() { value = 0, originalValue = 0 });
                        }
                        partRewardGroupInfo.generalItems[item.Key].value += item.Value.value;
                        partRewardGroupInfo.generalItems[item.Key].originalValue = partRewardGroupInfo.generalItems[item.Key].value;
                    }
                    else if (item.Key is PBPartCardsSO partCardSO)
                    {
                        if (!partRewardGroupInfo.generalItems.ContainsKey(item.Key))
                        {
                            partRewardGroupInfo.generalItems.Add(item.Key, new ShopProductSO.DiscountableValue() { value = 0, originalValue = 0 });
                        }
                        partRewardGroupInfo.generalItems[item.Key].value += item.Value.value;
                        partRewardGroupInfo.generalItems[item.Key].originalValue = partRewardGroupInfo.generalItems[item.Key].value;
                    }
                    else if (item.Key is GachaPack gachaPack)
                    {
                        resultGachaPacks.Add(gachaPack);
                    }
                    else if (item.Key is ActiveSkillSO activeSkillSO)
                    {
                        if (!partRewardGroupInfo.generalItems.ContainsKey(item.Key))
                        {
                            partRewardGroupInfo.generalItems.Add(item.Key, new ShopProductSO.DiscountableValue() { value = 0, originalValue = 0 });
                        }
                        partRewardGroupInfo.generalItems[item.Key].value += item.Value.value;
                        partRewardGroupInfo.generalItems[item.Key].originalValue = partRewardGroupInfo.generalItems[item.Key].value;
                    }
                }
            }
        }

        var order = new List<CurrencyType> { CurrencyType.Standard, CurrencyType.Premium, CurrencyType.RVTicket };
        currencyRewardGroupInfo.currencyItems = currencyRewardGroupInfo.currencyItems.OrderBy(pair => order.IndexOf(pair.Key)).ToDictionary(pair => pair.Key, pair => pair.Value);
        resultGachaCards.AddRange(GenerateCurrencyCards(currencyRewardGroupInfo));
        resultGachaCards.AddRange(GenerateGeneralItemSOCards(skinRewardGroupInfo));
        resultGachaCards.AddRange(GenerateGeneralItemSOCards(preBuildBotRewardGroupInfo));
        resultGachaCards.AddRange(GenerateGeneralItemSOCards(partRewardGroupInfo));

        gachaCards = resultGachaCards;
        gachaPacks = resultGachaPacks.Count > 0 ? resultGachaPacks : null;
    }
}

public enum RewardType
{
    Coins,
    Gems,
    Skip_Ads,
    Part_Cards,
    Classic_Boxes,
    Great_Boxes,
    Ultra_Boxes,
    Skins,
    Prebuilt_bots,
    SkillCard
}

[Serializable]
public struct Reward
{
    public RewardType type;
    public float amount;
}