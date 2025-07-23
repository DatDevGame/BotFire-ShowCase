using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HyrphusQ.Events;
using DG.Tweening;
using System;
using System.Linq;
using LatteGames.Template;
using UnityEngine.EventSystems;
using Sirenix.OdinInspector;
using LatteGames;

public class GearListSelection : MonoBehaviour
{
    [SerializeField, BoxGroup("Ref")] private RectTransform overUpPanel;
    [SerializeField, BoxGroup("Ref")] private RectTransform overDownPanel;

    public RectTransform OverUpPanel => overUpPanel;
    public RectTransform OverDownPanel => overDownPanel;

    [Header("Reference")]
    [SerializeField] private CanvasGroup canvasListGearButton;

    [SerializeField] DockerTab dockerTab;
    [SerializeField] GameObject notFoundSectionPrefab;
    [SerializeField] GameObject notFoundSectionPrefab_TransformBot;

    [SerializeField] PBPartManagerSO SpecialSO;
    [SerializeField] PBPartManagerSO BodySO;
    [SerializeField] PBPartManagerSO WheelSO;
    [SerializeField] PBPartManagerSO FrontSO;
    [SerializeField] PBPartManagerSO UpperSO;
    [SerializeField] ScrollRect scrollRect;
    [SerializeField] CurrentGearHolder currentGearHolder;
    [Header("Properties")]
    public List<GearButton> gearButtons = null;

    GearButton selectedGearButton = null;
    RectTransform rectTransform;
    private IEnumerator sortList;
    private IEnumerator sortButtonIfNotUnlock;

    public GearButton SelectedGearButton
    {
        get => selectedGearButton;
        set
        {
            if (value == null) return;
            if (!FTUEMainScene.Instance.FTUE_Equip.value && value.PartSO.PartType != HandleFTUECharacterUI.partType_Equip_1)
            {
                return;
            }
            if (!FTUEMainScene.Instance.FTUEUpgrade.value && value.PartSO.PartType != HandleFTUECharacterUI.partType_Equip_1)
            {
                return;
            }
            if (selectedGearButton == value && selectedGearButton != null)
            {
                if (selectedGearButton.IsSelected)
                {
                    selectedGearButton.DeSelect();
                    return;
                }
                if (!selectedGearButton.IsSelected)
                {
                    selectedGearButton.Select();
                    return;
                }
            }
            var previousButton = selectedGearButton;
            selectedGearButton = value;
            OnButtonClick(previousButton, selectedGearButton);
        }
    }

    protected void Awake()
    {
        GameEventHandler.AddActionEvent(CharacterUIEventCode.ClickGearButtonViaCode, OnViaCodeClickGearButton);
        GameEventHandler.AddActionEvent(CharacterUIEventCode.OnGearCardEquip, OnGearCardEquip);
        GameEventHandler.AddActionEvent(CharacterUIEventCode.OnResetSelect, ResetChassisEquiped);
        GameEventHandler.AddActionEvent(CharacterUIEventCode.OnSelectTabSpecial, SelectSpecialTabButton);
        GameEventHandler.AddActionEvent(CharacterUIEventCode.OnEquipedSpecial, SelectSpecialButton);
        GameEventHandler.AddActionEvent(CharacterUIEventCode.OnEquipedNotSpecial, SelectChassisButton);
        GameEventHandler.AddActionEvent(CharacterUIEventCode.OnTabButtonChange, HandleOnTabButtonChange);
        GameEventHandler.AddActionEvent(MainSceneEventCode.OnClickButtonMain, OnClickButtonMain);
        GameEventHandler.AddActionEvent(MainSceneEventCode.OnClickButtonCharacter, OnClickButtonCharacter);

        rectTransform = GetComponent<RectTransform>();
    }

