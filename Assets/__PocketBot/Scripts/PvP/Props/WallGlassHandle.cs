using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WallGlassHandle : MonoBehaviour
{
    [SerializeField] private GameObject m_WallDefault;
    [SerializeField] private List<Renderer> m_WallRenderer;
    [SerializeField] private List<Rigidbody> m_Rigidbodies;
    [SerializeField] private List<OnCollisionEnableRB> m_OnCollisionEnableRB;

    private void Start()
    {
        m_OnCollisionEnableRB.ForEach((v) => 
        {
            v.OnGravity += OnCollisionEnterHandle;
        });
    }

    private void OnCollisionEnterHandle()
    {
        OnBreak();
    }
    private void OnBreak()
    {
        if (m_WallDefault.activeSelf)
        {
            m_WallDefault.SetActive(false);
            m_WallRenderer
                .Where(x => x != null)
                .ToList()
                .ForEach(v => v.enabled = true);
        }
    }
}
