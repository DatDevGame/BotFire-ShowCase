using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PBPvPStatUI : MonoBehaviour
{
    [SerializeField] PBStatID statID;
    [SerializeField] TMP_Text playerStat;
    [SerializeField] TMP_Text opponentStat;
    [SerializeField] Image statIcon;
    [SerializeField] Color higherStatColor;
    [SerializeField] Color lowerStatColor;
    [SerializeField] PBStatIconMappingSO statIconMappingSO;

    float playerStatValue;
    float opponentStatValue;

    public PBStatID StatID => statID;

    private void Awake()
    {
        statIcon.sprite = statIconMappingSO.GetStatIcon(statID);
    }

    public float PlayerStatValue
    {
        set
        {
            playerStatValue = value;
            UpdateView();
        }
    }

    public float OpponentStatValue
    {
        set
        {
            opponentStatValue = value;
            UpdateView();
        }
    }

    void UpdateView()
    {
        var isPlayerStatHigher = playerStatValue >= opponentStatValue; //For checking two stat is equal
        var isOpponentStatHigher = opponentStatValue >= playerStatValue; //For checking two stat is equal
        playerStat.color = isPlayerStatHigher ? higherStatColor : lowerStatColor;
        opponentStat.color = isOpponentStatHigher ? higherStatColor : lowerStatColor;
        playerStat.text = playerStatValue.RoundToInt().ToRoundedText();
        opponentStat.text = opponentStatValue.RoundToInt().ToRoundedText();
        
    }
}
