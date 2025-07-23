using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CheatResetHotOffers : MonoBehaviour
{
    public Button Button;
    public HotOffersManagerSO HotOffersManagerSO;

    private void Awake()
    {
        Button.onClick.AddListener(() =>
        {
            HotOffersManagerSO.ResetNow();
        });
    }
}
