using UnityEngine;
using LatteGames.PvP.TrophyRoad;
using HyrphusQ.SerializedDataStructure;
using UnityEngine.UI;
using System.Collections.Generic;

public class PBTrophyRoadFoundInCardUI : TrophyRoadFoundInCardUI
{
    [Header("PB PROPERTIES")]
    [SerializeField] SerializedDictionary<RarityType, Material> rarityMaterialTable = new();
    [SerializeField] SerializedDictionary<PBPartType, Sprite> partTypeSpriteTable = new();
    [SerializeField] Image rarityImage;
    [SerializeField] Image partTypeImage;
    [SerializeField] Image statIconImageBase;
    [SerializeField] Sprite atkSprite;
    [SerializeField] Sprite hpSprite;

    public override void Setup(GachaItemSO gachaItemSO)
    {
        base.Setup(gachaItemSO);
        rarityImage.material = rarityMaterialTable[gachaItemSO.GetRarityType()];
        if (gachaItemSO is PBPartSO partSO)
        {
            partTypeImage.sprite = partTypeSpriteTable[partSO.PartType];
            GenerateStatIcon(partSO.UpgradePath.attackUpgradeSteps, atkSprite);
            GenerateStatIcon(partSO.UpgradePath.hpUpgradeSteps, hpSprite);
        }
        else
        {
            partTypeImage.sprite = null;
        }
    }

    private void GenerateStatIcon(List<int> upgradeSteps, Sprite iconSprite)
    {
        if (upgradeSteps[0] <= 0) return;
        var statIconImage = Instantiate(statIconImageBase, statIconImageBase.transform.parent);
        statIconImage.sprite = iconSprite;
        statIconImage.gameObject.SetActive(true);
    }
}