    private void OnDestroy()
    {
#if UNITY_EDITOR
        if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
            return;
#endif
        foreach (var gearButton in gearButtons)
        {
            GearButtonPool.Release(gearButton);
        }

        GameEventHandler.RemoveActionEvent(CharacterUIEventCode.ClickGearButtonViaCode, OnViaCodeClickGearButton);
        GameEventHandler.RemoveActionEvent(CharacterUIEventCode.OnGearCardEquip, OnGearCardEquip);
        GameEventHandler.RemoveActionEvent(CharacterUIEventCode.OnResetSelect, ResetChassisEquiped);
        GameEventHandler.RemoveActionEvent(CharacterUIEventCode.OnSelectTabSpecial, SelectSpecialTabButton);
        GameEventHandler.RemoveActionEvent(CharacterUIEventCode.OnEquipedSpecial, SelectSpecialButton);
        GameEventHandler.RemoveActionEvent(CharacterUIEventCode.OnEquipedNotSpecial, SelectChassisButton);
        GameEventHandler.RemoveActionEvent(CharacterUIEventCode.OnTabButtonChange, HandleOnTabButtonChange);
        GameEventHandler.RemoveActionEvent(MainSceneEventCode.OnClickButtonMain, OnClickButtonMain);
        GameEventHandler.RemoveActionEvent(MainSceneEventCode.OnClickButtonCharacter, OnClickButtonCharacter);
    }

    private void OnViaCodeClickGearButton(params object[] eventData)
    {
        var partSO = (PBPartSO)eventData[0];
        var isShowInfo = (bool)eventData[1];
        if (partSO != null)
        {
            ClickGearButton(partSO, isShowInfo);
        }
    }

    private void ResetChassisEquiped(params object[] eventData)
    {
        PBChassisSO pbBody = BodySO.initialValue.Find(v => (v as PBChassisSO).IsEquipped).Cast<PBChassisSO>();
        if (pbBody != null)
        {
            ClickGearButton(pbBody);
            return;
        }
        PBChassisSO pbSpecial = SpecialSO.initialValue.Find(v => (v as PBChassisSO).IsEquipped).Cast<PBChassisSO>();
        if (pbSpecial != null)
            ClickGearButton(pbSpecial);

    }
    private void ClickGearButton(PBPartSO pB, bool isShowInfo = false)
    {
        for (int i = 0; i < gearButtons.Count; i++)
        {
            if (gearButtons[i].PartSO == pB)
            {
                gearButtons[i].Button.onClick.Invoke();
                if (isShowInfo)
                {
                    gearButtons[i].upgradeBtn.onClick.Invoke();
                }
                // SoundManager.Instance.StopCurrentSFX();
                break;
            }
        }
    }
    private void SelectSpecialTabButton(params object[] eventData)
    {
        StartCoroutine(AutoEquipedSpecial());
    }

    private void SelectChassisButton(params object[] eventData)
    {
        for (int i = 0; i < SpecialSO.initialValue.Count; i++)
        {
            PBChassisSO pbSkin = SpecialSO.initialValue[i] as PBChassisSO;
            pbSkin.IsEquipped = false;
        }
    }
    private void SelectSpecialButton(params object[] eventData)
    {
        if (eventData.Length > 0)
        {
            PBChassisSO pBChassis = (PBChassisSO)eventData[0];
            Button gearButton = gearButtons.Where(v => v != null && v.PartSO == pBChassis).First().Button;
            gearButton.onClick.Invoke();
        }

        for (int i = 0; i < SpecialSO.initialValue.Count; i++)
        {
            PBChassisSO pbBody = BodySO.initialValue[i] as PBChassisSO;
            pbBody.IsEquipped = false;
        }
    }
    private bool IsEquipSpecial() => SpecialSO.initialValue
        .Where(v => v != null)
        .Any(v => (v as PBChassisSO).IsEquipped);

    private void GenerateEmptyButton(int count)
    {
        float cal_2 = count == 0 ? 0 : 3 - (count % 3);
        bool check = count >= 3 && count % 3 == 0;
        if (!check)
        {
            Enumerable.Range(0, (int)cal_2).ToList().ForEach(_ =>
            {
                GearButton gearButton = GearButtonPool.Get();
                gearButton.transform.SetParent(transform);
                gearButton.CanvasGroup.alpha = 0;
                gearButton.gameObject.SetActive(true);
                gearButtons.Add(gearButton);
                emptyGearButtons.Add(gearButton);
            });
        }
    }

