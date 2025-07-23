using UnityEngine;
using LatteGames.PvP.TrophyRoad;
using UnityEngine.UI;
using System.Collections.Generic;

public class PBTrophyRoadFoundInCardsTabUI : TrophyRoadFoundInCardsTabUI<PBPartSO>
{
    [Header("PB PROPERTIES")]
    [SerializeField] Image scrollView;

    public override void SetExpanding(bool shouldExpand)
    {
        base.SetExpanding(shouldExpand);
        scrollView.enabled = shouldExpand;
    }

    protected override void GenerateFoundInCardUIs<T>(List<GachaItemManagerSO<T>> gachaItemManagerSOs)
    {
        foreach (var gachaItemManagerSO in gachaItemManagerSOs)
        {
            foreach (var gachaItem in gachaItemManagerSO.genericItems)
            {
                if (gachaItem.foundInArena != currentSection.arenaSO.index + 1) continue;
                if (gachaItem is PBPartSO && (gachaItem as PBPartSO).IsBossPart) continue;
                var cardUI = Instantiate(foundInCardUIPrefab, cardsContainer);
                cardUI.Setup(gachaItem);
                cardUIs.Add(cardUI);
            }
        }
    }
}
