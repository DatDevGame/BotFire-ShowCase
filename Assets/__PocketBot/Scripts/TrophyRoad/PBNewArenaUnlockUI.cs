using DG.Tweening;
using HyrphusQ.Events;
using LatteGames;
using LatteGames.PvP.TrophyRoad;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum ArenaUnlockEvent
{
    StartUnlockNewArena,
    OpenArenaUnlock,
    CloseArenaUnlock,
    ShowTextAnimation,
    ShowParttern,
    HideParttern,
    StartBadgeAnimation,
    EndUnBadgeAnimation,
    ShowLightEffect
}

public class PBNewArenaUnlockUI : Singleton<PBNewArenaUnlockUI>
{
    private const string DEFAULT_TEXT_MEDAL = "{value} <sprite name=Medal>";

    [SerializeField, BoxGroup("Property")] private float duration = 2f;

    [SerializeField, BoxGroup("Ref")] private Transform badgeObj;
    [SerializeField, BoxGroup("Ref")] private Transform cameraView;
    [SerializeField, BoxGroup("Ref")] private RectTransform rawImageBadgeRect;
    [SerializeField, BoxGroup("Ref")] private Animator viewUIAnimator;
    [SerializeField, BoxGroup("Ref")] private Animator badgeAnimator;
    [SerializeField, BoxGroup("Ref")] private Image patternImg;
    [SerializeField, BoxGroup("Ref")] private Image lightImageEffect;
    [SerializeField, BoxGroup("Ref")] private Image m_BGImg;
    [SerializeField, BoxGroup("Ref")] private MeshRenderer badgeMeshRenderer;
    [SerializeField, BoxGroup("Ref")] private TMP_Text arenaIndexText;
    [SerializeField, BoxGroup("Ref")] private TMP_Text nameArenaText;
    [SerializeField, BoxGroup("Ref")] private TMP_Text trophyText;
    [SerializeField, BoxGroup("Ref")] private CanvasGroupVisibility infoCanvasGroupVisibility;
    [SerializeField, BoxGroup("Ref")] private CanvasGroupVisibility partternCanvasGroupVisibility;
    [SerializeField, BoxGroup("Ref")] private CanvasGroupVisibility mainCanvasGroupVisibility;

    private TrophyRoadSO.ArenaSection arenaSection;

    public bool IsRunningAnimator { get; set; }

    private void Awake()
    {
        GameEventHandler.AddActionEvent(ArenaUnlockEvent.StartUnlockNewArena, PlayAnimationBadge);
        GameEventHandler.AddActionEvent(ArenaUnlockEvent.OpenArenaUnlock, Show);
        GameEventHandler.AddActionEvent(ArenaUnlockEvent.CloseArenaUnlock, Hide);
        GameEventHandler.AddActionEvent(ArenaUnlockEvent.ShowTextAnimation, ShowTextAnimation);
        GameEventHandler.AddActionEvent(ArenaUnlockEvent.ShowParttern, ShowParttern);
        GameEventHandler.AddActionEvent(ArenaUnlockEvent.HideParttern, HideParttern);
        GameEventHandler.AddActionEvent(ArenaUnlockEvent.StartBadgeAnimation, StartUnlockArenaAnimation);
        GameEventHandler.AddActionEvent(ArenaUnlockEvent.EndUnBadgeAnimation, EndUnlockArenaAnimation);
        GameEventHandler.AddActionEvent(ArenaUnlockEvent.ShowLightEffect, ShowLightEffect);
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(ArenaUnlockEvent.StartUnlockNewArena, PlayAnimationBadge);
        GameEventHandler.RemoveActionEvent(ArenaUnlockEvent.OpenArenaUnlock, Show);
        GameEventHandler.RemoveActionEvent(ArenaUnlockEvent.CloseArenaUnlock, Hide);
        GameEventHandler.RemoveActionEvent(ArenaUnlockEvent.ShowTextAnimation, ShowTextAnimation);
        GameEventHandler.RemoveActionEvent(ArenaUnlockEvent.ShowParttern, ShowParttern);
        GameEventHandler.RemoveActionEvent(ArenaUnlockEvent.HideParttern, HideParttern);
        GameEventHandler.RemoveActionEvent(ArenaUnlockEvent.StartBadgeAnimation, StartUnlockArenaAnimation);
        GameEventHandler.RemoveActionEvent(ArenaUnlockEvent.EndUnBadgeAnimation, EndUnlockArenaAnimation);
        GameEventHandler.RemoveActionEvent(ArenaUnlockEvent.ShowLightEffect, ShowLightEffect);
    }

