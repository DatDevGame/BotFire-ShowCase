using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnreadUI : MonoBehaviour
{
    [SerializeField] GameObject newTag, dotTag;
    [SerializeField] List<UnreadLocation> unreadLocations;

    private void Awake()
    {
        UnreadManager.Instance.OnUnreadTypeUpdated += OnUnreadTypeUpdated;
        UpdateView();
    }

    private void OnDestroy()
    {
        if (UnreadManager.Instance != null)
        {
            UnreadManager.Instance.OnUnreadTypeUpdated -= OnUnreadTypeUpdated;
        }
    }

    void OnUnreadTypeUpdated()
    {
        UpdateView();
    }

    void UpdateView()
    {
        var result = UnreadType.None;
        foreach (var unreadLocation in unreadLocations)
        {
            if (UnreadManager.Instance.GetUnreadType(unreadLocation) == UnreadType.New)
            {
                result = UnreadType.New;
                break;
            }
            else if (UnreadManager.Instance.GetUnreadType(unreadLocation) == UnreadType.Dot)
            {
                result = UnreadType.Dot;
                break;
            }
        }
        newTag.SetActive(result == UnreadType.New);
        dotTag.SetActive(result == UnreadType.Dot);
    }
}