    private void GenerateButton(PBPartSO partSO, PBPartSlot partSlot, bool isDisable)
    {
        if (!partSO.IsUnlocked())
        {
            return;
        }

        GearButton gearButton = GearButtonPool.Get();
        gearButton.transform.SetParent(transform);
        gearButton.isDisable = IsEquipSpecial() && partSO.PartType != PBPartType.Body || isDisable;
        gearButton.PartSO = partSO;
        gearButton.PartSlot = partSlot;
        gearButton.Button.onClick.AddListener(() => OnChangeSelectedButton(gearButton));
        // gearButton.LoadEnergyUI(false);
        gearButton.gameObject.SetActive(true);
        gearButtons.Add(gearButton);
        /// ??? Why do we need to loop through all these gearButtons every time we generate a single one????? Why not just set the isDisable field directly?
        // if (HasSpecial())
        // {
        //     if (!(partSO as PBChassisSO))
        //     {
        //         gearButtons.ForEach((v) =>
        //         {
        //             v.isDisable = true;
        //         });
        //     }
        // }
        unlockearButtons.Add(gearButton);
    }

    private void GenerateDisableTransformBotButton(PBPartSO partSO, PBPartSlot partSlot, bool isDisable)
    {
        if (partSO.IsUnlocked() || !partSO.IsTransformBot)
        {
            return;
        }

        GearButton gearButton = GearButtonPool.Get();
        gearButton.transform.SetParent(transform);
        gearButton.isDisable = isDisable;
        gearButton.PartSO = partSO;
        gearButton.PartSlot = partSlot;
        gearButton.Button.onClick.AddListener(() => OnChangeSelectedButton(gearButton));
        gearButton.gameObject.SetActive(true);
        gearButtons.Add(gearButton);
        transformBotLockgearButtons.Add(gearButton);
    }

    private void GenerateDisableButton(PBPartSO partSO, PBPartSlot partSlot, bool isDisable)
    {
        if (partSO.IsUnlocked() || partSO.IsTransformBot)
        {
            return;
        }

        GearButton gearButton = GearButtonPool.Get();
        gearButton.transform.SetParent(transform);
        gearButton.isDisable = IsEquipSpecial() && partSO.PartType != PBPartType.Body || isDisable;
        gearButton.PartSO = partSO;
        gearButton.PartSlot = partSlot;
        gearButton.Button.onClick.AddListener(() => OnChangeSelectedButton(gearButton));
        gearButton.gameObject.SetActive(true);
        gearButtons.Add(gearButton);
        lockgearButtons.Add(gearButton);
    }

    private void OnChangeSelectedButton(GearButton gearButton)
    {
        SelectedGearButton = gearButton;
    }

    private void OnButtonClick(GearButton _previousButton, GearButton _selectedGearTabButton)
    {
        _previousButton?.DeSelect();
        _selectedGearTabButton?.Select();
        if (_selectedGearTabButton.isDisable) return;
        if (_selectedGearTabButton.PartSO.PartType == PBPartType.Body)
        {
            NotifyTabButtonChange((PBChassisSO)_selectedGearTabButton.PartSO);
            void NotifyTabButtonChange(params object[] eventData)
            {
                GameEventHandler.Invoke(CharacterUIEventCode.OnChassisButtonClick, eventData);
            }
        }
        GameEventHandler.Invoke(CharacterUIEventCode.OnGearButtonClick, _selectedGearTabButton.PartSO, _selectedGearTabButton.PartSlot, true);
    }
    // PBPartSlot partSlot;
    // PBPartType partType;
    bool _isShowingCharacterUI = false;

    private void OnClickButtonMain()
    {
        _isShowingCharacterUI = false;
    }

    private void OnClickButtonCharacter()
    {
        _isShowingCharacterUI = true;
    }

    private void HandleOnTabButtonChange(params object[] parameters)
    {
        canvasListGearButton.DOFade(0, 0.1F);
        if (!_isShowingCharacterUI)
            return;

        if (parameters[0] is GearTabButton gearTabButton && gearTabButton.ItemManagerSO is PBPartManagerSO partManagerSO)
        {
            GenerateButtonList(partManagerSO, gearTabButton.PartSlot == PBPartSlot.PrebuiltBody ? PBPartSlot.Body : gearTabButton.PartSlot, false);
        }
        
    }

