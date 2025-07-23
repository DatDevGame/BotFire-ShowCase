using System.Collections;
using System.Collections.Generic;
using LatteGames.Monetization;
using UnityEngine;
using UnityEngine.UI;

public class ClaimRVButton : MonoBehaviour
{
    public Button Button;
    public RVButtonBehavior RVButtonBehavior;
    public GrayscaleUI ClaimRVGrayscale;

    private void Awake()
    {
        if(Button == null)
            this.Button = gameObject.GetComponent<Button>();
    }

    private void Start()
    {
        UpdateGrayScale();
    }

    public void UpdateGrayScale()
    {
        RVButtonBehavior.interactable = AdsManager.Instance.IsReadyRewarded;
        if (ClaimRVGrayscale != null)
            ClaimRVGrayscale.SetGrayscale(!RVButtonBehavior.interactable);
    }
}
