using System.Collections;
using System.Collections.Generic;
using LatteGames.Monetization;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class UnreadNewShopSectionTracker : MonoBehaviour
{
    public static float READABLE_MAX_Y = 0.9f;
    public static float READABLE_MIN_Y = 0.1f;

    [SerializeField] UnreadLocation unreadLocation;

    LayoutElement layoutElement;
    Coroutine coroutine;
    protected virtual bool isNew
    {
        get
        {
            return PlayerPrefs.GetInt($"UnreadNewShopSectionTracker_{unreadLocation}", 1) == 1;
        }
        set
        {
            PlayerPrefs.SetInt($"UnreadNewShopSectionTracker_{unreadLocation}", value ? 1 : 0);
        }
    }
    PBDockController _dockController;
    protected PBDockController dockController
    {
        get
        {
            if (_dockController == null)
            {
                _dockController = FindObjectOfType<PBDockController>();
            }
            return _dockController;
        }
    }

    private void Awake()
    {
        layoutElement = GetComponent<LayoutElement>();
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
            UnreadManager.Instance.RemoveUnreadTag(UnreadType.New, unreadLocation, gameObject);
        }
    }

    private void OnDestroy()
    {
        if (UnreadManager.Instance != null)
        {
            UnreadManager.Instance.RemoveUnreadTag(UnreadType.New, unreadLocation, gameObject);
        }
    }

    IEnumerator CR_Tracking()
    {
        while (true)
        {
            yield return new WaitUntil(() => isNew && gameObject.activeInHierarchy && !layoutElement.ignoreLayout);
            UnreadManager.Instance.AddUnreadTag(UnreadType.New, unreadLocation, gameObject);
            yield return new WaitUntil(() => !gameObject.activeInHierarchy || layoutElement.ignoreLayout || (isNew && dockController != null && dockController.CurrentSelectedButtonType == ButtonType.Shop && IsInReadableRange() && gameObject.activeInHierarchy && !layoutElement.ignoreLayout));
            UnreadManager.Instance.RemoveUnreadTag(UnreadType.New, unreadLocation, gameObject);
            if (isNew && dockController != null && dockController.CurrentSelectedButtonType == ButtonType.Shop && IsInReadableRange())
            {
                isNew = false;
            }
        }
    }

    public bool IsInReadableRange()
    {
        return transform.position.y <= READABLE_MAX_Y * Screen.height && transform.position.y >= READABLE_MIN_Y * Screen.height;
    }

    [Button]
    public void ResetIsNew()
    {
        isNew = true;
    }
}
