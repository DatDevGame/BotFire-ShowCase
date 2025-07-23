using HightLightDebug;
using HyrphusQ.Events;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Cinemachine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class DebrisCache
{
    public Transform Object;
    public Transform ParrentObject;
    public Vector3 LocalPosition;
    public Vector3 LocalRotation;
    public Vector3 LocalScale;

    public void ResetBlendShape()
    {
        SkinnedMeshRenderer skinnedMeshRenderer = Object.GetComponent<SkinnedMeshRenderer>();
        for (int i = 0; i < GetBlendShapeCount(Object.gameObject); i++)
            skinnedMeshRenderer.SetBlendShapeWeight(i, 0);
    }
    public int GetBlendShapeCount(GameObject obj)
    {
        SkinnedMeshRenderer skinnedMeshRenderer = obj.GetComponent<SkinnedMeshRenderer>();
        if (skinnedMeshRenderer != null)
        {
            return skinnedMeshRenderer.sharedMesh.blendShapeCount;
        }
        return 0;
    }
}

[Serializable]
public class DebrisController
{
    public GameObject Debris;
    public bool IsEnableVFX;
}

[Serializable]
public class BlendShapeController
{
    public SkinnedMeshRenderer SkinnedMesh;
    public float BlendShapeValue;
}

[Serializable]
public class DestructionLevel
{
    [TabGroup("Debris & VFX", "Debris")]
    [LabelText("Debris Objects")]
    public List<DebrisController> DebrisController;

    [TabGroup("Debris & VFX", "VFX")]
    [LabelText("Destruction Effect")]
    public ParticleSystem DestructableTransition;

    [TabGroup("BlendShapes", "Level 1")]
    [LabelText("BlendShape 1 Debris")]
    public List<BlendShapeController> SkinMeshDebrisBlendShape_1;

    [TabGroup("BlendShapes", "Level 2")]
    [LabelText("BlendShape 2 Debris")]
    public List<BlendShapeController> SkinMeshDebrisBlendShape_2;

    [TabGroup("BlendShapes", "Level 3")]
    [LabelText("BlendShape 3 Debris")]
    public List<BlendShapeController> SkinMeshDebrisBlendShape_3;

    [TabGroup("BlendShapes", "Level 4")]
    [LabelText("BlendShape 4 Debris")]
    public List<BlendShapeController> SkinMeshDebrisBlendShape_4;

    [TabGroup("BlendShapes", "Level 5")]
    [LabelText("BlendShape 5 Debris")]
    public List<BlendShapeController> SkinMeshDebrisBlendShape_5;

    [FoldoutGroup("Threshold")]
    [LabelText("Health Threshold (%)"), Range(0, 100)]
    public float HealthThresholdPercentage;
}

public class ChassisDestructible : MonoBehaviour
{
    [FoldoutGroup("Configuration"), SerializeField]
    [LabelText("Explosion Force")]
    private float explosionForce = 3000;

    [FoldoutGroup("Configuration"), SerializeField]
    [LabelText("Explosion Radius")]
    private float explosionRadius = 40;

    [FoldoutGroup("Configuration"), SerializeField]
    [LabelText("Destruction Levels")]
    private List<DestructionLevel> destructionLevels = new List<DestructionLevel>();

    [FoldoutGroup("References"), SerializeField]
    [LabelText("Normal Body Model")]
    private GameObject m_BodyNomal;

    [FoldoutGroup("References"), SerializeField]
    [LabelText("Destructible Body Model")]
    private GameObject m_BodyDestructable;

    [FoldoutGroup("Resources"), SerializeField]
    [LabelText("Destructible Model")]
    private GameObject m_BodyDestructableModel;

    [FoldoutGroup("Resources"), SerializeField]
    [LabelText("Destruction Debris VFX")]
    private ParticleSystem m_DestructableDebrisVFX;

    private List<DebrisCache> m_DebrisCaches;
    private PBChassis m_PBChassis;
    private CameraShake m_CameraShake;
    private int currentLevel = -1;

    private void Awake()
    {
        m_PBChassis = gameObject.GetComponent<PBChassis>();
        SaveCacheObjectDestructable();
    }
    
    private void Start()
    {
        GameEventHandler.AddActionEvent(RobotStatusEventCode.OnRobotDamaged, OnPartReceiveDamage);
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(RobotStatusEventCode.OnRobotDamaged, OnPartReceiveDamage);
    }

