using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HyrphusQ.Events;
using UnityEngine.UI;
using LatteGames.PvP.TrophyRoad;
using Sirenix.OdinInspector;
using System;

public class CharacterTabButton : MonoBehaviour
{
    [SerializeField] public PBPartManagerSO[] partManagerSO;
    [SerializeField] private GameObject newTag;
    [SerializeField] private GameObject upgradableTag;
    [SerializeField] private bool hasNewTag;
    [SerializeField] private bool hasUpgradeTag;

    [SerializeField, BoxGroup("FTUE")] private GameObject _ftueHand;
    [SerializeField, BoxGroup("FTUE")] private PPrefBoolVariable _buildTabFTUE;
    [SerializeField, BoxGroup("FTUE")] private PPrefBoolVariable _activeBuildTab;
    [SerializeField, BoxGroup("FTUE")] private PPrefBoolVariable _lootBoxSlotTheFirstTime;
    [SerializeField, BoxGroup("FTUE")] private PPrefBoolVariable equip_1FTUE;
    [SerializeField, BoxGroup("FTUE")] private PPrefBoolVariable equip_2FTUE;
    [SerializeField, BoxGroup("FTUE")] private PPrefBoolVariable upgradeFTUE;
    [SerializeField, BoxGroup("FTUE")] private PPrefBoolVariable skinUI_FTUE;
    [SerializeField, BoxGroup("FTUE")] HighestAchievedPPrefFloatTracker highestAchievedPPrefFloatTracker;

    [SerializeField, BoxGroup("PSFTUE")] private MultiImageButton m_CharacterUIBtn;
    [SerializeField, BoxGroup("PSFTUE")] private PSFTUESO m_PSFTUESO;

    private IEnumerator _buildTabFTUECR;
    private bool isOpeningPackFromDockSlot;
    private bool m_IsOpenTrophyRoad = false;

    public bool HasNewTag
    {
        get => hasNewTag;
        set
        {
            hasNewTag = value;
            newTag.SetActive(value);
            upgradableTag.SetActive(!value);
        }
    }
    public bool HasUpgradeTag
    {
        get => hasUpgradeTag;
        set
        {
            hasUpgradeTag = value;
            upgradableTag.SetActive(value);
        }
    }
    public GameObject FTUEHand
    {
        get => _ftueHand;
    }

    private void Start()
    {
        StartCoroutine(DelayCheckFTUE());

        _buildTabFTUECR = CheckFTUEGachaBoxEnd(0.2f);
        StartCoroutine(_buildTabFTUECR);
        CheckButtonIsNew();
        CheckButtonIsUpgradable();
        CheckFTUETab();
    }

    private IEnumerator DelayCheckFTUE()
    {
        yield return new WaitForSeconds(0.5f);
        yield return new WaitUntil(() => !m_IsOpenTrophyRoad);

        if (m_PSFTUESO.FTUEStartFirstMatch.value && m_PSFTUESO.FTUEInFirstMatch.value && !m_PSFTUESO.FTUEOpenBuildUI.value)
        {
            m_PSFTUESO.FTUEOpenBuildUI.value = true;
            GameEventHandler.Invoke(PSFTUEBlockAction.Block, this.gameObject, PSFTUEBubbleText.OpenBuild);
            OnBlockFTUE();

            GameEventHandler.Invoke(PSLogFTUEEventCode.StartOpenBuildUI);
        }

        if (m_PSFTUESO.FTUEStartFirstMatch.value && m_PSFTUESO.FTUEInFirstMatch.value && !m_PSFTUESO.FTUEOpenInfoPopup.value)
        {
            GameEventHandler.Invoke(PSFTUEBlockAction.Block, this.gameObject, PSFTUEBubbleText.OpenBuild);
            OnBlockFTUE();
        }

        if (m_PSFTUESO.FTUEStart2ndMatch.value && !m_PSFTUESO.FTUENewWeapon.value)
        {
            GameEventHandler.Invoke(PSFTUEBlockAction.Block, this.gameObject, PSFTUEBubbleText.NewWeapon);
            OnBlockFTUE();

            GameEventHandler.Invoke(PSLogFTUEEventCode.StartNewWeapon_OpenBuildUI);
        }

        void OnBlockFTUE()
        {
            FTUEHand.gameObject.SetActive(true);
            if (m_CharacterUIBtn != null)
                m_CharacterUIBtn.onClick.AddListener(OnClickFTUEBuildUI);
        }
    }

