using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using HyrphusQ.Events;
using LatteGames;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class GearTabSelection : ComposeCanvasElementVisibilityController
{
    [SerializeField, BoxGroup("Ref")] private RectTransform subTabPanelRectTransform;
    [SerializeField, BoxGroup("Ref")] private RectTransform listViewPanelRectTransform;
    [SerializeField, BoxGroup("Ref")] private GearTabButton prebuiltRobotTabButton;
    [SerializeField, BoxGroup("Ref")] private CompositeGearTabButton assembleRobotTabButton;
    [SerializeField, BoxGroup("Ref")] private GearTabButton activeSkillTabButton;

    [SerializeField, Sirenix.OdinInspector.ReadOnly] private DockerTab dockerTab;
    [ShowInInspector, Sirenix.OdinInspector.ReadOnly] private GearTabButton selectedGearTabButton;
    [ShowInInspector, Sirenix.OdinInspector.ReadOnly] private List<GearTabButton> allMainTabButtons;
    [ShowInInspector, Sirenix.OdinInspector.ReadOnly] private List<GearTabButton> allSubTabButtons;
    [ShowInInspector, Sirenix.OdinInspector.ReadOnly] private List<GearTabButton> allTabButtons;

    public GameObject SubTabGO => transform.GetChild(0).GetChild(3).gameObject;
    public GearTabButton ActiveSkillTabButton => activeSkillTabButton;
    public GearTabButton SelectedGearTabButton
    {
        get => selectedGearTabButton;
        set
        {
            if (selectedGearTabButton == value || value == null)
                return;
            GameEventHandler.Invoke(CharacterUIEventCode.OnClickTabButtonUI, value.PartType);

            selectedGearTabButton?.Deselect();
            selectedGearTabButton = value;
            selectedGearTabButton.Select();
            NotifyTabButtonChange(selectedGearTabButton);

            if (selectedGearTabButton != assembleRobotTabButton)
            {
                subTabPanelRectTransform.DOSizeDelta(new Vector2(subTabPanelRectTransform.sizeDelta.x, 18.9f), 0.2f);
                listViewPanelRectTransform.DOSizeDelta(new Vector2(listViewPanelRectTransform.sizeDelta.x, 1031.25f), 0.2f);
                allSubTabButtons.ForEach(tabButton => tabButton.gameObject.SetActive(false));
            }
            else
            {
                subTabPanelRectTransform.DOSizeDelta(new Vector2(subTabPanelRectTransform.sizeDelta.x, 150.7972f), 0.2f);
                listViewPanelRectTransform.DOSizeDelta(new Vector2(listViewPanelRectTransform.sizeDelta.x, 899f), 0.2f);
                allSubTabButtons.ForEach(tabButton => tabButton.gameObject.SetActive(true));
            }

            //TODO: Hide IAP & Popup
            allSubTabButtons.Find(v => v.PartType == PBPartType.Front).gameObject.SetActive(false);
        }
    }
    public GearTabButton DefaultReloadTabButton { get; set; }

    private void Awake()
    {
        dockerTab = GetComponentInParent<DockerTab>();
        allTabButtons = GetComponentsInChildren<GearTabButton>().ToList();
        allSubTabButtons = assembleRobotTabButton.SubTabButtons;
        allMainTabButtons = allTabButtons.Except(assembleRobotTabButton.SubTabButtons).ToList();
        // GameEventHandler.AddActionEvent(CharacterUIEventCode.OnManuallyClickTab, OnManuallyClickTab);
        GameEventHandler.AddActionEvent(CharacterUIEventCode.OnTabSpecial, SelectSkinTab);
        GameEventHandler.AddActionEvent(MainSceneEventCode.OnClickButtonMain, ResetSelect);
        GameEventHandler.AddActionEvent(MainSceneEventCode.OnClickButtonShop, ResetSelect);
        GameEventHandler.AddActionEvent(MainSceneEventCode.OnClickButtonBattlePass, ResetSelect);
        GameEventHandler.AddActionEvent(BossFightEventCode.OnDisableUnlockBoss, OnCloseUnlockBoss);
        ObjectFindCache<GearTabSelection>.Add(this);

        //TODO: Hide IAP & Popup
        #region Hide IAP & Popup
        DisableCanvasGroupTemp(prebuiltRobotTabButton.gameObject.GetOrAddComponent<CanvasGroup>());
        DisableCanvasGroupTemp(assembleRobotTabButton.gameObject.GetOrAddComponent<CanvasGroup>());
        DisableCanvasGroupTemp(activeSkillTabButton.gameObject.GetOrAddComponent<CanvasGroup>());

        void DisableCanvasGroupTemp(CanvasGroup canvasGroup)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
        }
        #endregion
    }

    private void OnDestroy()
    {
        // GameEventHandler.RemoveActionEvent(CharacterUIEventCode.OnManuallyClickTab, OnManuallyClickTab);
        GameEventHandler.RemoveActionEvent(CharacterUIEventCode.OnTabSpecial, SelectSkinTab);
        GameEventHandler.RemoveActionEvent(MainSceneEventCode.OnClickButtonMain, ResetSelect);
        GameEventHandler.RemoveActionEvent(MainSceneEventCode.OnClickButtonShop, ResetSelect);
        GameEventHandler.RemoveActionEvent(MainSceneEventCode.OnClickButtonBattlePass, ResetSelect);
        GameEventHandler.RemoveActionEvent(MainSceneEventCode.OnClickButtonCharacter, SelectCharacterTab);
        GameEventHandler.RemoveActionEvent(BossFightEventCode.OnDisableUnlockBoss, OnCloseUnlockBoss);
        ObjectFindCache<GearTabSelection>.Remove(this);
    }

    private void Start()
    {
        foreach (var tabButton in allMainTabButtons)
        {
            tabButton.Button.onClick.AddListener(() =>
            {
                SelectedGearTabButton = tabButton;
            });
        }
        GameEventHandler.AddActionEvent(MainSceneEventCode.OnClickButtonCharacter, SelectCharacterTab);
    }

    private bool IsUsingPrebuiltRobot()
    {
        return assembleRobotTabButton.GetDefaultSelectedSubTabButton().ItemManagerSO.currentItemInUse.Cast<PBChassisSO>().IsSpecial;
    }

    private void OnCloseUnlockBoss()
    {
        // assembleRobotTabButton?.Deselect();
        // prebuiltRobotTabButton?.Select();
    }

    // private void OnManuallyClickTab(params object[] eventData)
    // {
    //     var partSlot = (PBPartSlot)eventData[0];
    //     SelectTabButton(partSlot);
    // }

    private void ResetSelect()
    {
        assembleRobotTabButton.SetSelectedSubTabButton(assembleRobotTabButton.GetDefaultSelectedSubTabButton());
        assembleRobotTabButton.Button.onClick.Invoke();

        //TODO: Hide IAP & Popup
        //if (IsUsingPrebuiltRobot())
        //    prebuiltRobotTabButton.Button.onClick.Invoke();
        //else
        //{
        //    assembleRobotTabButton.SetSelectedSubTabButton(assembleRobotTabButton.GetDefaultSelectedSubTabButton());
        //    assembleRobotTabButton.Button.onClick.Invoke();
        //}

        GameEventHandler.Invoke(CharacterUIEventCode.OnResetSelect);
    }

    private void SelectCharacterTab(params object[] eventData)
    {
        ReloadCharacterTab();
    }

    private void SelectSkinTab(params object[] eventData)
    {
        if (eventData.Length <= 0) return;
        if (IsUsingPrebuiltRobot())
        {
            GameEventHandler.Invoke(CharacterUIEventCode.OnShowPopup);
            GameEventHandler.Invoke(CharacterUIEventCode.OnHidePowerInfo);
        }
        else
        {
            GameEventHandler.Invoke(CharacterUIEventCode.PureOnClosePopup);
            GameEventHandler.Invoke(CharacterUIEventCode.OnShowPowerInfo);
        }
    }

    private void ReloadCharacterTab()
    {
        if (!dockerTab.isShowing)
            return;
        bool isUsingPrebuiltRobot = IsUsingPrebuiltRobot();
        SelectTabButton(DefaultReloadTabButton ?? (isUsingPrebuiltRobot ? prebuiltRobotTabButton : assembleRobotTabButton.GetDefaultSelectedSubTabButton()));
        DefaultReloadTabButton = null;

        if (isUsingPrebuiltRobot)
        {
            GameEventHandler.Invoke(CharacterUIEventCode.OnShowPopup);
            GameEventHandler.Invoke(CharacterUIEventCode.OnHidePowerInfo);
        }
        else
        {
            GameEventHandler.Invoke(CharacterUIEventCode.PureOnClosePopup);
            GameEventHandler.Invoke(CharacterUIEventCode.OnShowPowerInfo);
        }
    }

    private void NotifyTabButtonChange(params object[] eventData)
    {
        GameEventHandler.Invoke(CharacterUIEventCode.OnTabButtonChange, eventData);
        GameEventHandler.Invoke(CharacterUIEventCode.OnIndexTab, selectedGearTabButton.transform.GetSiblingIndex());
    }

    public List<GearTabButton> GetAllMainTabButtons()
    {
        return allMainTabButtons;
    }

    public List<GearTabButton> GetAllSubTabButtons()
    {
        return allSubTabButtons;
    }

    public List<GearTabButton> GetAllTabButtons()
    {
        return allTabButtons;
    }

    public void SelectTabButton(GearTabButton tabButton)
    {
        selectedGearTabButton = null;
        if (tabButton == prebuiltRobotTabButton)
        {
            prebuiltRobotTabButton.Button.onClick.Invoke();
        }
        else if (tabButton == activeSkillTabButton)
        {
            activeSkillTabButton.Button.onClick.Invoke();
        }
        else
        {
            assembleRobotTabButton.SetSelectedSubTabButton(tabButton);
            assembleRobotTabButton.Button.onClick.Invoke();
        }
    }

    public void SelectTabButton(PBPartSlot partSlot)
    {
        foreach (var tabButton in allTabButtons)
        {
            if (tabButton != assembleRobotTabButton && tabButton.PartSlot == partSlot)
            {
                SelectTabButton(tabButton);
                break;
            }
        }
    }

    public void SetDefaultReloadButton(PBPartSlot partSlot)
    {
        foreach (var tabButton in allTabButtons)
        {
            if (tabButton != assembleRobotTabButton && tabButton.PartSlot == partSlot)
            {
                SetDefaultReloadButton(tabButton);
                break;
            }
        }
    }

    public void SetDefaultReloadButton(GearTabButton tabButton)
    {
        DefaultReloadTabButton = tabButton;
    }
}