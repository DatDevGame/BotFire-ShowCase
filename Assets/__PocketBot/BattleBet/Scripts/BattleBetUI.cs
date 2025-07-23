using System.Collections;
using UnityEngine;
using Sirenix.OdinInspector;
using HyrphusQ.Events;
using LatteGames;
using UnityEngine.UI;
using DanielLochner.Assets.SimpleScrollSnap;
using LatteGames.PvP;
using UnityEngine.UIElements;
using LatteGames.GameManagement;
using System.Linq;

public class BattleBetUI : MonoBehaviour
{
    private const float PEAK_OFFSET = 195f;
    private const int SCREEN_CHECK_FRAME = 20;

    [SerializeField, BoxGroup("UI")] private CanvasGroupVisibility _mainCanvasGroup;
    [SerializeField, BoxGroup("UI")] private CanvasScaler _canvasScaler;
    [SerializeField, BoxGroup("UI")] private SimpleScrollSnap _simpleScrollSnap;
    [SerializeField, BoxGroup("UI")] private Canvas _blockUICanvas;
    [SerializeField, BoxGroup("Template")] private BattleBetSelectArenaUI _battleBetSelectArenaTemplate;
    [SerializeField, BoxGroup("Data")] private PvPTournamentSO _pvpTournamentSO;
    [SerializeField, BoxGroup("Data")] private CurrentHighestArenaVariable _currentHighestArenaVar;
    [SerializeField, BoxGroup("Data")] private BattleBetArenaVariable _battleBetArenaArenaVar;
    [SerializeField, BoxGroup("Data")] private PPrefBoolVariable _pPrefBoolBattleRoyal;

    public bool isShowing =>_isShow || _isTryingShow;

    private bool _isShow = false;
    private bool _isTryingShow = false;
    private bool _isInitCompleted = false;
    private int _frameOffset;
    private float _screenRatio;
    private Coroutine _showUICoroutine = null;

    #region Unity Messages

    private void Awake()
    {
        _isInitCompleted = false;
        // Block UI interaction until the Start coroutine is completed
        _blockUICanvas.enabled = true;
        _blockUICanvas.overrideSorting = true;
        _blockUICanvas.sortingOrder = 9999;
    }

    private IEnumerator Start()
    {
        // ASSUMING: _canvasScaler set ScreenMatchMode to fully matching in height
        _screenRatio = _canvasScaler.referenceResolution.x / _canvasScaler.referenceResolution.y;
        GameEventHandler.AddActionEvent(BattleBetEventCode.OnBattleBetOpened, OnShow);
        GameEventHandler.AddActionEvent(BattleBetEventCode.OnBattleBetClosed, OnHide);
        GameEventHandler.AddActionEvent(BattleBetEventCode.OnEnterBattle, OnEnterBattle);
        GameEventHandler.AddActionEvent(FTUEEventCode.OnRoyalModeSelectArena, OnFTUESelectArena);
        yield return null;
        foreach (PvPArenaSO arenaSO in _pvpTournamentSO.arenas)
        {
            _simpleScrollSnap.AddToBack(_battleBetSelectArenaTemplate.gameObject);
            yield return null;
            _simpleScrollSnap.Panels[_simpleScrollSnap.NumberOfPanels - 1].GetComponent<BattleBetSelectArenaUI>().InitData(arenaSO);
            yield return null;
        }
        OnEndHide();
        _mainCanvasGroup.GetOnEndHideEvent().Subscribe(OnEndHide);
        _frameOffset = (Time.frameCount + 1) % SCREEN_CHECK_FRAME;
        yield return null;
        CheckUpdateScrollSize();
        _blockUICanvas.enabled = false;
        _isInitCompleted = true;
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(BattleBetEventCode.OnBattleBetOpened, OnShow);
        GameEventHandler.RemoveActionEvent(BattleBetEventCode.OnBattleBetClosed, OnHide);
        GameEventHandler.RemoveActionEvent(BattleBetEventCode.OnEnterBattle, OnEnterBattle);
        GameEventHandler.RemoveActionEvent(FTUEEventCode.OnRoyalModeSelectArena, OnFTUESelectArena);
        _mainCanvasGroup.GetOnEndHideEvent().Unsubscribe(OnEndHide);
    }

    private void Update()
    {
        if (_isInitCompleted && _isShow && Time.frameCount % SCREEN_CHECK_FRAME == _frameOffset)
        {
            CheckUpdateScrollSize();
        }
    }
    #endregion