    private void Awake()
    {
        GameEventHandler.AddActionEvent(MainSceneEventCode.OnClickButtonCharacter, CheckButtonIsNew);
        GameEventHandler.AddActionEvent(PartManagementEventCode.OnPartNewStateChanged, CheckButtonIsNew);
        GameEventHandler.AddActionEvent(CharacterUIEventCode.OnGearButtonClick, CheckButtonIsNew);
        GameEventHandler.AddActionEvent(MainSceneEventCode.OnClickButtonCharacter, CheckButtonIsUpgradable);
        GameEventHandler.AddActionEvent(PartManagementEventCode.OnPartCardChanged, CheckButtonIsUpgradable);
        GameEventHandler.AddActionEvent(CharacterUIEventCode.OnGearButtonClick, CheckButtonIsUpgradable);
        GameEventHandler.AddActionEvent(CharacterUIEventCode.OnLockCharacterUIButton, LockButton);
        GameEventHandler.AddActionEvent(CharacterUIEventCode.OnUnLockCharacterUIButton, UnlockButton);
        GameEventHandler.AddActionEvent(TrophyRoadEventCode.OnTrophyRoadOpened, OnOpenrophyRoad);
        GameEventHandler.AddActionEvent(TrophyRoadEventCode.OnTrophyRoadClosed, OnOutTrophyRoad);
        GameEventHandler.AddActionEvent(UnpackEventCode.OnUnpackDone, OnUnpackDone);
        GameEventHandler.AddActionEvent(GachaPackDockEventCode.OnOpenNowByGem, OnDockSlotOpened);
        GameEventHandler.AddActionEvent(GachaPackDockEventCode.OnOpenNowByRV, OnDockSlotOpened);
        GameEventHandler.AddActionEvent(GachaPackDockEventCode.OnOpenByProgression, OnDockSlotOpened);

        GameEventHandler.AddActionEvent(UnpackEventCode.OnUnpackStart, OnUnpackStart);
    }
    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(MainSceneEventCode.OnClickButtonCharacter, CheckButtonIsNew);
        GameEventHandler.RemoveActionEvent(PartManagementEventCode.OnPartNewStateChanged, CheckButtonIsNew);
        GameEventHandler.RemoveActionEvent(CharacterUIEventCode.OnGearButtonClick, CheckButtonIsNew);
        GameEventHandler.RemoveActionEvent(MainSceneEventCode.OnClickButtonCharacter, CheckButtonIsUpgradable);
        GameEventHandler.RemoveActionEvent(PartManagementEventCode.OnPartCardChanged, CheckButtonIsUpgradable);
        GameEventHandler.RemoveActionEvent(CharacterUIEventCode.OnGearButtonClick, CheckButtonIsUpgradable);
        GameEventHandler.RemoveActionEvent(CharacterUIEventCode.OnLockCharacterUIButton, LockButton);
        GameEventHandler.RemoveActionEvent(CharacterUIEventCode.OnUnLockCharacterUIButton, UnlockButton);
        GameEventHandler.RemoveActionEvent(TrophyRoadEventCode.OnTrophyRoadOpened, OnOpenrophyRoad);
        GameEventHandler.RemoveActionEvent(TrophyRoadEventCode.OnTrophyRoadClosed, OnOutTrophyRoad);
        GameEventHandler.RemoveActionEvent(UnpackEventCode.OnUnpackDone, OnUnpackDone);
        GameEventHandler.RemoveActionEvent(GachaPackDockEventCode.OnOpenNowByGem, OnDockSlotOpened);
        GameEventHandler.RemoveActionEvent(GachaPackDockEventCode.OnOpenNowByRV, OnDockSlotOpened);
        GameEventHandler.RemoveActionEvent(GachaPackDockEventCode.OnOpenByProgression, OnDockSlotOpened);

