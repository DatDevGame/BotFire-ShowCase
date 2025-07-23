using LatteGames;
using UnityEngine;
using UnityEngine.UI;
using HyrphusQ.Events;

public class IAPGemPackPopup : Singleton<IAPGemPackPopup>
{
    [SerializeField] Button closeBtn;
    CanvasGroupVisibility canvasGroupVisibility;

    [SerializeField] private HotOffersFreeGemsUI m_HotOffersFreeGemsUI;

    private void Awake()
    {
        canvasGroupVisibility = GetComponent<CanvasGroupVisibility>();
        closeBtn.onClick.AddListener(OnClose);
    }

    public void Show()
    {
        canvasGroupVisibility.Show();

        #region Design Events
        if (m_HotOffersFreeGemsUI != null)
        {
            string type = m_HotOffersFreeGemsUI.PackRewardState == PackReward.PackRewardState.Ready ? "FreeGemAvailable" : "FreeGemUnavailable";
            string rvName = $"HotOffers_{type}";
            string location = "ResourcePopup";
            GameEventHandler.Invoke(DesignEvent.RVShow, rvName, location);
        }
        #endregion
        GameEventHandler.Invoke(MainSceneEventCode.HideMainUICurrency);
    }

    public void Hide()
    {
        canvasGroupVisibility.Hide();
        GameEventHandler.Invoke(MainSceneEventCode.ShowMainUICurrency);
    }

    void OnClose()
    {
        Hide();
    }
}