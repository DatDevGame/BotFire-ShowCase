using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveAdsShopSection : MonoBehaviour
{
    [SerializeField] PPrefBoolVariable removeAds;
    [SerializeField] GameObject UnpurchasedGroup, purchasedGroup;

    IEnumerator Start()
    {
        purchasedGroup.SetActive(removeAds.value);
        UnpurchasedGroup.SetActive(!removeAds.value);
        yield return new WaitUntil(() => removeAds.value);
        purchasedGroup.SetActive(removeAds.value);
        UnpurchasedGroup.SetActive(!removeAds.value);
    }
}
