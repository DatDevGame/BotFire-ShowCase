using System.Collections;
using System.Collections.Generic;
using LatteGames.Monetization;
using UnityEngine;

public class UnreadRVButtonTracker : MonoBehaviour
{
    public UnreadLocation unreadLocation;
    RVButtonBehavior _RVButtonBehavior;

    Coroutine coroutine;
    bool isInit;

    public void Init(UnreadLocation unreadLocation)
    {
        _RVButtonBehavior = GetComponent<RVButtonBehavior>();
        this.unreadLocation = unreadLocation;
        isInit = true;
    }

    private void OnEnable()
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }
        coroutine = StartCoroutine(CR_Tracking());
    }

    private void OnDisable()
    {
        if (UnreadManager.Instance != null)
        {
            UnreadManager.Instance.RemoveUnreadTag(UnreadType.Dot, unreadLocation, gameObject);
        }
    }

    private void OnDestroy()
    {
        if (UnreadManager.Instance != null)
        {
            UnreadManager.Instance.RemoveUnreadTag(UnreadType.Dot, unreadLocation, gameObject);
        }
    }

    IEnumerator CR_Tracking()
    {
        while (true)
        {
            yield return new WaitUntil(() => isInit && _RVButtonBehavior.interactable && _RVButtonBehavior.gameObject.activeInHierarchy);
            UnreadManager.Instance.AddUnreadTag(UnreadType.Dot, unreadLocation, gameObject);
            yield return new WaitUntil(() => isInit && (!_RVButtonBehavior.interactable || !_RVButtonBehavior.gameObject.activeInHierarchy));
            UnreadManager.Instance.RemoveUnreadTag(UnreadType.Dot, unreadLocation, gameObject);
        }
    }
}
