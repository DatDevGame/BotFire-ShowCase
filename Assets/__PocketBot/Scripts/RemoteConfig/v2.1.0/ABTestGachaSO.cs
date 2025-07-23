using System;
using System.Collections;
using System.Collections.Generic;
using GachaSystem.Core;
using HyrphusQ.Helpers;
using HyrphusQ.SerializedDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
using static BaseGachaPack;
using static GachaSystem.Core.GachaPacksCollection;

[CreateAssetMenu(fileName = "ABTestGachaSO", menuName = "PocketBots/ABTest/v2.1.0/ABTestGachaSO")]
public class ABTestGachaSO : GroupBasedABTestSO
{
    [Serializable]
    public class ABTestGachaData
    {
        public ABTestGachaData(PBGachaPack gachaBox)
        {
            this.gachaBox = gachaBox;
            this.totalCardsCount = gachaBox.TotalCardsCount;
            this.moneyRange = new Vector2Int(Mathf.RoundToInt(gachaBox.moneyAmountRange.x), Mathf.RoundToInt(gachaBox.moneyAmountRange.y));
            this.gemRange = new Vector2Int(Mathf.RoundToInt(gachaBox.gemAmountRange.x), Mathf.RoundToInt(gachaBox.gemAmountRange.y));
            this.guaranteedEpicCardsCount = gachaBox.GetGuaranteedCardsCount(RarityType.Epic);
            this.guaranteedLegendaryCardsCount = gachaBox.GetGuaranteedCardsCount(RarityType.Legendary);
            this.commonRate = gachaBox.GetDropRateByRarity(RarityType.Common);
            this.epicRate = gachaBox.GetDropRateByRarity(RarityType.Epic);
            this.legendaryRate = gachaBox.GetDropRateByRarity(RarityType.Legendary);
            this.partDropRateTable = GetDropRateTableFromBox(gachaBox);

            SerializedDictionary<GearInfo, float> GetDropRateTableFromBox(PBGachaPack gachaBox)
            {
                SerializedDictionary<GearInfo, float> dropRateTable = new SerializedDictionary<GearInfo, float>();
                var gearDropRates = gachaBox.GetGearDropRates();
                foreach (var gearDropRate in gearDropRates)
                {
                    dropRateTable.Add(new GearInfo() { gearManagerSO = gearDropRate.PartSO.ManagerSO, gearSO = gearDropRate.PartSO }, gearDropRate.Probability);
                }
                dropRateTable.OnBeforeSerialize();
                return dropRateTable;
            }
        }

        public PBGachaPack gachaBox;
        public int totalCardsCount;
        public Vector2Int moneyRange;
        public Vector2Int gemRange;
        public int guaranteedEpicCardsCount;
        public int guaranteedLegendaryCardsCount;
        public float commonRate;
        public float epicRate;
        public float legendaryRate;
        public SerializedDictionary<GearInfo, float> partDropRateTable;

        public void InjectData()
        {
            gachaBox.SetTotalCards(totalCardsCount);

            gachaBox.SetMoneyRange(moneyRange.x, moneyRange.y);
            gachaBox.SetGemRange(gemRange.x, gemRange.y);

            gachaBox.SetGuaranteeAmount(guaranteedEpicCardsCount, guaranteedLegendaryCardsCount);

            gachaBox.SetGearRarityDropRate(commonRate, epicRate, legendaryRate);
            gachaBox.SetGearDropRateTable(partDropRateTable);
        }
    }
    [Serializable]
    public class ABTestGachaPacksCollectionData
    {
        public ABTestGachaPacksCollectionData(GachaPacksCollection packsCollection)
        {
            this.gachaPacksCollection = packsCollection;
            this.packRngInfos = new List<PackRngInfo>(packsCollection.PackRngInfos);
        }

        public GachaPacksCollection gachaPacksCollection;
        public List<PackRngInfo> packRngInfos = new List<PackRngInfo>();

        public void InjectData()
        {
            gachaPacksCollection.PackRngInfos.Clear();
            gachaPacksCollection.PackRngInfos.AddRange(packRngInfos);
        }
    }
    [Serializable]
    public class ABTestGachaDataGroup
    {
        [TableList]
        public List<ABTestGachaData> dataGachaBoxes;
        [TableList]
        public List<ABTestGachaPacksCollectionData> dataGachaBoxesCollections;

        public void InjectData()
        {
            dataGachaBoxes.ForEach(item => item.InjectData());
            dataGachaBoxesCollections.ForEach(item => item.InjectData());
        }

        public void RetrieveData(List<GachaPacksCollection> gachaPacksCollections, List<PBGachaPack> gachaPacks)
        {
            dataGachaBoxes.Clear();
            dataGachaBoxesCollections.Clear();
            foreach (var item in gachaPacks)
            {
                dataGachaBoxes.Add(new ABTestGachaData(item));
            }
            foreach (var item in gachaPacksCollections)
            {
                dataGachaBoxesCollections.Add(new ABTestGachaPacksCollectionData(item));
            }
        }
    }

    [SerializeField]
    private List<ABTestGachaDataGroup> groups;

    public override void InjectData(int groupIndex)
    {
        groups[groupIndex].InjectData();
    }

    public void RetrieveData(int groupIndex, List<GachaPacksCollection> gachaPacksCollections, List<PBGachaPack> gachaPacks)
    {
        groups[groupIndex].RetrieveData(gachaPacksCollections, gachaPacks);
    }
}