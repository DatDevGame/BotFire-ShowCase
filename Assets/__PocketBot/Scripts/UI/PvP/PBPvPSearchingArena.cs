using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using HyrphusQ.Events;
using TMPro;
using LatteGames.Template;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using System.Linq;

public class PBPvPSearchingArena : MonoBehaviour
{
    [SerializeField] private Transform _content;
    [SerializeField] private GameObject _arenaItemPrefabs;
    [SerializeField] private TextMeshProUGUI lookingForArenaTimerTMP;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private int searchingDuration = 2;
    [SerializeField] private int _speedScroll = 40;

    private List<ArenaItem> _listItemArena;
    private PBPvPStageSpawner _pbPvPStageSpawner;
    private PBFightingStage _pBFightingStage;
    private Sprite _spriteAvatars;
    private ArenaItem arenaItemSelected;
    private int _offsetIndex = 725;
    private int _indexSelect = 0;
    private IEnumerator _countDonwLookingForArenTimer;
    private bool _isSearching = false;

    private void Awake()
    {
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnStartSearchingArena, OnStartSearchingArena);
    }
    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnStartSearchingArena, OnStartSearchingArena);
    }
    private void OnStartSearchingArena(object[] parameters)
    {
        // PBPvPStageSpawner pBPvPStageSpawner = parameters[0] as PBPvPStageSpawner;
        // _pbPvPStageSpawner = pBPvPStageSpawner;
        // _pBFightingStage = _pbPvPStageSpawner.GetCurrentFightingStagePrefab();
        // _spriteAvatars = _pBFightingStage.GetComponent<PBFightingStage>().GetThumbnail();
        // _isSearching = true;
        // SetupSearchingArena();
        //Spawn Stage
        _pbPvPStageSpawner = parameters[0] as PBPvPStageSpawner;
        _pBFightingStage = _pbPvPStageSpawner.GetCurrentFightingStagePrefab();
        _pbPvPStageSpawner.SpawnStage(_pBFightingStage);
        Hide();
        //GameEventHandler.Invoke(PBPvPEventCode.OnEndSearchingArena);
    }
    private void SetupSearchingArena()
    {
        List<Sprite> avatars = _pbPvPStageSpawner.CurrentStageAvatars;
        List<int> indexs = new List<int>();
        _listItemArena = new List<ArenaItem>();
        int newIndex = 0;
        for (int i = 0; i < _speedScroll; i++)
        {
            if (newIndex > avatars.Count - 1)
                newIndex = 0;

            ArenaItem arenaItem = Instantiate(_arenaItemPrefabs, _content).GetComponent<ArenaItem>();
            _listItemArena.Add(arenaItem);
            arenaItem.SetUp(avatars[newIndex]);
            if (i == _speedScroll - 5)
            {
                if (_spriteAvatars != null)
                    arenaItem.SetUp(_spriteAvatars);

                arenaItemSelected = arenaItem;
                _indexSelect = i;
            }

            if (i == _speedScroll - 4 || i == _speedScroll - 6)
                indexs.Add(arenaItem.transform.GetSiblingIndex());

            newIndex++;
        }

        do
        {
            _listItemArena[indexs[0]].SetUp(avatars[Random.Range(0, avatars.Count)]);
            _listItemArena[indexs[1]].SetUp(avatars[Random.Range(0, avatars.Count)]);
        }
        while (_listItemArena[indexs[0]].GetAvatar() == _spriteAvatars || _listItemArena[indexs[1]].GetAvatar() == _spriteAvatars);
        StartCoroutine(AutoScrollToIndex());
    }
    private IEnumerator CountDownLookingForArenaTimer()
    {
        int time = searchingDuration;
        while (true)
        {
            lookingForArenaTimerTMP.text = time.ToString();
            time--;
            yield return new WaitForSeconds(1);
        }
    }
    private IEnumerator AutoScrollToIndex()
    {
        yield return null;

        if (_countDonwLookingForArenTimer != null)
            StopCoroutine(_countDonwLookingForArenTimer);
        _countDonwLookingForArenTimer = CountDownLookingForArenaTimer();
        StartCoroutine(_countDonwLookingForArenTimer);

        lookingForArenaTimerTMP.transform.parent.gameObject.SetActive(true);

        SoundManager.Instance.PlaySFX(GeneralSFX.UIUpgrade);
        _content.GetComponent<RectTransform>()
            .DOAnchorPosY((_indexSelect - 1) * _offsetIndex, searchingDuration).SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                if (_countDonwLookingForArenTimer != null)
                    StopCoroutine(_countDonwLookingForArenTimer);
                lookingForArenaTimerTMP.transform.parent.gameObject.SetActive(false);

                DOVirtual.DelayedCall(0, () =>
                {
                    SoundManager.Instance.PlaySFX(GeneralSFX.UITapPLAYButton);
                    arenaItemSelected.GetComponent<RectTransform>()
                    .DOScale(Vector2.one * 1.1f, AnimationDuration.SSHORT).SetEase(Ease.OutElastic)
                    .OnComplete(() =>
                    {
                        DOVirtual.DelayedCall(0, () =>
                        {
                            SoundManager.Instance.PlaySFX(GeneralSFX.UIEquipAndUse);
                            arenaItemSelected.ShowOutline();

                            //Spawn Stage
                            _pbPvPStageSpawner.SpawnStage(_pBFightingStage);

                            DOVirtual.DelayedCall(1, () =>
                            {
                                Hide();
                                _isSearching = false;
                                GameEventHandler.Invoke(PBPvPEventCode.OnEndSearchingArena);
                            });
                        });


                    });

                });
            });
    }

    private void Show()
    {
        SetVisible(true);
    }
    private void Hide()
    {
        SetVisible(false);
    }
    private void SetVisible(bool isShow)
    {
        _canvasGroup.DOFade(isShow ? 1f : 0f, 0.1f);
        _canvasGroup.interactable = isShow;
        _canvasGroup.blocksRaycasts = isShow;
    }

    private void OnApplicationPause(bool pause)
    {
        if (_isSearching)
        {
            _isSearching = false;

            string name = "";
            if (_pBFightingStage != null)
                name = _pBFightingStage.gameObject.name.Replace("(Clone)", "");

            string stageName = name;
            string mainPhase = "Matchmaking";
            string subPhase = "SearchingStage";
            GameEventHandler.Invoke(DesignEvent.QuitCheck, stageName, mainPhase, subPhase);
        }
    }
}
