using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CrowdLookAtController : MonoBehaviour
{
    private List<Renderer> m_Crowds;
    private Camera m_Camera;
    private void Awake()
    {
        m_Crowds = new List<Renderer>();
        m_Crowds = gameObject.GetComponentsInChildren<Renderer>()
                     .Where(r => r.gameObject != gameObject)
                     .ToList();

        m_Camera = MainCameraFindCache.Get();
    }

    private void Update()
    {
        if (m_Crowds == null || m_Camera == null) return;

        int count = m_Crowds.Count;
        if (count == 0) return;

        for (int i = 0; i < count; i++)
        {
            Renderer audienceRenderer = m_Crowds[i];
            if (audienceRenderer == null || !audienceRenderer.gameObject.activeInHierarchy || !audienceRenderer.isVisible)
                continue;

            Transform audienceTransform = audienceRenderer.transform;

            Vector3 originalEulerAngles = audienceTransform.eulerAngles;
            Vector3 directionToCamera = (m_Camera.transform.position - audienceTransform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(directionToCamera);
            Vector3 eulerAngles = lookRotation.eulerAngles;

            audienceTransform.eulerAngles = new Vector3(originalEulerAngles.x, eulerAngles.y, originalEulerAngles.z);
        }
    }


}
