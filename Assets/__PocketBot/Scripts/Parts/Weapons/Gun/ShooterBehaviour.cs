using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LatteGames;
using LatteGames.GameManagement;
using LatteGames.Template;
using Sirenix.OdinInspector;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using static PBRobot;

public class ShooterBehaviour : PartBehaviour, IBoostFireRate
{
    [SerializeField] ArrowCollider arrowPrefab;
    [SerializeField] Transform rocketContainer;
    [SerializeField] SoundID shootSoundID;
    [SerializeField] SkinnedMeshRenderer crossBowRenderer;
    [SerializeField] float soundAmount = 0.05f;
    [SerializeField] float waitForLaunchDuration = 0.5f;
    [SerializeField] float loadArrowAnimationDuration = 0.25f;
    [SerializeField] float shootArrowAnimationDuration = 0.1f;
    [SerializeField] float pullArrowDistance = 0.15f;
    [SerializeField] int amountLimit = 10;
    [SerializeField] int attachedArrowAmountLimit = 10;

    Queue<ArrowCollider> arrowQueue = new();
    Queue<GameObject> attachedArrowQueue = new();
    GameObject rocketWorldSpaceContainer;
    private PBChassis pbChassisBody;

    float holdingArrowDuration;

    [SerializeField, BoxGroup("Visual")]private List<SkinnedMeshRenderer> m_SkinnedMeshesBooster;

    //Speed Up Handle
    private bool m_IsSpeedUp = false;
    private int m_StackSpeedUp;
    private float m_ObjectTimeScale;
    private float m_TimeScaleOrginal => TimeManager.Instance.originalScaleTime;
    private float m_BoosterPercent;

    protected override IEnumerator Start()
    {
        m_ObjectTimeScale = m_TimeScaleOrginal;

        pbChassisBody = gameObject.GetComponentInParent<PBChassis>();
        holdingArrowDuration = AttackCycleTime - waitForLaunchDuration;
        arrowPrefab.PartBehaviour = this;
        yield return base.Start();
        yield return new WaitForEndOfFrame();
        rocketWorldSpaceContainer = new GameObject($"{PbPart.RobotBaseBody.name}_ArrowContainer");
        arrowPrefab.gameObject.SetActive(false);
        for (var i = 0; i < amountLimit; i++)
        {
            var instance = Instantiate(arrowPrefab, rocketContainer);
            instance.gameObject.layer = gameObject.layer;
            instance.PartBehaviour = this;
            instance.attachedArrowQueue = attachedArrowQueue;

            arrowQueue.Enqueue(instance);
        }
        for (var i = 0; i < attachedArrowAmountLimit; i++)
        {
            var meshOnly = new GameObject("Arrow");
            meshOnly.transform.SetParent(rocketWorldSpaceContainer.transform, true);
            var meshFilter = meshOnly.AddComponent<MeshFilter>();
            var meshRenderer = meshOnly.AddComponent<MeshRenderer>();
            var m_MeshFilter = arrowPrefab.gameObject.GetComponent<MeshFilter>();
            var m_MeshRenderer = arrowPrefab.gameObject.GetComponent<MeshRenderer>();
            meshFilter.sharedMesh = m_MeshFilter.sharedMesh; // Prevent to creat a mesh instance
            meshRenderer.sharedMaterials = m_MeshRenderer.sharedMaterials;
            var navMeshObstacle = meshOnly.AddComponent<NavMeshModifier>();
            navMeshObstacle.ignoreFromBuild = true;
            meshOnly.SetActive(false);

            attachedArrowQueue.Enqueue(meshOnly);
        }
        StartCoroutine(CR_Shoot());
    }

    private void OnDestroy()
    {
        foreach (var arrow in arrowQueue)
        {
            Destroy(arrow);
        }
        foreach (var arrow in attachedArrowQueue)
        {
            Destroy(arrow);
        }
        Destroy(rocketWorldSpaceContainer);
    }

