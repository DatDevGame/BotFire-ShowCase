using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using LatteGames.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using LatteGames;
using Sirenix.OdinInspector;
using System;

public class BossMapUI : MonoBehaviour
{
    public List<BossInfoNode> BossInfoNodes => bossInfoNodes;

    [SerializeField] CanvasGroupVisibility visibility;
    // [SerializeField] Button previousChapterBtn, nextChapterBtn;
    // [SerializeField] TMP_Text chapterNameTxt;
    [SerializeField] Transform scrollViewContent;
    [SerializeField] BossInfoNode bossInfoNodePrefab;
    // [SerializeField] GameObject comingSoonBGImg;
    [SerializeField] GameObject scrollView;

    public bool isShowing = false;
    private bool isShowingBeforeUnpack = false;
    ObjectPooling<BossInfoNode> infoNodePooling;
    private List<BossInfoNode> bossInfoNodes = new List<BossInfoNode>();
    private bool isUIWarmUp = false;
    private Action uiWarmUpCb;

    [SerializeField] private Button _backButton;
    private int currentChapterIndex
    {
        get
        {
            return BossFightManager.Instance.selectingChapterIndex;
        }
        set
        {
            BossFightManager.Instance.selectingChapterIndex = value;
        }
    }
    BossMapSO bossMapSO => BossFightManager.Instance.bossMapSO;
    private int chapterAmount => BossFightManager.Instance.bossMapSO.chapterCount;

    [SerializeField] private RectTransform _content;
    private float _scrollDuration = 1f;
    private float _offsetSrollIndex = 435f;

    private void Awake()
    {
        GameEventHandler.AddActionEvent(BossFightEventCode.OnBossMapOpened, HandleOpened);
        GameEventHandler.AddActionEvent(BossFightEventCode.OnBossMapClosed, HandleClosed);
        GameEventHandler.AddActionEvent(UnpackEventCode.OnUnpackStart, OnUnpackStart);
        GameEventHandler.AddActionEvent(UnpackEventCode.OnUnpackDone, OnUnpackDone);
        // previousChapterBtn.onClick.AddListener(OnPreviousChapterClicked);
        // nextChapterBtn.onClick.AddListener(OnNextChapterClicked);

        //Setup object pool
        infoNodePooling = new ObjectPooling<BossInfoNode>(
            instantiateMethod: () =>
            {
                var infoNode = Instantiate(bossInfoNodePrefab, scrollViewContent);
                infoNode.transform.localScale = Vector3.one;
                return infoNode;
            },
            destroyMethod: (obj) => { Destroy(obj.gameObject); },
            preAddToPool: (obj) => obj.gameObject.SetActive(false),
            preLeavePool: obj => obj.gameObject.SetActive(true)
        )
        {
            PregenerateOffset = 1
        };

        _backButton.onClick.AddListener(() =>
        {
            Hide();
        });
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(BossFightEventCode.OnBossMapOpened, HandleOpened);
        GameEventHandler.RemoveActionEvent(BossFightEventCode.OnBossMapClosed, HandleClosed);
        GameEventHandler.RemoveActionEvent(UnpackEventCode.OnUnpackStart, OnUnpackStart);
        GameEventHandler.RemoveActionEvent(UnpackEventCode.OnUnpackDone, OnUnpackDone);

        if (isShowing)
        {
            #region Firebase Event
            GameEventHandler.Invoke(LogFirebaseEventCode.BossFightMenu);
            #endregion
        }
    }

    private void Start()
    {
        StartCoroutine(WarmupView());
    }
    
    private IEnumerator WarmupView()
    {
        isUIWarmUp = true;
        var currentChapterSO = BossFightManager.Instance.bossMapSO.chapterList[currentChapterIndex];
        for (int i = 0; i < currentChapterSO.bossCount; i++)
        {
            var infoNode = infoNodePooling.Get();
            bossInfoNodes.Add(infoNode);
            yield return null;
        }
        UpdateView();
        AutoScrollToIndex();
        isUIWarmUp = false;
    }

