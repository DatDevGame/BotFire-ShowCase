using UnityEngine;
using UnityEngine.UI;
using LatteGames.PvP.TrophyRoad;
using TMPro;
using LatteGames.PvP;

public class PBTrophyRoadArenaBannerUI : TrophyRoadArenaBannerUI
{
    [SerializeField] Image darkLayer;
    [SerializeField] TextMeshProUGUI commingSoonText;
    [SerializeField] TextMeshProUGUI nameArenaText;

    PvPArenaSO arenaSO;

    public override void SetLocked(bool isLocked)
    {
        base.SetLocked(isLocked);
        darkLayer.enabled = isLocked || arenaSO is PBEmptyPvPArenaSO;
    }

    public override void Setup(float height, PvPArenaSO arenaSO)
    {
        this.arenaSO = arenaSO;

        var rectTransform = GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, height);
        arenaBackgroundImage.sprite = arenaSO.GetThumbnailImage();
        infoBackgroundImage.sprite = (arenaSO as  PBPvPArenaSO).BoardSpriteArena;
        float screenWidth = Screen.width;
        //arenaBackgroundImage.rectTransform.localScale = Vector3.one * (screenWidth / 1000);
        arenaIndexText.text = (arenaSO.index + 1).ToString();
        commingSoonText.gameObject.SetActive(arenaSO is PBEmptyPvPArenaSO);
        if (arenaSO.TryGetModule<NameItemModule>(out var nameModule))
        {
            nameArenaText.SetText(nameModule.displayName);
        }

        var requirements = arenaSO.GetUnlockRequirements();
        if (requirements == null)
        {
            requiredAmountText.transform.parent.gameObject.SetActive(false);
            return;
        }
        foreach (var req in requirements)
        {
            if (req is Requirement_Currency requirement_Currency && requirement_Currency.currencyType == CurrencyType.Medal)
            {
                requiredAmountText.text = requiredAmountText.text.Replace("{value}", $"{requirement_Currency.requiredAmountOfCurrency}");
            }
        }
    }
}