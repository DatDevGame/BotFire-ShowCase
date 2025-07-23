using HyrphusQ.Events;
using LatteGames;
using UnityEngine;
using UnityEngine.UI;

public class EventTicketPopup : Singleton<EventTicketPopup>
{
    [SerializeField] Button closeBtn;
    CanvasGroupVisibility canvasGroupVisibility;

    private void Awake()
    {
        canvasGroupVisibility = GetComponent<CanvasGroupVisibility>();
        closeBtn.onClick.AddListener(OnClose);
    }

    public void Show()
    {
        canvasGroupVisibility.Show();
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