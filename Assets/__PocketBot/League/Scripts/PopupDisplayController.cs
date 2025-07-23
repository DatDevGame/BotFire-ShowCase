using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LatteGames;

public class ShowPopupRequest
{
    private IUIVisibilityController m_Popup;

    public bool isShowImmediately { get; set; } = false;
    public IUIVisibilityController popup
    {
        get => m_Popup;
        set
        {
            if (value == null)
                return;
            m_Popup = value;
            m_Popup.GetOnStartShowEvent().Subscribe(OnStartShow);
            m_Popup.GetOnEndShowEvent().Subscribe(OnEndShow);
            m_Popup.GetOnStartHideEvent().Subscribe(OnStartHide);
            m_Popup.GetOnEndHideEvent().Subscribe(OnEndHide);
        }
    }
    public Action onStartShowCallback { get; set; } = delegate { };
    public Action onEndShowCallback { get; set; } = delegate { };
    public Action onStartHideCallback { get; set; } = delegate { };
    public Action onEndHideCallback { get; set; } = delegate { };

    private void OnStartShow()
    {
        onStartShowCallback?.Invoke();
        popup.GetOnStartShowEvent().Unsubscribe(OnStartShow);
    }

    private void OnEndShow()
    {
        onEndShowCallback?.Invoke();
        popup.GetOnEndShowEvent().Unsubscribe(OnEndShow);
    }

    private void OnStartHide()
    {
        onStartHideCallback?.Invoke();
        popup.GetOnStartHideEvent().Unsubscribe(OnStartHide);
    }

    private void OnEndHide()
    {
        onEndHideCallback?.Invoke();
        popup.GetOnEndHideEvent().Unsubscribe(OnEndHide);
    }

    public static ShowPopupRequest CreateRequest(IUIVisibilityController popup, bool isShowImmediately = false, Action onStartShowCallback = null, Action onEndShowCallback = null, Action onStartHideCallback = null, Action onEndHideCallback = null)
    {
        return new ShowPopupRequest()
        {
            popup = popup,
            isShowImmediately = isShowImmediately,
            onStartShowCallback = onStartShowCallback,
            onEndShowCallback = onEndShowCallback,
            onStartHideCallback = onStartHideCallback,
            onEndHideCallback = onEndHideCallback,
        };
    }
}

public class PopupDisplayController
{
    public event Action onCompleted = delegate { };

    private Queue<ShowPopupRequest> m_ShowPopupRequestQueue = new Queue<ShowPopupRequest>();

    private void ShowPopupRecursive()
    {
        if (m_ShowPopupRequestQueue.Count <= 0)
        {
            NotifyCompletedEvent();
            return;
        }

        ShowPopupRequest request = m_ShowPopupRequestQueue.Dequeue();
        if (!request.isShowImmediately)
            request.popup.Show();
        else
            request.popup.ShowImmediately();
        request.onStartHideCallback += OnPopupClosed;

        void OnPopupClosed()
        {
            request.onStartHideCallback -= OnPopupClosed;
            ShowPopupRecursive();
        }
    }

    public void Enqueue(ShowPopupRequest request)
    {
        m_ShowPopupRequestQueue.Enqueue(request);
    }

    public void Enqueue(IUIVisibilityController popup, bool isShowImmediately = false, Action onStartShowCallback = null, Action onEndShowCallback = null, Action onStartHideCallback = null, Action onEndHideCallback = null)
    {
        if (!UnityEngine.Object.Equals(popup, null))
            Enqueue(ShowPopupRequest.CreateRequest(popup, isShowImmediately, onStartShowCallback, onEndShowCallback, onStartHideCallback, onEndHideCallback));
    }

    public void ShowPopup(Action completedCallback = null)
    {
        if (completedCallback != null)
            onCompleted += completedCallback;
        ShowPopupRecursive();
    }

    public void NotifyCompletedEvent()
    {
        onCompleted?.Invoke();
    }

    public static PopupDisplayController Combine(PopupDisplayController popupControllerA, PopupDisplayController popupControllerB)
    {
        foreach (var request in popupControllerB.m_ShowPopupRequestQueue)
        {
            popupControllerA.Enqueue(request);
        }
        popupControllerA.onCompleted += OnCompleted;
        return popupControllerA;

        void OnCompleted()
        {
            popupControllerA.onCompleted -= OnCompleted;
            popupControllerB.NotifyCompletedEvent();
        }
    }
}