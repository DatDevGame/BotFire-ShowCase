using System.Collections;
using System.Collections.Generic;
using GachaSystem.Core;
using UnityEngine;
using UnityEngine.UI;

public class CheatLeagueEndDivisionNow : MonoBehaviour
{
    [SerializeField] DayBasedRewardSO _timeLeftUntilEndDivision;
    [SerializeField] Button _button;
    private void Awake()
    {
        _button.onClick.AddListener(() =>
        {
            _timeLeftUntilEndDivision.CoolDownInterval = 1;
        });
    }
}
