using System;
using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using LatteGames.PvP;
using LatteGames.UI;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class PvPSurrenderButton : MonoBehaviour
{
    private Button m_Button;
    private Button button
    {
        get
        {
            if (m_Button == null)
                m_Button = GetComponent<Button>();
            return m_Button;
        }
    }

    private void Awake()
    {
        button.onClick.AddListener(OnSurrenderButtonClicked);
    }

    private void OnDestroy()
    {
        button.onClick.RemoveListener(OnSurrenderButtonClicked);
    }

    private void OnSurrenderButtonClicked()
    {
        MessageManager.Title = I2LHelper.TranslateTerm(I2LTerm.PvP_SurrenderPopup_Title);
        MessageManager.Message = I2LHelper.TranslateTerm(I2LTerm.PvP_SurrenderPopup_Message);
        MessageManager.PositiveText = I2LHelper.TranslateTerm(I2LTerm.PvP_SurrenderPopup_KeepPlaying);
        MessageManager.NegativeText = I2LHelper.TranslateTerm(I2LTerm.PvP_SurrenderPopup_Leave);
        MessageManager.Show(true, true);
        MessageManager.OnButtonClicked += OnButtonClicked;

        void OnButtonClicked(MessageManager.ClickedEventData eventData)
        {
            MessageManager.OnButtonClicked -= OnButtonClicked;
            if (!eventData.isPositive)
            {
                var matchManager = ObjectFindCache<PBPvPMatchManager>.Get();
                if (matchManager != null)
                    GameEventHandler.Invoke(PBPvPEventCode.OnLeaveInMiddleOfMatch, matchManager.GetCurrentMatchOfPlayer(), matchManager.GetCurrentMatchOfPlayer().GetOpponentInfo());
                var levelController = ObjectFindCache<PBLevelController>.Get();
                levelController.Surrender();
            }
        }
    }
}