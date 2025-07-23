using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HyrphusQ.SerializedDataStructure;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

public class SeatController : MonoBehaviour
{
    [SerializeField]
    private float m_BoxSize = 5f;
    [SerializeField, Range(0f, 1f)]
    private float m_DistancePercentage = 0.5f;
    // [SerializeField]
    // private float m_MaxAngle = 90f;
    [SerializeField]
    private SerializedDictionary<Material, Material> m_MaterialDictionary;

    private LayerMask m_SeatLayerMask;
    private PBRobot m_LocalPlayerRobot;
    private Camera m_MainCam;
    // private float[] m_OriginalYAngles;
    private Renderer[] m_AudienceRenderers;
    private Transform[] m_Seats;
    // [ShowInInspector]
    // private Transform[] m_LogAudience;

    private void Awake()
    {
        m_MainCam = MainCameraFindCache.Get();
        m_SeatLayerMask = LayerMask.GetMask("Seat");
        m_Seats = FetchSeats();
        m_AudienceRenderers = FetchAudienceRenderers();
        UpdateMaterials();
    }

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => PBFightingStage.Instance && PBFightingStage.Instance.isFightingReady);
        m_LocalPlayerRobot = PBRobot.allFightingRobots.First(robot => robot.PersonalInfo.isLocal);
    }

    private void OnDrawGizmos()
    {
        if (m_LocalPlayerRobot == null)
            return;
        Gizmos.color = Color.green;
        Vector3 dir = m_LocalPlayerRobot.GetTargetPoint() - m_MainCam.transform.position;
        Vector3 from = m_MainCam.transform.position;
        Vector3 to = m_MainCam.transform.position + dir * m_DistancePercentage;
        Vector3 center = (from + to) / 2f;
        Vector3 size = Vector3.one * m_BoxSize;
        size.z = dir.magnitude * m_DistancePercentage;
        Gizmos.matrix = Matrix4x4.TRS(center, Quaternion.LookRotation(dir.normalized), Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, size);
    }

    private void Update()
    {
        if (m_LocalPlayerRobot != null)
        {
            m_Seats.ForEach(item => item.gameObject.SetActive(true));
            Vector3 dir = m_LocalPlayerRobot.GetTargetPoint() - m_MainCam.transform.position;
            Vector3 from = m_MainCam.transform.position;
            Vector3 to = m_MainCam.transform.position + dir * m_DistancePercentage;
            Vector3 center = (from + to) / 2f;
            Collider[] detectedColliders = Physics.OverlapBox(center, m_BoxSize / 2f * Vector3.one, Quaternion.LookRotation(dir.normalized), m_SeatLayerMask, QueryTriggerInteraction.Collide);
            for (int i = 0; i < detectedColliders.Length; i++)
            {
                detectedColliders[i].transform.parent.gameObject.SetActive(false);
            }
        }
        if (m_AudienceRenderers != null && m_AudienceRenderers.Length > 0 && m_MainCam != null)
        {
            for (int i = 0; i < m_AudienceRenderers.Length; i++)
            {
                Renderer audienceRenderer = m_AudienceRenderers[i];
                if (audienceRenderer == null)
                    continue;
                if (!audienceRenderer.gameObject.activeInHierarchy || !audienceRenderer.isVisible)
                    continue;
                Vector3 originalEulerAngles = audienceRenderer.transform.eulerAngles;
                Vector3 eulerAngles = Quaternion.LookRotation((m_MainCam.transform.position - audienceRenderer.transform.position).normalized).eulerAngles;
                // float angleY = eulerAngles.y;
                // float min = m_OriginalYAngles[i] - m_MaxAngle / 2f;
                // float max = m_OriginalYAngles[i] + m_MaxAngle / 2f;
                // angleY = Mathf.Clamp(angleY, min, max);
                //audienceRenderer.transform.eulerAngles = new Vector3(originalEulerAngles.x, angleY, originalEulerAngles.z);
                audienceRenderer.transform.eulerAngles = new Vector3(originalEulerAngles.x, eulerAngles.y, originalEulerAngles.z);
                // if (m_LogAudience != null && m_LogAudience.Contains(audienceRenderer.transform))
                // {
                //     Debug.Log($"{audienceRenderer.transform}: {m_OriginalYAngles[i]} - {eulerAngles.y} - {angleY} - {min} - {max} - {audienceRenderer.transform.eulerAngles.y}", audienceRenderer.transform);
                // }
            }
        }
    }

    private Transform[] FetchSeats()
    {
        var seats = new List<Transform>();
        var seatsTransform = transform.FindRecursive("Seats");
        for (int i = 0; i < seatsTransform.childCount; i++)
        {
            seats.Add(seatsTransform.GetChild(i));
        }
        return seats.ToArray();
    }

    private Renderer[] FetchAudienceRenderers()
    {
        var audiences = new List<Renderer>();
        // var yAngles = new List<float>();
        for (int i = 0; i < m_Seats.Length; i++)
        {
            for (int j = 0; j < m_Seats[i].childCount; j++)
            {
                for (int k = 0; k < m_Seats[i].GetChild(j).childCount; k++)
                {
                    audiences.Add(m_Seats[i].GetChild(j).GetChild(k).GetComponent<Renderer>());
                }
            }
        }
        return audiences.ToArray();
    }

    [Button]
    private void UpdateMaterials()
    {
        if (m_MaterialDictionary != null && m_MaterialDictionary.Count > 0)
        {
            var renderers = GetComponentsInChildren<MeshRenderer>();
            foreach (var renderer in renderers)
            {
                var sharedMaterials = renderer.sharedMaterials;
                for (int i = 0; i < sharedMaterials.Length; i++)
                {
                    if (sharedMaterials[i] != null && m_MaterialDictionary.ContainsKey(sharedMaterials[i]))
                    {
                        sharedMaterials[i] = m_MaterialDictionary[sharedMaterials[i]];
                    }
                }
                renderer.sharedMaterials = sharedMaterials;
            }
        }
    }
}