        GameEventHandler.RemoveActionEvent(UnpackEventCode.OnUnpackStart, OnUnpackStart);
    }

    private IEnumerator CheckFTUEGachaBoxEnd(float timeDelay)
    {
        yield return new WaitForSeconds(timeDelay);
        MultiImageButton button = gameObject.GetComponent<MultiImageButton>();
        if (button != null)
        {
            button.onClick.AddListener(() =>
            {
                if (equip_1FTUE.value && !upgradeFTUE.value)
                {
                    GameEventHandler.Invoke(BlockBackGround.LockDarkHasObject);
                    _ftueHand.gameObject.SetActive(false);
                }

                if (_buildTabFTUE.value)
                {
                    GameEventHandler.Invoke(LogFTUEEventCode.EndBuildTab);
                    GameEventHandler.Invoke(FTUEEventCode.OnBuildTabFTUE);
                }

                if (_buildTabFTUE.value && !equip_2FTUE.value)
                {
                    GameEventHandler.Invoke(BlockBackGround.LockDarkHasObject);
                }
                _ftueHand.gameObject.SetActive(false);
            });
        }
    }



    private void OnOpenrophyRoad()
    {
        m_IsOpenTrophyRoad = true;
        if (_buildTabFTUECR != null)
            StopCoroutine(_buildTabFTUECR);

        //FTUE
        if (m_PSFTUESO.FTUEStartFirstMatch.value && m_PSFTUESO.FTUEInFirstMatch.value && !m_PSFTUESO.FTUEOpenBuildUI.value)
            DisableBlock();
        if (m_PSFTUESO.FTUEStart2ndMatch.value && !m_PSFTUESO.FTUENewWeapon.value)
            DisableBlock();

        void DisableBlock()
        {
            FTUEHand.gameObject.SetActive(false);
            GameEventHandler.Invoke(PSFTUEBlockAction.Unblock, this.gameObject);
            m_CharacterUIBtn.onClick.RemoveListener(OnClickFTUEBuildUI);
        }
    }
    private void OnOutTrophyRoad()
    {
        m_IsOpenTrophyRoad = false;
        if (_buildTabFTUECR != null)
            StopCoroutine(_buildTabFTUECR);
        _buildTabFTUECR = CheckFTUEGachaBoxEnd(0);
        StartCoroutine(_buildTabFTUECR);

        //FTUE
        if (m_PSFTUESO.FTUEStartFirstMatch.value && m_PSFTUESO.FTUEInFirstMatch.value && !m_PSFTUESO.FTUEOpenBuildUI.value)
        {
            FTUEHand.gameObject.SetActive(true);
            GameEventHandler.Invoke(PSFTUEBlockAction.Block, this.gameObject, PSFTUEBubbleText.OpenBuild);

            if (m_CharacterUIBtn != null)
                m_CharacterUIBtn.onClick.AddListener(OnClickFTUEBuildUI);

            GameEventHandler.Invoke(PSLogFTUEEventCode.StartOpenBuildUI);
        }

        if (m_PSFTUESO.FTUEStart2ndMatch.value && !m_PSFTUESO.FTUENewWeapon.value)
        {
            FTUEHand.gameObject.SetActive(true);
            GameEventHandler.Invoke(PSFTUEBlockAction.Block, this.gameObject, PSFTUEBubbleText.NewWeapon);

            if (m_CharacterUIBtn != null)
                m_CharacterUIBtn.onClick.AddListener(OnClickFTUEBuildUI);

            if (!m_PSFTUESO.WeaponNewFTUEPPref.IsUnlocked())
                m_PSFTUESO.WeaponNewFTUEPPref.TryUnlockItem();

            GameEventHandler.Invoke(PSLogFTUEEventCode.StartNewWeapon_OpenBuildUI);
        }
    }

    void CheckButtonIsNew()
    {
        foreach (var item in partManagerSO)
        {
            if (item.HasOnePartIsNew())
            {
                HasNewTag = true;
                break;
            }
            else
            {
                HasNewTag = false;
            }
        }
    }

    void CheckButtonIsUpgradable()
    {
        foreach (var item in partManagerSO)
        {
            if (item.HasOnePartIsNew())
            {
                HasUpgradeTag = false;
                break;
            }
            if (item.HasOnePartIsUpgradable() && !HasNewTag)
            {
                HasUpgradeTag = true;
                break;
            }
            else
            {
                HasUpgradeTag = false;
            }
        }
    }

    private void LockButton()
    {
        Button characterButton = gameObject.GetComponent<Button>();
        if (characterButton != null)
            characterButton.interactable = false;
    }
    private void UnlockButton()
    {
        Button characterButton = gameObject.GetComponent<Button>();
        if (characterButton != null)
            characterButton.interactable = true;
    }

    private void OnUnpackStart()
    {

    }
    private void OnUnpackDone()
    {
        CheckFTUETab();
        isOpeningPackFromDockSlot = false;
    }

    private void OnDockSlotOpened(object[] _params)
    {
        isOpeningPackFromDockSlot = true;
    }

    private void CheckFTUETab()
    {
        PBPartManagerSO bodyManagerSO = Array.Find(partManagerSO, partManagerSO => partManagerSO.PartType == PBPartType.Body);
        PBChassisSO chassisSO = bodyManagerSO.CurrentPartInUse.Cast<PBChassisSO>();

        if (equip_1FTUE.value && !upgradeFTUE.value)
        {
            GameEventHandler.Invoke(BlockBackGround.LockDarkHasObject, this.gameObject);
            _ftueHand.gameObject.SetActive(true);
        }

        if (_buildTabFTUE.value && !equip_2FTUE.value && _lootBoxSlotTheFirstTime.value)
        {
            if (!chassisSO.IsTransformBot && chassisSO.AllPartSlots.Exists(slot => slot.PartType == PBPartType.Upper))
            {
                GameEventHandler.Invoke(BlockBackGround.LockDarkHasObject, this.gameObject);
                _ftueHand.gameObject.SetActive(true);
            }
            equip_2FTUE.value = true;
        }

        if (isOpeningPackFromDockSlot && _activeBuildTab.value && !_buildTabFTUE.value && _lootBoxSlotTheFirstTime.value)
        {
            if (!chassisSO.IsTransformBot && chassisSO.AllPartSlots.Exists(slot => slot.PartType == PBPartType.Upper))
            {
                GameEventHandler.Invoke(LogFTUEEventCode.StartBuildTab);
                GameEventHandler.Invoke(FTUEEventCode.OnBuildTabFTUE, this.gameObject);
                GameEventHandler.Invoke(BlockBackGround.LockDarkHasObject, this.gameObject);
                _ftueHand.gameObject.SetActive(true);
            }
            else
            {
                equip_2FTUE.value = true;
            }
            _buildTabFTUE.value = true;
        }

        // if (!skinUI_FTUE.value && highestAchievedPPrefFloatTracker.value >= SkinLocker.UNLOCK_TROPHY)
        // {
        //     GameEventHandler.Invoke(BlockBackGround.LockDarkHasObject, this.gameObject);
        //     _ftueHand.gameObject.SetActive(true);
        //     GameEventHandler.Invoke(LogFTUEEventCode.StartSkinUI);
        // }
    }

    private void OnClickFTUEBuildUI()
    {
        GameEventHandler.Invoke(PSFTUEBlockAction.Unblock, this.gameObject);
        if (m_PSFTUESO.FTUEStartFirstMatch.value && m_PSFTUESO.FTUEInFirstMatch.value && m_PSFTUESO.FTUEOpenBuildUI.value)
        {
            GameEventHandler.Invoke(PSLogFTUEEventCode.EndOpenBuildUI);
            GameEventHandler.Invoke(PSLogFTUEEventCode.StartOpenInfoPopup);
        }

        if (m_PSFTUESO.FTUEStart2ndMatch.value && !m_PSFTUESO.FTUENewWeapon.value)
        {
            GameEventHandler.Invoke(PSLogFTUEEventCode.EndNewWeapon_OpenBuildUI);
            GameEventHandler.Invoke(PSLogFTUEEventCode.StartNewWeapon_Equip);
        }
        m_CharacterUIBtn.onClick.RemoveListener(OnClickFTUEBuildUI);
    }
}
