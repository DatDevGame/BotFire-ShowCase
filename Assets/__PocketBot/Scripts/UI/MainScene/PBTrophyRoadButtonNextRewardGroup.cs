using System.Collections;
using System.Collections.Generic;
using GachaSystem.Core;
using HyrphusQ.SerializedDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class PBTrophyRoadButtonNextRewardGroup : MonoBehaviour
{
    [SerializeField, BoxGroup("Ref")] protected Image currencyNextReward;
    [SerializeField, BoxGroup("Ref")] protected Image partNextReward;
    [SerializeField, BoxGroup("Ref")] protected Image partNextRewardRarityOutline;
    [SerializeField, BoxGroup("Ref")] protected Image partNextRewardIcon;
    [SerializeField, BoxGroup("Ref")] protected Image boxNextReward;
    [SerializeField, BoxGroup("Ref")] protected Image unknownNextReward;
    [SerializeField, BoxGroup("Ref")] protected SerializedDictionary<RarityType, Material> rarityMaterials;
    [SerializeField, BoxGroup("Ref")] protected SerializedDictionary<CurrencyType, Sprite> currencySprites;

    public void SetReward()
    {
        Show(unknownNextReward);
    }

    public void SetReward(PBPartSO partSO)
    {
        Show(partNextReward);
        partNextRewardRarityOutline.material = rarityMaterials[partSO.GetRarityType()];
        partNextRewardIcon.sprite = partSO.GetThumbnailImage();
    }

    public void SetReward(GachaPack gachaPack)
    {
        Show(boxNextReward);
        boxNextReward.sprite = gachaPack.GetThumbnailImage();
    }

    public void SetReward(CurrencyType currencyType)
    {
        Show(boxNextReward);
        boxNextReward.sprite = currencySprites[currencyType];
    }
    
    void Show(Image nextReward)
    {
        unknownNextReward.gameObject.SetActive(false);
        currencyNextReward.gameObject.SetActive(false);
        partNextReward.gameObject.SetActive(false);
        boxNextReward.gameObject.SetActive(false);
        nextReward.gameObject.SetActive(true);
    }
}
