using System;
using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using HyrphusQ.SerializedDataStructure;
using LatteGames;
using LatteGames.Template;
using LatteGames.Utils;
using TMPro;
using UnityEngine;

public class KillingSystem : MonoBehaviour
{
    [SerializeField] ModeVariable m_CurrentChosenModeVariable;
    [SerializeField] PvPArenaVariable m_ChosenArenaVariable;
    [SerializeField] SerializedDictionary<KillType, KillFXConfig> m_KillFXConfigs;
    [SerializeField] Rigidbody coinPrefab;
    [SerializeField] TextMeshProUGUI earnCoinTxt;
    [SerializeField] SoundID collectMoneySFX;
    [SerializeField] float showingEarnCoinTxtDuration = 1;
    [SerializeField] AnimationCurve showingEarnCoinCurve;
    [SerializeField] Transform worldCanvas;
    [SerializeField] float delayToAbsorbCoin = 3;
    [SerializeField] Vector2 upForceMagnitudeRange;
    [SerializeField] AnimationCurve coinAmountCurve;
    [SerializeField] bool isFTUE;

    Dictionary<PBRobot, int> killStreaks = new Dictionary<PBRobot, int>();
    bool isFirstBlood;
    KillingSystemUI killingSystemUI;
    ObjectPooling<Rigidbody> coinPool;
    Transform coinContainer;

    private void Awake()
    {
        if (isFTUE || m_CurrentChosenModeVariable.value == Mode.Normal)
        {
            return;
        }
        coinContainer = new GameObject("CoinContainer").transform;
        coinContainer.SetParent(transform);
        coinPool = new ObjectPooling<Rigidbody>(
            instantiateMethod: () =>
            {
                var coinModel = Instantiate(coinPrefab);
                coinModel.transform.SetParent(coinContainer, false);
                coinModel.transform.position = new Vector3(0, 100000);
                return coinModel;
            },
            destroyMethod: (obj) => { Destroy(obj.gameObject); }
        )
        {
            PregenerateOffset = 3
        };
        killingSystemUI = GetComponentInChildren<KillingSystemUI>();
        GameEventHandler.AddActionEvent(RobotStatusEventCode.OnModelSpawned, OnModelSpawned);
        GameEventHandler.AddActionEvent(KillingSystemEvent.OnKillDeathInfosUpdated, OnKillDeathInfosUpdated);
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(RobotStatusEventCode.OnModelSpawned, OnModelSpawned);
        GameEventHandler.RemoveActionEvent(KillingSystemEvent.OnKillDeathInfosUpdated, OnKillDeathInfosUpdated);
    }

    void OnModelSpawned(params object[] parameters)
    {
        var robot = parameters[0] as PBRobot;
        killStreaks.Set(robot, 0);
    }

    void OnKillDeathInfosUpdated(object[] parameters)
    {
        var killAndDeathTracker = parameters[0] as KillAndDeathTracker;
        var killer = parameters[1] as PBRobot;
        var victim = parameters[2] as PBRobot;
        if (killer != null)
        {
            if (!killStreaks.ContainsKey(killer))
            {
                killStreaks.Add(killer, 0);
            }
            killStreaks[killer] = Mathf.Clamp(killStreaks[killer] + 1, (int)KillType.FirstBlood, (int)KillType.MegaKill);

            var killFXConfig = !isFirstBlood ? m_KillFXConfigs[KillType.FirstBlood] : m_KillFXConfigs[(KillType)killStreaks[killer]];
            if (killer.PersonalInfo.isLocal)
            {
                TriggerKillFX(killFXConfig);
                var coinRewardModule = m_ChosenArenaVariable.value.GetReward<CurrencyRewardModule>(item => item.CurrencyType == CurrencyType.Standard);
                var coinRewardAmount = Mathf.RoundToInt(coinRewardModule.Amount * killFXConfig.standardCurrencyPercent);
                CurrencyManager.Instance.AcquireWithoutLogEvent(CurrencyType.Standard, coinRewardAmount);
                PlayEarnCoinAnimation(killFXConfig, killer, victim);
            }

            if (!isFirstBlood)
            {
                isFirstBlood = true;
            }
        }
        else if (victim != null)
        {
            if (!killStreaks.ContainsKey(victim))
            {
                killStreaks.Add(victim, 0);
            }
            killStreaks[victim] = 0;
        }
    }