    private void OnPartReceiveDamage(params object[] parameters)
    {
        if (parameters == null || parameters.Length <= 0)
            return;

        var damagedRobot = (PBRobot)parameters[0];
        var attacker = (IAttackable)parameters[3];

        if (damagedRobot != m_PBChassis.Robot)
            return;

        float healthPercentage = m_PBChassis.Robot.Health / m_PBChassis.Robot.MaxHealth;
        int newLevel = -1;

        if (m_BodyNomal == null || m_BodyDestructable == null)
            return;

        for (int i = 0; i < destructionLevels.Count; i++)
        {
            if (healthPercentage <= destructionLevels[i].HealthThresholdPercentage / 100)
            {
                newLevel = i;
            }
        }

        if (newLevel >= 0)
        {
            m_BodyNomal.SetActive(false);
            m_BodyDestructable.SetActive(true);
        }

        if (newLevel != currentLevel)
        {
            UpdateDestructionLevel(newLevel);
        }
    }

    private void UpdateDestructionLevel(int newLevel)
    {
        if (newLevel >= destructionLevels.Count) return;
        for (int level = currentLevel + 1; level <= newLevel; level++)
        {
            if (destructionLevels[level].DestructableTransition != null)
            {
                var destructableBodyVFX = Instantiate(destructionLevels[level].DestructableTransition);
                destructableBodyVFX.transform.SetParent(m_BodyDestructable.transform);
                destructableBodyVFX.transform.localPosition = Vector3.zero;
                destructableBodyVFX.transform.localScale = Vector3.one + ((Vector3.one * level) / 10);
                destructableBodyVFX.Play();
                Destroy(destructableBodyVFX.gameObject, 2f);
            }

            //Add Physic Debris
            destructionLevels[level].DebrisController.ForEach(debrisController =>
            {
                if (debrisController != null && debrisController.Debris != null)
                {
                    if (m_DestructableDebrisVFX != null && debrisController.IsEnableVFX)
                    {
                        var destructableDebrisVFX = Instantiate(m_DestructableDebrisVFX);
                        destructableDebrisVFX.transform.position = debrisController.Debris.transform.position;
                        destructableDebrisVFX.Play();
                        Destroy(destructableDebrisVFX.gameObject, 2f);

                        //Force Body
                        if (m_PBChassis != null)
                        {
                            if (m_PBChassis.RobotChassis.RobotBaseBody != null)
                            {
                                m_PBChassis.RobotChassis.RobotBaseBody.AddExplosionForce(20, destructableDebrisVFX.transform.position, explosionRadius, 1, ForceMode.Impulse);
                            }
                        }
                    }

                    Rigidbody rb = debrisController.Debris.GetComponent<Rigidbody>();
                    if (rb == null)
                        rb = debrisController.Debris.AddComponent<Rigidbody>();
                    if (rb == null) return;

                    MeshCollider mesCollider = rb.AddComponent<MeshCollider>();
                    Mesh mesh = new Mesh();
                    SkinnedMeshRenderer skinnedMeshRenderer = rb.GetComponent<SkinnedMeshRenderer>();

                    if (skinnedMeshRenderer != null)
                    {
                        skinnedMeshRenderer.BakeMesh(mesh);
                    }

                    if (mesCollider != null)
                    {
                        mesCollider.convex = true;
                        mesCollider.sharedMesh = mesh;
                    }
                    debrisController.Debris.transform.SetParent(m_PBChassis.Robot.transform);

                    // Apply explosion force for more dynamic debris
                    Vector3 explosionCenter = debrisController.Debris.transform.position + Vector3.up * 0.5f;
                    Vector3 randomForceDirection = UnityEngine.Random.insideUnitSphere.normalized;
                    randomForceDirection.y = Mathf.Abs(randomForceDirection.y);
                    float randomForceMagnitude = UnityEngine.Random.Range(2f, 5f);

                    float randomTorque = UnityEngine.Random.Range(-5f, 5f);

                    // Generate a random explosion force within a range
                    float randomExplosionForce = UnityEngine.Random.Range(explosionForce * 0.8f, explosionForce * 1.2f); // Adjust range as needed
                    float randomRadius = UnityEngine.Random.Range(explosionRadius * 0.8f, explosionRadius * 1.2f); // Randomize radius for variation

                    rb.AddExplosionForce(randomExplosionForce, explosionCenter, randomRadius, 1f, ForceMode.Impulse);
                    rb.AddForce(randomForceDirection * randomForceMagnitude, ForceMode.Impulse);
                    rb.AddTorque(UnityEngine.Random.insideUnitSphere * randomTorque, ForceMode.Impulse);

                    Destroy(debrisController.Debris, 3f);
                }
            });

            //Adjust Blendshape
            destructionLevels[level].SkinMeshDebrisBlendShape_1.ForEach(blendShape => 
            {
                blendShape?.SkinnedMesh?.SetBlendShapeWeight(0, blendShape.BlendShapeValue);
            });

            destructionLevels[level].SkinMeshDebrisBlendShape_2.ForEach(blendShape =>
            {
                blendShape?.SkinnedMesh?.SetBlendShapeWeight(1, blendShape.BlendShapeValue);
            });

            destructionLevels[level].SkinMeshDebrisBlendShape_3.ForEach(blendShape =>
            {
                blendShape?.SkinnedMesh?.SetBlendShapeWeight(2, blendShape.BlendShapeValue);
            });

            destructionLevels[level].SkinMeshDebrisBlendShape_4.ForEach(blendShape =>
            {
                blendShape?.SkinnedMesh?.SetBlendShapeWeight(3, blendShape.BlendShapeValue);
            });

            destructionLevels[level].SkinMeshDebrisBlendShape_5.ForEach(blendShape =>
            {
                blendShape?.SkinnedMesh?.SetBlendShapeWeight(4, blendShape.BlendShapeValue);
            });

            //Shake Camera
            if (m_CameraShake == null)
            {
                ICinemachineCamera liveCam = Camera.main.GetComponent<CinemachineBrain>().ActiveVirtualCamera;
                if(liveCam != null)
                    m_CameraShake = liveCam.VirtualCameraGameObject.GetComponent<CameraShake>();
            }
            if (m_CameraShake != null)
                m_CameraShake.ExplosionShake();
        }
        currentLevel = newLevel;
    }

