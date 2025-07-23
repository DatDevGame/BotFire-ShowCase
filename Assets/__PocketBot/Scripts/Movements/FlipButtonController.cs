using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using HyrphusQ.Events;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FlipButtonController : MonoBehaviour
{
    [SerializeField] Button flipButton;
    [SerializeField] float flipPrice = 50;
    [SerializeField] GrayscaleUI flipBtnGrayscaleUI;
    [SerializeField] TMP_Text flipPriceTxt;
    [SerializeField] CurrencyType priceCurrencyType;
    [SerializeField] private ModeVariable currentMode;
    CarPhysics player;

    private void Awake()
    {
        if (flipButton != null)
        {
            flipButton.onClick.AddListener(HandleFlipButtonClicked);
        }
        GameEventHandler.AddActionEvent(RobotStatusEventCode.OnModelSpawned, HandleBotModelSpawned);
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnResetBots, OnCarRecoveredFromImmobilized);
        flipPriceTxt.text = flipPriceTxt.text.Replace("{value}", flipPrice.ToString()).Replace("{sprite}", CurrencyManager.Instance.GetCurrencySO(priceCurrencyType).TMPSprite);
    }

    private void OnDestroy()
    {
        if (flipButton != null)
        {
            flipButton.onClick.RemoveListener(HandleFlipButtonClicked);
        }
        GameEventHandler.RemoveActionEvent(RobotStatusEventCode.OnModelSpawned, HandleBotModelSpawned);
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnResetBots, OnCarRecoveredFromImmobilized);
        if (player != null)
        {
            player.OnCarImmobilized -= OnCarImmobilized;
            player.OnCarRecoveredFromImmobilized -= OnCarRecoveredFromImmobilized;
        }
    }

    void HandleFlipButtonClicked()
    {
        if (CurrencyManager.Instance.IsAffordable(priceCurrencyType, flipPrice))
        {
            ResourceLocation resourceLocation = currentMode.value switch
            {
                Mode.Normal => ResourceLocation.SinglePvP,
                Mode.Boss => ResourceLocation.BossFight,
                Mode.Battle => ResourceLocation.BattlePvP,
                _ => ResourceLocation.None
            };

            CurrencyManager.Instance.Spend(priceCurrencyType, flipPrice, resourceLocation, "FlipButton");
            player.Flip();
            UpdateView();
            GameEventHandler.Invoke(AssistiveEventCode.Flip);
        }
    }

    void OnCarImmobilized()
    {
        UpdateView();
    }

    void UpdateView()
    {
        flipButton.gameObject.SetActive(true);
        if (CurrencyManager.Instance.IsAffordable(priceCurrencyType, flipPrice))
        {
            flipButton.interactable = true;
            flipBtnGrayscaleUI.SetGrayscale(false);
            flipButton.transform.DOKill();
            flipButton.transform.localScale = Vector3.one;
            flipButton.transform.DOScale(1.2f, AnimationDuration.TINY).OnComplete(() =>
            {
                flipButton.transform.DOScale(1, AnimationDuration.TINY);
            }).SetLoops(-1, LoopType.Yoyo);
        }
        else
        {
            flipButton.interactable = false;
            flipBtnGrayscaleUI.SetGrayscale(true);
            flipButton.transform.DOKill();
            flipButton.transform.localScale = Vector3.one;
        }
    }

    void OnCarRecoveredFromImmobilized()
    {
        flipButton.gameObject.SetActive(false);
    }

    void HandleBotModelSpawned(params object[] parameters)
    {
        if (parameters[0] is not PBRobot pbBot) return;
        if (pbBot.PersonalInfo.isLocal == false) return;
        if (parameters[1] is not GameObject chassis) return;
        if (player != null)
        {
            player.OnCarImmobilized -= OnCarImmobilized;
            player.OnCarRecoveredFromImmobilized -= OnCarRecoveredFromImmobilized;
        }

        player = chassis.GetComponent<CarPhysics>();

        player.OnCarImmobilized += OnCarImmobilized;
        player.OnCarRecoveredFromImmobilized += OnCarRecoveredFromImmobilized;
        flipButton.gameObject.SetActive(false);
    }
}