    private void PlayAnimationBadge(params object[] parrameter)
    {
        if (parrameter.Length > 0)
        {
            if (parrameter[0] != null && parrameter[0] is Sprite)
            {
                patternImg.sprite = (Sprite)parrameter[0];
            }

            if (parrameter[1] != null && parrameter[1] is TrophyRoadSO.ArenaSection)
            {
                arenaSection = (TrophyRoadSO.ArenaSection)parrameter[1];
            }
        }

        if (arenaSection != null)
        {
            if(arenaSection.arenaSO is PBPvPArenaSO arenaSO)
            {
                SetUpColorBG(arenaSO);
            }
        }

        int arenaIndex = arenaSection.arenaSO.index + 1;
        arenaIndexText.SetText($"ARENA {arenaIndex.ToString("D2")}");

        if (arenaSection.arenaSO.TryGetModule<NameItemModule>(out var nameModule))
        {
            nameArenaText.SetText(nameModule.displayName);
        }

        trophyText.SetText($"{trophyText.text.Replace("{value}", $"{arenaSection.GetRequiredMedals()}")}");

        badgeMeshRenderer.material = (arenaSection.arenaSO as PBPvPArenaSO).LogoArena;

        Show();
        badgeAnimator.SetTrigger("BadgeAnimation");
    }
    private void SetUpColorBG(PBPvPArenaSO pvpArenaSO)
    {
        if (pvpArenaSO == null) return;

        Material bgMat = Instantiate(m_BGImg.material);
        var arenaSO = pvpArenaSO;
        bgMat.SetColor("_InsideColor", arenaSO.InsideColor);
        bgMat.SetColor("_OutsideColor", arenaSO.OutsideColor);
        m_BGImg.material = bgMat;
    }
    private void ShowTextAnimation()
    {
        viewUIAnimator.enabled = true;
        viewUIAnimator.SetTrigger("PlayShowTextAnimation");
    }

    private void ShowParttern()
    {
        partternCanvasGroupVisibility.Show();
    }

    private void HideParttern()
    {
        partternCanvasGroupVisibility.Hide();
    }

    private void ShowLightEffect()
    {
        lightImageEffect.gameObject.SetActive(true);
        lightImageEffect.GetComponent<RectTransform>()
            .DOScale(2.5f, 0.2f)
            .OnComplete(() =>
            {
                lightImageEffect.DOFade(0, 0.5f).OnComplete(() =>
                {
                    lightImageEffect.gameObject.SetActive(false);
                    lightImageEffect.GetComponent<RectTransform>().DOScale(1, 0);
                    lightImageEffect.DOFade(1, 0);
                });
            });

    }

    public void StartUnlockArenaAnimation()
    {
        infoCanvasGroupVisibility.Show();
        IsRunningAnimator = true;
    }

    public void EndUnlockArenaAnimation()
    {
        arenaIndexText.gameObject.SetActive(false);
        nameArenaText.gameObject.SetActive(false);
        trophyText.gameObject.SetActive(false);
        IsRunningAnimator = false;
    }


    public void Show()
    {
        cameraView.gameObject.SetActive(true);
        infoCanvasGroupVisibility.Show();
        mainCanvasGroupVisibility.Show();
    }

    public void Hide()
    {
        cameraView.gameObject.SetActive(false);
        trophyText.SetText($"{DEFAULT_TEXT_MEDAL}");
        infoCanvasGroupVisibility.Hide();
        mainCanvasGroupVisibility.Hide();
    }
}