    [Button]
    private void RevertDestructable()
    {
        if (m_DebrisCaches == null || m_DebrisCaches.Count <= 0)
            return;

        m_DebrisCaches.ForEach((debrisCache) =>
        {
            if (debrisCache.Object == null || debrisCache.ParrentObject == null)
                return;

            Rigidbody rigidbody = debrisCache.Object.GetComponent<Rigidbody>();
            if (rigidbody != null)
                Destroy(rigidbody);

            MeshCollider meshCollider = debrisCache.Object.GetComponent<MeshCollider>();
            if (meshCollider != null)
                Destroy(meshCollider);

            debrisCache.Object.SetParent(debrisCache.ParrentObject);
            debrisCache.Object.localPosition = debrisCache.LocalPosition;
            debrisCache.Object.localEulerAngles = debrisCache.LocalRotation;
            debrisCache.Object.localScale = debrisCache.LocalScale;
            debrisCache.ResetBlendShape();
        });
    }

    private void SaveCacheObjectDestructable()
    {
        m_DebrisCaches = new List<DebrisCache>();
        if (m_BodyDestructable == null)
            return;

        List<Transform> allChilds = new List<Transform>();
        allChilds = GetAllChildrenRecursive(m_BodyDestructable.transform);
        for (int i = 0; i < allChilds.Count; i++)
        {
            DebrisCache debrisCache = new DebrisCache();
            debrisCache.Object = allChilds[i];
            debrisCache.ParrentObject = allChilds[i].parent;
            debrisCache.LocalPosition = allChilds[i].localPosition;
            debrisCache.LocalRotation = allChilds[i].localEulerAngles;
            debrisCache.LocalScale = allChilds[i].localScale;
            m_DebrisCaches.Add(debrisCache);
        }

        List<Transform> GetAllChildrenRecursive(Transform parent)
        {
            List<Transform> children = new List<Transform>();

            foreach (Transform child in parent)
            {
                children.Add(child);
                children.AddRange(GetAllChildrenRecursive(child));
            }
            return children;
        }
    }


#if UNITY_EDITOR
    [Button]
    private void SetUpDestructable()
    {
        if (m_BodyDestructableModel == null)
        {
            DebugPro.AquaBold($"Body Destructable Is Null");
        }
        m_BodyDestructable = (GameObject)PrefabUtility.InstantiatePrefab(m_BodyDestructableModel, m_BodyNomal.transform.parent);
        m_BodyDestructable.transform.position = m_BodyNomal.transform.position;
        m_BodyNomal.gameObject.SetActive(true);
        m_BodyDestructable.gameObject.SetActive(false);

    }
#endif
}
