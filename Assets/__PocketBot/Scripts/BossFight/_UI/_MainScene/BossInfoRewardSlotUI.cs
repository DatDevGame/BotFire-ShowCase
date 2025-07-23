using System.Collections;
using System.Collections.Generic;
using LatteGames.GameManagement;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossInfoRewardSlotUI : MonoBehaviour
{
    [SerializeField] TMP_Text currencyTxt;
    [SerializeField] Image rewardIconImg, tickImg;

    public void Setup(Sprite icon, float amount, bool isCurrency)
    {
        rewardIconImg.sprite = icon;
        currencyTxt.text = $"{(isCurrency ? "+" : "x")}{amount}";
    }

    public void SetClaimed(bool isClaimed)
    {
        tickImg.gameObject.SetActive(isClaimed);
    }
}
