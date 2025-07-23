using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using Sirenix.Utilities;

public class GarageModelHandle : MonoBehaviour
{
    public MeshRenderer Renderer => m_Renderer;

    [SerializeField, BoxGroup("Ref")] private GameObject m_DissolveObject;
    [SerializeField, BoxGroup("Ref")] private GameObject m_GarageObject;
    [SerializeField, BoxGroup("Ref")] private MeshRenderer m_Renderer;
    [SerializeField, BoxGroup("Ref")] private List<MeshRenderer> m_ObjectRoomMeshrenderer;
    [SerializeField, BoxGroup("Ref")] private List<SkinnedMeshRenderer> m_ObjectRoomSkinedMeshrenderer;

    private List<Material> m_DissolveMaterials;
    private List<Material> m_ObjectMaterials;
    public void SetMeshRenderer(List<Material> materials)
    {
        Material[] materialsArr = materials.ToArray();
        m_Renderer.materials = materialsArr;
        m_DissolveMaterials = materials;

        //Meshrenderer
        if(m_ObjectRoomMeshrenderer != null && m_ObjectRoomMeshrenderer.Count > 0)
        {
            m_ObjectMaterials = new List<Material>();
            for (int i = 0; i < m_ObjectRoomMeshrenderer.Count; i++)
            {
                Material objectMaterial = new Material(m_ObjectRoomMeshrenderer[i].material);
                m_ObjectRoomMeshrenderer[i].material = objectMaterial;
                m_ObjectMaterials.Add(objectMaterial);
            }
        }

        //Skined Meshrenderer
        if (m_ObjectRoomSkinedMeshrenderer != null && m_ObjectRoomSkinedMeshrenderer.Count > 0)
        {
            m_ObjectMaterials = new List<Material>();
            for (int i = 0; i < m_ObjectRoomSkinedMeshrenderer.Count; i++)
            {
                Material objectMaterial = new Material(m_ObjectRoomSkinedMeshrenderer[i].material);
                m_ObjectRoomSkinedMeshrenderer[i].material = objectMaterial;
                m_ObjectMaterials.Add(objectMaterial);
            }
        }
    }

    public void Show()
    {
        this.gameObject.SetActive(true);
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }

    public void ShowDissolve(float Duration, Action OnCompletedAnimationDissolve = null)
    {
        m_Renderer.enabled = true;
        if (m_DissolveObject != null)
            m_DissolveObject.SetActive(true);
        if (m_GarageObject != null)
            m_GarageObject.SetActive(false);

        m_DissolveMaterials.ForEach(v => v.SetVector("_Reverse", new Vector4(0, 1, 0, 0)));
        m_DissolveMaterials.ForEach(v => v
                .DOFloat(0, "_Dissolve", Duration)
                .OnComplete(() =>
                {
                    OnCompletedAnimationDissolve?.Invoke();
                    if(m_GarageObject != null)
                    {
                        m_GarageObject.SetActive(true);
                        if (m_DissolveObject != null)
                            m_DissolveObject.SetActive(false);
                    }
                }));

        if(m_ObjectMaterials != null && m_ObjectMaterials.Count > 0)
            m_ObjectMaterials.ForEach(v => v.DOFloat(0, "_Dissolve", Duration));
    }

    public void HideDissolve(float Duration, Action OnCompletedAnimationDissolve = null)
    {
        if (m_DissolveObject != null)
            m_DissolveObject.SetActive(true);
        if (m_GarageObject != null)
            m_GarageObject.SetActive(false);

        m_DissolveMaterials.ForEach(v => v.SetVector("_Reverse", new Vector4(1, 0, 0, 0)));
        m_DissolveMaterials.ForEach(v => v
            .DOFloat(1, "_Dissolve", Duration)
            .OnComplete(() =>
            {
                OnCompletedAnimationDissolve?.Invoke();
                m_Renderer.enabled = false;
            }));

        if (m_ObjectMaterials != null && m_ObjectMaterials.Count > 0)
            m_ObjectMaterials.ForEach(v => v.DOFloat(1, "_Dissolve", Duration));
    }
}
