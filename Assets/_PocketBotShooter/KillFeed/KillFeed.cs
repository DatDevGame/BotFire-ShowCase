using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using UnityEngine;
using DG.Tweening; // For DOTween

public class KillFeed : MonoBehaviour
{
    [Header("Kill Feed Settings")]
    public GameObject killFeedEntryPrefab; // Assign your KillFeedEntry prefab in the inspector
    public Transform feedParent; // Assign a UI parent (e.g., VerticalLayoutGroup)
    public float feedDisplayDuration = 2.5f;
    public float feedFadeDuration = 0.4f;
    public int maxFeedsOnScreen = 2;

    private Queue<KillFeedData> feedQueue = new Queue<KillFeedData>();
    private List<GameObject> activeFeeds = new List<GameObject>();
    private bool isProcessingQueue = false;

    private void Awake()
    {
        GameEventHandler.AddActionEvent(KillingSystemEvent.OnKillDeathInfosUpdated, OnKillDeathInfosUpdated);
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(KillingSystemEvent.OnKillDeathInfosUpdated, OnKillDeathInfosUpdated);
    }

    void OnKillDeathInfosUpdated(object[] parameters)
    {
        var killer = parameters[1] as PBRobot;
        var victim = parameters[2] as PBRobot;
        if (killer != null && victim != null)
        {
            feedQueue.Enqueue(new KillFeedData(killer, victim));
            ProcessQueue();
        }
    }

    void ProcessQueue()
    {
        if (isProcessingQueue) return;
        StartCoroutine(ProcessQueueCoroutine());
    }

    IEnumerator ProcessQueueCoroutine()
    {
        isProcessingQueue = true;
        while (feedQueue.Count > 0 && activeFeeds.Count < maxFeedsOnScreen)
        {
            var data = feedQueue.Dequeue();
            ShowFeed(data);
            yield return new WaitForSeconds(0.2f); // Small delay between feeds
        }
        isProcessingQueue = false;
    }

    void ShowFeed(KillFeedData data)
    {
        GameObject entry = Instantiate(killFeedEntryPrefab, feedParent);
        KillFeedEntry entryScript = entry.GetComponent<KillFeedEntry>();
        entryScript.SetData(data.killer, data.victim);
        activeFeeds.Add(entry);
        // Animate in
        CanvasGroup cg = entry.GetComponent<CanvasGroup>();
        if (cg == null) cg = entry.AddComponent<CanvasGroup>();
        cg.alpha = 0;
        cg.DOFade(1, feedFadeDuration);
        cg.transform.localScale = Vector3.zero;
        cg.transform.DOScale(Vector3.one, feedFadeDuration);
        // Remove after duration
        StartCoroutine(HideFeedAfterDelay(entry, feedDisplayDuration));
    }

    IEnumerator HideFeedAfterDelay(GameObject entry, float delay)
    {
        yield return new WaitForSeconds(delay);
        CanvasGroup cg = entry.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.transform.DOScale(Vector3.zero, feedFadeDuration);
            cg.DOFade(0, feedFadeDuration).SetEase(Ease.Linear).OnComplete(() => {
                activeFeeds.Remove(entry);
                Destroy(entry);
                ProcessQueue();
            });
        }
        else
        {
            activeFeeds.Remove(entry);
            Destroy(entry);
            ProcessQueue();
        }
    }

    // Helper data struct
    public struct KillFeedData
    {
        public PBRobot killer;
        public PBRobot victim;
        public KillFeedData(PBRobot killer, PBRobot victim)
        {
            this.killer = killer;
            this.victim = victim;
        }
    }
}