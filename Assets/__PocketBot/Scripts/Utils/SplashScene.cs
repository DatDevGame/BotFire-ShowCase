using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using LatteGames.GameManagement;
using System;

public class SplashScene : MonoBehaviour
{
    [SerializeField] float m_LoadingTime = 2f;
    [SerializeField] Slider m_LoadingSlider;
    [SerializeField] private PSFTUESO m_PSFTUESO;

    private void Start()
    {
        // float time = 0f;
        // while (time < m_LoadingTime)
        // {
        //     yield return new WaitForEndOfFrame();
        //     time += Time.deltaTime;
        //     m_LoadingSlider.value = Mathf.InverseLerp(0f, m_LoadingTime, time);
        // }

        //if (!FTUE_Lootbox.value)
        //{
        //    LoadLootBoxScene(); yield break;
        //}

        PBRemoteConfigManager.onRemoteConfigReady += OnRemoteConfigReady;
    }

    private void OnRemoteConfigReady()
    {
        PBRemoteConfigManager.onRemoteConfigReady -= OnRemoteConfigReady;
        PBSkinAssetResourceManager.onLoadDefaultSkinsCompleted += OnLoadDefaultSkinsCompleted;

        void OnLoadDefaultSkinsCompleted()
        {
            PBSkinAssetResourceManager.onLoadDefaultSkinsCompleted -= OnLoadDefaultSkinsCompleted;

            if (!m_PSFTUESO.FTUEFightPPref.value)
            {
                LoadFightingScene();
            }
            else
            {
                LoadMainScene();
            }
        }
    }

    private void LoadMainScene()
    {
        SceneManager.LoadScene(SceneName.MainScene, isPushToStack: false);
    }

    private void LoadFightingScene()
    {
        SceneManager.LoadScene(SceneName.FTUE_PocketShooter, isPushToStack: false);
    }

    private void LoadLootBoxScene()
    {
        SceneManager.LoadScene(SceneName.FTUE_LootBox, isPushToStack: false);
    }
}