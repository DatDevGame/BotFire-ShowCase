using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using Sirenix.OdinInspector;
using UnityEngine;

public class TrackingBossInfoEvent : MonoBehaviour
{
    [SerializeField, BoxGroup("Ref")] private RectTransform m_Content;
    [SerializeField, BoxGroup("Ref")] private RectTransform m_ScrollViewViewport;
    [SerializeField, BoxGroup("Ref")] private BossMapUI m_BossMapUI;

    private BossInfoNode m_CurrentBossInfoTrophyByPass;
    private bool m_IsTrackBossInfo = false;

    private void Awake()
    {
        GameEventHandler.AddActionEvent(BossFightEventCode.OnBossMapOpened, HandleOpened);
        GameEventHandler.AddActionEvent(BossFightEventCode.OnBossMapClosed, HandleClosed);
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(BossFightEventCode.OnBossMapOpened, HandleOpened);
        GameEventHandler.RemoveActionEvent(BossFightEventCode.OnBossMapClosed, HandleClosed);

    }

    private void HandleOpened()
    {

    }

    private void HandleClosed()
    {
        
    }

    private bool IsRectTransformInViewport(RectTransform target)
    {
        Vector3[] viewportCorners = new Vector3[4];
        Vector3[] targetCorners = new Vector3[4];

        m_ScrollViewViewport.GetWorldCorners(viewportCorners);
        target.GetWorldCorners(targetCorners);


        bool isInside = true;
        foreach (Vector3 corner in targetCorners)
        {
            if (corner.x < viewportCorners[0].x || corner.x > viewportCorners[2].x ||
                corner.y < viewportCorners[0].y || corner.y > viewportCorners[2].y)
            {
                isInside = false;
                break;
            }
        }
        return isInside;
    }
}
