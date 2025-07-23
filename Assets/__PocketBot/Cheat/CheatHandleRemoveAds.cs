using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CheatHandleRemoveAds : MonoBehaviour
{
    public bool IsEnableRemoveAds;
    public PPrefBoolVariable RemoveAdsData;
    public Button Button;


    private void Awake()
    {
        Button.onClick.AddListener(() =>
        {
            RemoveAdsData.value = IsEnableRemoveAds;
        });
    }
}
