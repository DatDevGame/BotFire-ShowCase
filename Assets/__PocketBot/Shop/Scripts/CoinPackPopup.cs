using HyrphusQ.Events;
using LatteGames;
using UnityEngine;
using UnityEngine.UI;

public class CoinPackPopup : Singleton<CoinPackPopup>
{
    [SerializeField] Button closeBtn;
    CanvasGroupVisibility canvasGroupVisibility;

    [SerializeField] private HotOffersFreeCoinsUI m_HotOffersFreeCoinUI;

    private void Awake()
    {
        canvasGroupVisibility = GetComponent<CanvasGroupVisibility>();
        closeBtn.onClick.AddListener(OnClose);
    }

    public void Show()
    {
        canvasGroupVisibility.Show();
        // TODO: add resource event
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