    private void AutoScrollToIndex()
    {
        int index = 0;
        for (int i = 0; i < bossInfoNodes.Count; i++)
        {
            if (bossMapSO.currentBossSO == bossInfoNodes[i].BossSO)
                index = _content.GetChild(i).GetSiblingIndex();
        }
        for (int i = 0; i < bossInfoNodes.Count; i++)
        {
            if (!bossInfoNodes[i].BossSO.IsClaimed)
            {
                index = bossInfoNodes[i].transform.GetSiblingIndex();
                break;
            }
        }
        _content.DOAnchorPosY(-(index * _offsetSrollIndex), _scrollDuration);
    }

    public void HandleComeBackUIFromPVP(bool hasGotoPvPSceneFromBossUI)
    {
        if (hasGotoPvPSceneFromBossUI)
        {
            bool isMatchPlayed = true;
            bool isClaimedBoss = PlayerPrefs.GetInt("Firebase_BossFightMenu", 0) == 1;
            GameEventHandler.Invoke(BossFightEventCode.OnBossMapOpened, isMatchPlayed, isClaimedBoss);
            Show();
        }
        else
        {
            GameEventHandler.Invoke(BossFightEventCode.OnBossMapClosed);
            Hide();
        }
    }
    private void Show()
    {
        SetVisible(true);
    }
    private void Hide()
    {
        SetVisible(false);
    }

    private void HandleOpened(params object[] eventData)
    {
        if (isShowing) return;
        SetVisible(true);

        #region Firebase Event
        if (eventData.Length <= 0 || eventData[0] == null || eventData[1] == null)
        {
            GameEventHandler.Invoke(LogFirebaseEventCode.BossFightMenu, false, false);
            return;
        } 
        bool matchPlayed = (bool)eventData[0];
        bool claimedboss = (bool)eventData[1];
        GameEventHandler.Invoke(LogFirebaseEventCode.BossFightMenu, matchPlayed, claimedboss);
        #endregion
    }

    private void HandleClosed()
    {
        if (!isShowing) return;
        SetVisible(false);

        #region Firebase Event
        GameEventHandler.Invoke(LogFirebaseEventCode.BossFightMenu);
        #endregion
    }

    private void SetVisible(bool isVisible)
    {
        this.isShowing = isVisible;
        if (isVisible)
        {
            visibility.ShowImmediately();
        }
        else
        {
            visibility.HideImmediately();
        }

        if (isVisible && !isUIWarmUp)
            AutoScrollToIndex();
    }

    void OnPreviousChapterClicked()
    {
        if (isUIWarmUp)
            return;
        if (currentChapterIndex > 0)
        {
            currentChapterIndex--;
            UpdateView();
        }
    }

    void OnNextChapterClicked()
    {
        if (isUIWarmUp)
            return;
        if (currentChapterIndex < chapterAmount - 1)
        {
            currentChapterIndex++;
            UpdateView();
        }
    }

    //idea: make a warm-up function for start case
    void UpdateView()
    {
        // previousChapterBtn.gameObject.SetActive(currentChapterIndex > 0);
        // nextChapterBtn.gameObject.SetActive(currentChapterIndex < chapterAmount - 1);

        var currentChapterSO = BossFightManager.Instance.bossMapSO.chapterList[currentChapterIndex];

        foreach (var node in bossInfoNodes)
        {
            infoNodePooling.Add(node);
        }
        bossInfoNodes.Clear();
        if (currentChapterSO.bossCount > 0)
        {
            // comingSoonBGImg.SetActive(false);
            scrollView.SetActive(true);
            for (int i = 0; i < currentChapterSO.bossCount; i++)
            {
                var bossSO = currentChapterSO.bossList[i];
                var infoNode = infoNodePooling.Get();
                infoNode.transform.SetSiblingIndex(i);
                infoNode.Setup(bossSO);
                bossInfoNodes.Add(infoNode);
            }
        }
        else
        {
            // comingSoonBGImg.SetActive(true);
            scrollView.SetActive(false);
        }
        // chapterNameTxt.text = $"{currentChapterIndex + 1}-{currentChapterSO.chapterName}";
    }

    void OnUnpackDone()
    {
        if (isShowingBeforeUnpack)
        {
            SetVisible(true);
        }
    }

    void OnUnpackStart()
    {
        if (isShowing)
        {
            SetVisible(false);
            isShowingBeforeUnpack = true;
        }
        else
        {
            isShowingBeforeUnpack = false;
        }
    }
}