    public List<GearButton> UnlockearButtons => unlockearButtons;

    [SerializeField] int emptyCount;

    [SerializeField] List<GearButton> emptyGearButtons;
    [SerializeField] List<GearButton> unlockearButtons;
    [SerializeField] List<GearButton> lockgearButtons;
    [SerializeField] List<GearButton> transformBotLockgearButtons;
    private RectTransform rectTransformLastButton;
    private RectTransform rectTransformThis;
    GameObject notFoundSection;
    GameObject notFoundSection_TransformBot;

    void GenerateButtonList(PBPartManagerSO partMangerSO, PBPartSlot partSlot, bool isDisable)
    {
        if (partMangerSO == null)
            return;
        currentGearHolder.currentSlot = partSlot;
        currentGearHolder.currentSO = null;

        var unlockParts = partMangerSO.Parts.FindAll(part => part.IsUnlocked());
        var normalLockParts = partMangerSO.Parts.FindAll(part => !part.IsUnlocked() && !part.IsTransformBot);
        var transformBotLockParts = partMangerSO.Parts.FindAll(part => !part.IsUnlocked() && part.IsTransformBot);

        if (sortList != null)
            StopCoroutine(sortList);

        gearButtons.ForEach(item => GearButtonPool.Release(item));

        gearButtons.Clear();
        lockgearButtons.Clear();
        transformBotLockgearButtons.Clear();
        unlockearButtons.Clear();
        emptyGearButtons.Clear();

        if (notFoundSection != null)
            notFoundSection.SetActive(false);
        if (notFoundSection_TransformBot != null)
            notFoundSection_TransformBot.SetActive(false);

        selectedGearButton = null;

        unlockParts.ForEach(part => GenerateButton(part, partSlot, isDisable));
        GenerateEmptyButton(unlockParts.Count);

        normalLockParts.ForEach(part => GenerateDisableButton(part, partSlot, isDisable));
        GenerateEmptyButton(normalLockParts.Count);
        transformBotLockParts.ForEach(part => GenerateDisableTransformBotButton(part, partSlot, isDisable));

        scrollRect.ScrollToTop();
        if (sortList != null)
            StopCoroutine(sortList);
        sortList = CR_SortList();
        StartCoroutine(sortList);

        if (sortButtonIfNotUnlock != null)
            StopCoroutine(sortButtonIfNotUnlock);
        sortButtonIfNotUnlock = SortButtonIfNotUnlock();
        StartCoroutine(sortButtonIfNotUnlock);
    }
    private IEnumerator SortButtonIfNotUnlock()
    {
        yield return new WaitForSeconds(0.1f);
        if (unlockearButtons.Count <= 0)
        {
            lockgearButtons.ForEach(v => v.transform.SetAsLastSibling());
            transformBotLockgearButtons.ForEach(v => v.transform.SetAsLastSibling());
        }

        yield return new WaitForSeconds(0.1f);
        canvasListGearButton.DOFade(1, 0.15f);
    }
    private Vector2 AdjustPosionGearListLimit()
    {
        rectTransformLastButton = gearButtons.OrderBy(v => v.RectTransform.anchoredPosition.y).First().RectTransform;
        rectTransformThis = gameObject.GetComponent<RectTransform>();
        return new Vector2(rectTransformThis.anchoredPosition.x, Mathf.Abs(rectTransformLastButton.anchoredPosition.y) + 670);
    }

    private bool _isCallOneTimeAutoEquipSpecial = false;
    private IEnumerator AutoEquipedSpecial()
    {
        if (_isCallOneTimeAutoEquipSpecial) yield break;
        PBChassisSO pbSpecial = SpecialSO.initialValue.Find(v => (v as PBChassisSO).IsEquipped).Cast<PBChassisSO>();
        for (int i = 0; i < gearButtons.Count; i++)
        {
            if (gearButtons[i].PartSO == pbSpecial)
            {
                gearButtons[i].Button.onClick.Invoke();
                yield return new WaitForSeconds(0.01f);
                gearButtons[i].equipBtn.onClick.Invoke();
                gearButtons[i].Button.onClick.Invoke();
                SoundManager.Instance.StopCurrentSFX();

                _isCallOneTimeAutoEquipSpecial = true;

                GameEventHandler.Invoke(CharacterUIEventCode.OnShowPopup);
                GameEventHandler.Invoke(CharacterUIEventCode.OnHidePowerInfo);

                yield break;
            }
        }
    }