    private void CheckUpdateScrollSize()
    {
        float ratio = (float)Screen.width / Screen.height;
        if (!Mathf.Approximately(ratio, _screenRatio))
        {
            float width = ratio * _canvasScaler.referenceResolution.y;
            _simpleScrollSnap.Size = new Vector2(
                Mathf.Max(1, width * 0.5f + PEAK_OFFSET),
                _simpleScrollSnap.Size.y);
            _screenRatio = ratio;
        }
    }

    public void TryShowUI()
    {
        _isTryingShow = true;
        if (_showUICoroutine != null)
        {
            StopCoroutine(_showUICoroutine);
            _showUICoroutine = null;
        }
        _showUICoroutine = StartCoroutine(CommonCoroutine.WaitUntil(
            () => _isInitCompleted, 
            () => { 
                GameEventHandler.Invoke(BattleBetEventCode.OnBattleBetOpened);
                _simpleScrollSnap.GoToPanel(_battleBetArenaArenaVar.value.index);
                _isTryingShow = false;
        }));
    }

    private void OnShow()
    {
        if (_isShow)
            return;

        _isShow = true;
        _mainCanvasGroup.Show();
        _simpleScrollSnap.GoToPanel(_currentHighestArenaVar.value.index);
    }

    private void OnHide()
    {
        if (_isTryingShow && _showUICoroutine != null)
        {
            StopCoroutine(_showUICoroutine);
            _showUICoroutine = null;
            _isTryingShow = false;
        }
        if (!_isShow)
            return;

        _isShow = false;
        _mainCanvasGroup.Hide();
    }

    private void OnEnterBattle(params object[] objs)
    {
        if (!_pPrefBoolBattleRoyal.value)
        {
            _pPrefBoolBattleRoyal.value = true;
            _simpleScrollSnap.ScrollRect.enabled = false;
            _simpleScrollSnap.Panels[_currentHighestArenaVar.value.index].GetComponent<BattleBetSelectArenaUI>().SetupFTUE(false);
            GameEventHandler.Invoke(FTUEEventCode.OnRoyalModeHighlinetArena);
        }

        if (objs[0] is PBPvPArenaSO pbPbPArenaSO)
        {
            #region Design Event
            int arenaChoosen = pbPbPArenaSO.index + 1;
            GameEventHandler.Invoke(DesignEvent.BattlePvPChooseArena, arenaChoosen);
            #endregion

            _battleBetArenaArenaVar.value = pbPbPArenaSO;

            var activeScene = SceneManager.GetActiveScene();
            var mainScreenUI = activeScene.GetRootGameObjects().FirstOrDefault(go => go.name == "MainScreenUI");
            foreach (var gameObject in activeScene.GetRootGameObjects())
            {
                if (gameObject == mainScreenUI)
                    continue;
                gameObject.SetActive(false);
            }
            SceneManager.LoadScene(SceneName.PvP, UnityEngine.SceneManagement.LoadSceneMode.Additive, callback: OnLoadSceneCompleted);

            void OnLoadSceneCompleted(SceneManager.LoadSceneResponse loadSceneResponse)
            {
                mainScreenUI?.SetActive(false);
                var previousBackgroundLoadingPriority = Application.backgroundLoadingPriority;
                Application.backgroundLoadingPriority = ThreadPriority.Low;
                SceneManager.UnloadSceneAsync(activeScene.name).onCompleted += () =>
                {
                    Application.backgroundLoadingPriority = previousBackgroundLoadingPriority;
                };
            }
        }
    }

    private void OnFTUESelectArena()
    {
        if (!_pPrefBoolBattleRoyal.value)
        {
            int index = _currentHighestArenaVar.value.index;
            _simpleScrollSnap.ScrollRect.enabled = false;
            _simpleScrollSnap.Panels[index].GetComponent<BattleBetSelectArenaUI>().SetupFTUE(true);
            GameEventHandler.Invoke(FTUEEventCode.OnRoyalModeHighlinetArena, _simpleScrollSnap.Panels[index].gameObject);
        }
    }

    private void OnEndHide()
    {
        if (_isInitCompleted)
            _simpleScrollSnap.GoToPanel(Mathf.Max(0, _currentHighestArenaVar.value.index - 1));
    }

    #if UNITY_EDITOR
    [Button]
    private void ShowUI()
    {
        GameEventHandler.Invoke(BattleBetEventCode.OnBattleBetOpened);
    }

    [Button]
    private void HideUI()
    {
        GameEventHandler.Invoke(BattleBetEventCode.OnBattleBetClosed);
    }
    #endif
}

[EventCode]
public enum BattleBetEventCode
{
    OnBattleBetOpened,
    OnBattleBetClosed,
    OnEnterBattle,
}