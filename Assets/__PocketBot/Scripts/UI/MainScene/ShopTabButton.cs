using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HyrphusQ.Events;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using LatteGames;
using DG.Tweening;

public class ShopTabButton : MonoBehaviour
{
    public static int SHOW_TROPHY_THRESHOLD = 130;
    const float HIGHLIGHT_TIME = 1.5f;
    [SerializeField] Button button;
    [SerializeField] Image icon;
    [SerializeField] Sprite lockSprite, shopSprite;
    [SerializeField] ParticleSystem shinningVFX;
    [SerializeField] HighestAchievedPPrefFloatTracker highestAchievedMedal;
    [SerializeField] PPrefBoolVariable firstTimeShowShopTab;

    PBDockController dockController;

    private void Awake()
    {
        button.interactable = true;
        icon.sprite = shopSprite;

        if (!firstTimeShowShopTab.value)
        {
            // button.interactable = false;
            // icon.sprite = lockSprite;
            dockController = FindObjectOfType<PBDockController>();
            StartCoroutine(CommonCoroutine.WaitUntil(() => dockController.CurrentSelectedButtonType == ButtonType.Main && highestAchievedMedal.value >= SHOW_TROPHY_THRESHOLD && LoadingScreenUI.IS_LOADING_COMPLETE, () =>
            {
                firstTimeShowShopTab.value = true;

                // GameEventHandler.Invoke(BlockBackGround.LockDarkHasObject, this.gameObject);
                // icon.transform.DOScale(0, AnimationDuration.TINY).SetDelay(AnimationDuration.TINY).SetEase(Ease.InBack).OnComplete(() =>
                // {
                //     icon.sprite = shopSprite;
                //     icon.transform.DOScale(1, AnimationDuration.SSHORT).SetEase(Ease.OutBack).OnComplete(() =>
                //     {
                //         shinningVFX.Play();
                //         StartCoroutine(CommonCoroutine.Delay(HIGHLIGHT_TIME, false, () =>
                //         {
                //             button.interactable = true;
                //             GameEventHandler.Invoke(BlockBackGround.LockDarkHasObject);
                //             shinningVFX.Stop();
                //         }));
                //     });
                // });
            }));
        }
    }
}