    protected virtual IEnumerator CR_Shoot()
    {
        var waitUntilEnable = new WaitUntil(() => { return this.enabled; });
        while (true)
        {
            // Wait until the component is enabled
            yield return waitUntilEnable;

            float bodySpeed = 1f;
            if (pbChassisBody != null)
            {
                bodySpeed = pbChassisBody.RobotBaseBody.velocity.magnitude <= 1f
                    ? 1f
                    : pbChassisBody.RobotBaseBody.velocity.magnitude;
            }

            var instance = arrowQueue.Dequeue();
            arrowQueue.Enqueue(instance);
            instance.gameObject.SetActive(true);
            instance.Reset();
            instance.transform.SetParent(rocketContainer);
            instance.transform.position = arrowPrefab.transform.position;
            instance.transform.rotation = arrowPrefab.transform.rotation;

            // Load arrow animation
            yield return CustomLerp(Mathf.Min(loadArrowAnimationDuration, holdingArrowDuration * 0.95f), m_ObjectTimeScale, (t) =>
            {
                instance.transform.position = Vector3.Lerp(
                    arrowPrefab.transform.position,
                    arrowPrefab.transform.position - instance.transform.up * pullArrowDistance,
                    t
                );
                crossBowRenderer.SetBlendShapeWeight(0, Mathf.Lerp(100, 0, t));
            });

            yield return CustomWaitForSeconds(holdingArrowDuration, m_ObjectTimeScale);

            // Shoot arrow animation
            yield return CustomLerp(Mathf.Min(shootArrowAnimationDuration, waitForLaunchDuration * 0.95f), m_ObjectTimeScale, (t) =>
            {
                instance.transform.position = Vector3.Lerp(
                    arrowPrefab.transform.position - instance.transform.up * pullArrowDistance,
                    arrowPrefab.transform.position,
                    t
                );
                crossBowRenderer.SetBlendShapeWeight(0, Mathf.Lerp(0, 100, t));
                if (t >= 1)
                {
                    instance.transform.SetParent(rocketWorldSpaceContainer.transform, true);
                    instance.Launch(bodySpeed);
                    SoundManager.Instance.PlaySFX(shootSoundID, PBSoundUtility.IsOnSound() ? soundAmount : 0);
                }
            });

            yield return CustomWaitForSeconds(waitForLaunchDuration, m_ObjectTimeScale);
        }
    }
    private IEnumerator CustomLerp(float duration, float customTimeScale, System.Action<float> callback)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime * customTimeScale;
            float t = Mathf.Clamp01(elapsed / duration);
            callback?.Invoke(t);
            yield return null;
        }
        callback?.Invoke(1f); // Ensure final state
    }

    private IEnumerator CustomWaitForSeconds(float duration, float customTimeScale)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime * customTimeScale;
            yield return null;
        }
    }
    public void BoostSpeedUpFire(float boosterPercent)
    {
        m_BoosterPercent += boosterPercent;

        //Enable VFX
        if (m_SkinnedMeshesBooster != null && m_SkinnedMeshesBooster.Count > 0)
            PBBoosterVFXManager.Instance.PlayBoosterSpeedUpAttackWeapon(pbPart, m_SkinnedMeshesBooster);

        m_StackSpeedUp++;
        m_IsSpeedUp = true;
        m_ObjectTimeScale += (m_TimeScaleOrginal * boosterPercent);
    }
    public void BoostSpeedUpStop(float boosterPercent)
    {
        m_StackSpeedUp--;
        if (m_StackSpeedUp <= 0)
        {
            //Disable VFX
            PBBoosterVFXManager.Instance.StopBoosterSpeedUpAttackWeapon(pbPart);

            m_IsSpeedUp = false;
            m_ObjectTimeScale = m_TimeScaleOrginal;
            m_BoosterPercent = 0;
        }
        else
        {
            m_BoosterPercent -= boosterPercent;
            m_ObjectTimeScale -= (m_TimeScaleOrginal * boosterPercent);
        }
    }
    public bool IsSpeedUp() => m_IsSpeedUp;
    public int GetStackSpeedUp() => m_StackSpeedUp;
    public float GetPercentSpeedUp() => m_BoosterPercent;
}