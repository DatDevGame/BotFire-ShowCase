using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CheatResetLinkRewards : MonoBehaviour
{
    public Button Button;
    public PBLinkRewardManagerSO PBLinkRewardManagerSO;

    private void Awake()
    {
        Button.onClick.AddListener(() =>
        {
            PBLinkRewardManagerSO.ResetNow();
        });
    }
}