    void TriggerKillFX(KillFXConfig killFXConfig)
    {
        SoundManager.Instance.PlaySFX(killFXConfig.m_Sfx, 1.5f);
        killingSystemUI.Show(killFXConfig.title);
    }

    void PlayEarnCoinAnimation(KillFXConfig killFXConfig, PBRobot killer, PBRobot victim)
    {
        StartCoroutine(CR_EarnCoinAnimation(killFXConfig, killer, victim));
    }

    IEnumerator CR_EarnCoinAnimation(KillFXConfig killFXConfig, PBRobot killer, PBRobot victim)
    {
        var coinRewardModule = m_ChosenArenaVariable.value.GetReward<CurrencyRewardModule>(item => item.CurrencyType == CurrencyType.Standard);
        var coinRewardAmount = Mathf.RoundToInt(coinRewardModule.Amount * killFXConfig.standardCurrencyPercent);

        int coinAmount = Mathf.RoundToInt(coinAmountCurve.Evaluate(killFXConfig.standardCurrencyPercent));
        List<Rigidbody> coins = new List<Rigidbody>();
        for (var i = 0; i < coinAmount; i++)
        {
            var coin = coinPool.Get();
            coin.gameObject.SetActive(true);
            coin.isKinematic = false;
            coin.detectCollisions = true;
            coin.transform.position = victim.ChassisInstance.CarPhysics.transform.position;
            coin.AddForce(Vector3.up * UnityEngine.Random.Range(upForceMagnitudeRange.x, upForceMagnitudeRange.y) + UnityEngine.Random.insideUnitSphere * 2, ForceMode.VelocityChange);
            coin.AddTorque(UnityEngine.Random.insideUnitSphere, ForceMode.VelocityChange);
            coins.Add(coin);
        }
        yield return new WaitForSeconds(delayToAbsorbCoin);
        foreach (var coin in coins)
        {
            coin.isKinematic = true;
            coin.detectCollisions = false;
        }
        int currentCoinAmount = coinAmount;
        float coinSpeed = 5;
        while (currentCoinAmount > 0)
        {
            foreach (var coin in coins)
            {
                if (coin.gameObject.activeInHierarchy)
                {
                    coin.transform.position = Vector3.MoveTowards(coin.transform.position, killer.ChassisInstance.CarPhysics.transform.position, coinSpeed * Time.deltaTime);
                    if ((coin.transform.position - killer.ChassisInstance.CarPhysics.transform.position).magnitude <= 0.03f)
                    {
                        coin.gameObject.SetActive(false);
                        HapticManager.Instance.PlayFlashHaptic(HapticTypes.HeavyImpact);
                        SoundManager.Instance.PlaySFX_3D_Pitch(collectMoneySFX, killer.ChassisInstance.CarPhysics.transform.position, true);
                        currentCoinAmount--;
                    }
                }
            }
            coinSpeed++;
            yield return null;
        }
        foreach (var coin in coins)
        {
            coinPool.Add(coin);
        }
        var earnCoinTxtInstance = Instantiate(earnCoinTxt);
        earnCoinTxtInstance.text = earnCoinTxtInstance.text.Replace("{value}", coinRewardAmount.ToString());
        earnCoinTxtInstance.transform.SetParent(worldCanvas);
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / showingEarnCoinTxtDuration;
            earnCoinTxtInstance.transform.position = killer.ChassisInstance.CarPhysics.transform.position + Vector3.up * Mathf.Lerp(1.5f, 2.3f, t);
            earnCoinTxtInstance.transform.localScale = Vector3.one * showingEarnCoinCurve.Evaluate(t);
            yield return null;
        }
        Destroy(earnCoinTxtInstance.gameObject);
    }

    [Serializable]
    public class KillFXConfig
    {
        public string title;
        public float standardCurrencyPercent;
        public SoundID m_Sfx;
    }
}

[EventCode]
public enum KillingSystemEvent
{
    /// <summary>
    /// This event is raised when updating the robot's kill and death info.
    /// <para><typeparamref name="KillAndDeathTracker"/>: KillAndDeathTracker </para>
    /// <para><typeparamref name="PBRobot"/>: Killer</para>
    /// <para><typeparamref name="PBRobot"/>: Victim</para>
    /// </summary>
    OnKillDeathInfosUpdated,
}

public enum KillType
{
    FirstBlood,
    Kill,
    DoubleKill,
    TripleKill,
    UltraKill,
    MegaKill
}
