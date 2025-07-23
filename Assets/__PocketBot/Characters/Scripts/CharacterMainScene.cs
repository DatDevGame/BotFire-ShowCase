using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class CharacterMainScene : MonoBehaviour
{
    public RenderTexture RenderTextureCharacter => m_RenderTextureCharacter;
    public CharacterSystem CurrentCharacter => m_CurrentCharacter;

    [SerializeField, BoxGroup("Ref")] private Camera m_CameraRenderer;
    [SerializeField, BoxGroup("Ref")] private Transform m_HolderModel;

    [SerializeField, BoxGroup("Data")] private CharacterManagerSO m_CharacterManagerSO;

    private CharacterSystem m_CurrentCharacter;
    private RenderTexture m_RenderTextureCharacter;

    private void Awake()
    {
        m_RenderTextureCharacter = new(1500, 1500, 0, GraphicsFormat.R8G8B8A8_UNorm);
        m_CameraRenderer.targetTexture = m_RenderTextureCharacter;
        SetCameraEnable(false);
    }

    public void SpawnCharacter(CharacterSO characterSO)
    {
        if (m_CurrentCharacter != null)
            Destroy(m_CurrentCharacter.gameObject);

        GameObject currentModel = characterSO.GetModelPrefabAsGameObject();
        if (currentModel != null)
        {
            m_CurrentCharacter = Instantiate(currentModel, m_HolderModel).GetComponent<CharacterSystem>();
            m_CurrentCharacter.Animator.applyRootMotion = false;
            m_CurrentCharacter.transform.localScale = Vector3.one * 0.8f;
            m_CurrentCharacter.transform.localPosition = new Vector3(0, 0.05f, 0);
            m_CurrentCharacter.transform.localEulerAngles = new Vector3(0, 30, 0);
            m_CurrentCharacter.gameObject.SetLayer(gameObject.layer, true);
            m_CurrentCharacter.CurrentState = CharacterState.ReadyFight;
        }
    }

    public void SetCameraEnable(bool enable)
    {
        m_CameraRenderer.enabled = enable;
    }
}
