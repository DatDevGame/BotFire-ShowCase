using DG.Tweening;
using HyrphusQ.Helpers;
using LatteGames;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Animations;

public class CoinRewardPVP : MonoBehaviour
{
    [Title("📝 UI References")]
    [SerializeField, BoxGroup("UI Elements")]
    private TextMeshProUGUI m_DescText;

    [Title("🔧 Object References")]
    [BoxGroup("Coin Prefabs"), InlineEditor]
    [SerializeField] private GameObject m_CoinPrefab;
    [BoxGroup("Coin Prefabs")]
    [SerializeField] private GameObject m_CoinModel;

    [BoxGroup("Physics Settings"), InlineProperty]
    [SerializeField] private BoxCollider m_BoxPhysics;
    [BoxGroup("Physics Settings"), InlineProperty]
    [SerializeField] private Rigidbody m_Rigidbody;
    [BoxGroup("Physics Settings"), InlineProperty]
    [SerializeField] private LockRotationBooster m_LockRotationBooster;

    [Title("⚙️ Coin Spawn Settings")]
    [BoxGroup("Spawn Settings")]
    [SerializeField, MinValue(0.1f)] private float m_SpawnRadius = 1f;
    [BoxGroup("Spawn Settings")]
    [SerializeField, MinValue(1f)] private float m_ForcePower = 5f;

    private List<GameObject> m_Coins;
    private Camera m_MainCam;

    private void Start()
    {
        m_CoinModel.SetActive(false);
        m_MainCam = MainCameraFindCache.Get();
    }
    private void Update()
    {
        if (m_DescText.gameObject.activeSelf)
        {
            m_DescText.transform.forward = (m_DescText.transform.position - m_MainCam.transform.position).normalized;
        }
    }
    public void Spawn(IAttackable attacker, int coinAmount)
    {
        if (attacker == null) return;
        if (attacker is PBPart part)
        {
            StartCoroutine(SpawnCoins(part, coinAmount));
        }
    }

    private IEnumerator SpawnCoins(PBPart part, int coinAmount)
    {
        if(part == null) 
            yield break;

        m_Coins = new List<GameObject>();

        int coinCount = Mathf.Max(1, Mathf.RoundToInt(coinAmount / 3));
        for (int i = 0; i < coinCount; i++)
        {
            Vector3 spawnPos = transform.position;

            GameObject coin = Instantiate(m_CoinPrefab, spawnPos, Quaternion.identity);
            m_Coins.Add(coin);

            Rigidbody coinRb = coin.GetComponent<Rigidbody>();

            Vector3 randomDirection = Random.insideUnitSphere.normalized;
            Vector3 launchForce = randomDirection * m_ForcePower + Vector3.up * m_ForcePower * 0.5f;

            coinRb.AddForce(launchForce, ForceMode.Impulse);

            Vector3 randomTorque = Random.insideUnitSphere * 10f;
            coinRb.AddTorque(randomTorque, ForceMode.Impulse);
        }

        if (part.RobotChassis != null)
        {
            if (part.RobotChassis.Robot != null && part.RobotChassis.Robot.PersonalInfo.isLocal)
            {
                CurrencyManager.Instance.AcquireWithoutLogEvent(CurrencyType.Standard, coinAmount);
            }
        }
        HandleText(part.RobotChassis.Robot, coinAmount);
        yield return new WaitForSeconds(1.5f);
        StartCoroutine(AttractCoin());

    }

    private IEnumerator AttractCoin()
    {
        List<GameObject> coinsToRemove = new List<GameObject>();

        for (int i = 0; i < m_Coins.Count; i++)
        {
            if (m_Coins[i] != null)
            {
                GameObject coin = m_Coins[i];
                float randomDelay = Random.Range(0f, 0.05f);
                yield return new WaitForSeconds(randomDelay);

                coin.transform.DOScale(Vector3.zero, 0.3f);
                coinsToRemove.Add(coin);
            }
        }
        foreach (GameObject coin in coinsToRemove)
            m_Coins.Remove(coin);

        Destroy(gameObject);
    }

    private void HandleText(PBRobot robot, int coinAmount)
    {
        m_DescText.SetText($"+{coinAmount} Coin");
        m_DescText.gameObject.SetActive(true);
        var positionConstraint = m_DescText.GetOrAddComponent<PositionConstraint>();
        positionConstraint.AddSource(new ConstraintSource() { weight = 1f, sourceTransform = robot.ChassisInstance.RobotBaseBody.transform });
        positionConstraint.translationOffset = new Vector3(0f, 3f);
        positionConstraint.constraintActive = true;
        StartCoroutine(CommonCoroutine.Delay(AnimationDuration.SHORT, false, () =>
        {
            positionConstraint.RemoveSource(0);
            m_DescText.gameObject.SetActive(false);
        }));
    }
}