    IEnumerator CR_SortList()
    {
        yield return new WaitForSeconds(0.05f);
        foreach (var item in gearButtons)
        {
            item.IgnoreLayout();
        }
        foreach (var item in gearButtons)
        {
            item.transform.SetAsFirstSibling();
        }
        foreach (var item in emptyGearButtons)
        {
            for (int i = 0; i < gearButtons.Count; i++)
            {
                if (gearButtons[i] == null) continue;
                if (gearButtons[i].gameObject.GetInstanceID() == item.gameObject.GetInstanceID())
                {
                    gearButtons.RemoveAt(i);
                }
            }
            GearButtonPool.Release(item);
        }

        foreach (var item in lockgearButtons)
        {
            RectTransform rectTransform = item.RectTransform;
            var offset = unlockearButtons.Count > 0 ? 100 : 0;
            rectTransform.position = new Vector2(item.RectTransform.position.x, item.RectTransform.position.y - offset);
        }

        foreach (var item in transformBotLockgearButtons)
        {
            RectTransform rectTransform = item.RectTransform;
            var offset = (unlockearButtons.Count > 0 ? 100 : 0) + (lockgearButtons.Count > 0 ? 100 : 0);
            rectTransform.position = new Vector2(item.RectTransform.position.x, item.RectTransform.position.y - offset);
        }

        rectTransform.sizeDelta = AdjustPosionGearListLimit();
        if (lockgearButtons.Count > 0)
        {
            if (notFoundSection == null)
            {
                notFoundSection = Instantiate(notFoundSectionPrefab, transform);
            }

            if (unlockearButtons.Count > 0)
            {
                notFoundSection.SetActive(true);
                notFoundSection.transform.localPosition = new Vector2(notFoundSection.transform.localPosition.x
                , Mathf.Lerp(unlockearButtons.Last().GetComponent<RectTransform>().localPosition.y, lockgearButtons.First().GetComponent<RectTransform>().localPosition.y, 0.5f));
            }
            else
            {
                lockgearButtons.ForEach((v) =>
                {
                    v.IgnoreLayout(false);
                });
                notFoundSection.SetActive(false);
            }
            notFoundSection.transform.SetAsFirstSibling();
        }

        if (transformBotLockgearButtons.Count > 0)
        {
            if (notFoundSection_TransformBot == null)
            {
                notFoundSection_TransformBot = Instantiate(notFoundSectionPrefab_TransformBot, transform);
            }

            if (lockgearButtons.Count > 0)
            {
                notFoundSection_TransformBot.SetActive(true);
                notFoundSection_TransformBot.transform.localPosition = new Vector2(notFoundSection.transform.localPosition.x
                , Mathf.Lerp(lockgearButtons.Last().GetComponent<RectTransform>().localPosition.y, transformBotLockgearButtons.First().GetComponent<RectTransform>().localPosition.y, 0.5f));
            }
            else if (unlockearButtons.Count > 0)
            {
                notFoundSection_TransformBot.SetActive(true);
                notFoundSection_TransformBot.transform.localPosition = new Vector2(notFoundSection.transform.localPosition.x
                , Mathf.Lerp(unlockearButtons.Last().GetComponent<RectTransform>().localPosition.y, transformBotLockgearButtons.First().GetComponent<RectTransform>().localPosition.y, 0.5f));
            }
            else
            {
                transformBotLockgearButtons.ForEach((v) =>
                {
                    v.IgnoreLayout(false);
                });
                notFoundSection_TransformBot.SetActive(false);
            }
            notFoundSection_TransformBot.transform.SetAsFirstSibling();
        }
    }

    private void OnGearCardEquip()
    {
        for (int i = 0; i < gearButtons.Count; i++)
        {
            gearButtons[i].UpdateOutlineSelected();
        }
    }
}