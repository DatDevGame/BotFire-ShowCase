using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using LatteGames;
using UnityEngine;
using UnityEngine.UI;

public class PvPDoubleTapReminder : MonoBehaviour
{
    [SerializeField] Button button;
    [SerializeField] EZAnimBase panelAnim, iconAnim;
    [SerializeField] float showingDuration;

    Coroutine delayToCloseReminderCoroutine;
    bool isShowing = false;
    public bool IsShowing
    {
        get { return isShowing; }
        private set
        {
            isShowing = value;
            if (isShowing)
            {
                panelAnim.Play();
            }
            else
            {
                panelAnim.InversePlay();
            }
            iconAnim.Play();
        }
    }

    private void Awake()
    {
        button.onClick.AddListener(OnButtonClick);
    }

    private void Start()
    {
        OnMatchStarted();
    }

    private void OnDestroy()
    {
        button.onClick.RemoveListener(OnButtonClick);
    }

    void OnButtonClick()
    {
        if (delayToCloseReminderCoroutine != null)
        {
            StopCoroutine(delayToCloseReminderCoroutine);
        }
        IsShowing = !IsShowing;
    }

    void OnMatchStarted()
    {
        IsShowing = true;
        delayToCloseReminderCoroutine = StartCoroutine(CommonCoroutine.Delay(showingDuration, false, () =>
        {
            if (isShowing)
            {
                IsShowing = false;
            }
        }));
    }
}
