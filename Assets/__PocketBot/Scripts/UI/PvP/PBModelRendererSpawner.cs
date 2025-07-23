using DG.Tweening;
using LatteGames;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class PBModelRendererSpawner : MonoBehaviour
{
    public CharacterSystem PlayerCharacter => m_PlayerCharacter;
    public CharacterSystem BossCharacter => m_BossCharacter;
    public List<CharacterSystem> OpponentsCharacter => m_OpponentsCharacter;

    [SerializeField] PBModelRenderer opponentModelPrefab;
    [SerializeField] PBModelRenderer playerModelPrefab;

    List<GameObject> modelInstances = new();

    int spawnIndex;
    int m_SpawnOpponentIndex = 0;
    [SerializeField, BoxGroup("Data")] private CharacterManagerSO m_CharacterManagerSO;
    [SerializeField, BoxGroup("Data")] private Variable<Mode> m_PlayMode;

    private CharacterSystem m_PlayerCharacter;
    private CharacterSystem m_BossCharacter;
    private List<PBModelRenderer> m_PBModelRenderers;
    private List<CharacterSystem> m_OpponentsCharacter;

    private void Awake()
    {
        ObjectFindCache<PBModelRendererSpawner>.Add(this);
        m_PBModelRenderers = new List<PBModelRenderer>();
        m_OpponentsCharacter = new List<CharacterSystem>();
    }

    private void OnDestroy()
    {
        ObjectFindCache<PBModelRendererSpawner>.Remove(this);
    }

    public void SetCameraRobotPreview()
    {
        if(m_PBModelRenderers.Count > 0)
            m_PBModelRenderers.ForEach(v => v.renderCamera.transform.DOLocalMove(new Vector3(0.7f, 2.4f, 5.45f), 0));

        m_PlayerCharacter.gameObject.SetActive(false);
        m_OpponentsCharacter.ForEach(v => v.gameObject.SetActive(false));
    }

    public RenderTexture SpawnPlayerModelRenderer()
    {
        RenderTexture texture = GetTextureFollowingPlayMode();
        var instance = SpawnModelRenderer(playerModelPrefab);
        m_PBModelRenderers.Add(instance);
        instance.renderCamera.targetTexture = texture;
        if(m_PlayMode == Mode.Battle)
            instance.renderCamera.transform.DOLocalMove(new Vector3(0.7f, 2.4f, 5.45f), 0);
        //Spawn Player Character
        if (m_CharacterManagerSO != null)
        {
            if (m_CharacterManagerSO.PlayerCharacterSO.value.TryGetModule<GameObjectModelPrefabItemModule>(out var modelPrefabItemModule))
            {
                CharacterSystem characterSystem = modelPrefabItemModule.modelPrefabAsGameObject.GetComponent<CharacterSystem>();
                if (characterSystem != null)
                {
                    var instanceCharacter = Instantiate(characterSystem, instance.transform);
                    instanceCharacter.transform.localPosition = new Vector3(-1.6f, -6f, -12.5f);
                    instanceCharacter.transform.localScale = GetCharacterSizeFollowingPlayMode();
                    m_PlayerCharacter = instanceCharacter;
                    //Disable Player Temp
                    if (m_PlayMode == Mode.Battle)
                        m_PlayerCharacter.gameObject.SetActive(false);

                    SetLayerRecursively(instanceCharacter.gameObject, 30);
                    instanceCharacter.CurrentState = CharacterState.ReadyFight;
                }
            }
        }

        return texture;
    }
    public RenderTexture SpawnOpponentModelRenderer(PlayerInfoVariable opponentInfoVariable)
    {
        RenderTexture texture = GetTextureFollowingPlayMode();
        var instance = SpawnModelRenderer(opponentModelPrefab);
        if (m_PlayMode.value == Mode.Battle)
            instance.renderCamera.transform.DOLocalMove(new Vector3(0.7f,2.4f, 5.45f), 0);
        m_PBModelRenderers.Add(instance);
        instance.transform.position += -10 * spawnIndex++ * Vector3.right;
        if (opponentInfoVariable != null) instance.SetInfo(opponentInfoVariable);
        instance.BuildRobot(false); //True or false is not important just a placeholder
        instance.renderCamera.targetTexture = texture;
        instance.ChassisInstance.CarPhysics.transform.localPosition = new Vector3(-1, -4.5f, -9.5f);
        //Spawn Opponent Character

        CharacterSO characterOpponent = m_CharacterManagerSO.initialValue.Where(v => !v.Cast<CharacterSO>().IsFakeLock).ToList().GetRandom().Cast<CharacterSO>();
        if (m_PlayMode.value == Mode.Boss)
            characterOpponent = m_CharacterManagerSO.BossCharacterSO;
        m_CharacterManagerSO.OpponentCharacterSOs[m_SpawnOpponentIndex].value = characterOpponent;
        m_SpawnOpponentIndex++;
        if (characterOpponent != null)
        {
            if (m_CharacterManagerSO != null)
            {
                if (m_CharacterManagerSO.CurrentMode.value == Mode.Normal)
                    HandleSpawnOpponentCharacter(characterOpponent);
                else if (m_CharacterManagerSO.CurrentMode.value == Mode.Boss)
                    HandleSpawnOpponentCharacter(characterOpponent);
                else
                    HandleSpawnOpponentCharacter(characterOpponent);

                void HandleSpawnOpponentCharacter(CharacterSO characterSO)
                {
                    if (characterSO.TryGetModule<GameObjectModelPrefabItemModule>(out var modelPrefabItemModule))
                    {
                        CharacterSystem characterSystem = modelPrefabItemModule.modelPrefabAsGameObject.GetComponent<CharacterSystem>();
                        if (characterSystem != null)
                        {
                            var instanceCharacter = Instantiate(characterSystem, instance.transform);
                            instanceCharacter.transform.localPosition = new Vector3(-1.6f, -6f, -12.5f);
                            instanceCharacter.transform.localScale = GetCharacterSizeFollowingPlayMode();
                            m_OpponentsCharacter.Add(instanceCharacter);
                            //Disable Opponent Temp
                            if (m_PlayMode.value == Mode.Battle)
                                instanceCharacter.gameObject.SetActive(false);
                            if (m_PlayMode.value == Mode.Boss)
                                instanceCharacter.transform.eulerAngles = new Vector3(-0, 55, 0);

                            SetLayerRecursively(instanceCharacter.gameObject, 30);
                            instanceCharacter.CurrentState = CharacterState.ReadyFight;
                        }
                    }
                }
            }
        }

        return texture;
    }
    private RenderTexture GetTextureFollowingPlayMode()
    {
        var graphicsFomat = UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm;
        if (m_PlayMode == null) return RenderTexture.GetTemporary(1024, 1024, 0, graphicsFomat);
        return m_PlayMode.value switch
        {
            Mode.Normal => RenderTexture.GetTemporary(1024, 1024, 0, graphicsFomat),
            Mode.Boss => RenderTexture.GetTemporary(1024, 1024, 0, graphicsFomat),
            Mode.Battle => RenderTexture.GetTemporary(256, 256, 0, graphicsFomat),
            _ => RenderTexture.GetTemporary(1024, 1024, 0, graphicsFomat)
        };
    }
    private Vector3 GetCharacterSizeFollowingPlayMode()
    {
        if (m_PlayMode == null) return Vector3.one;
        return m_PlayMode.value switch
        {
            Mode.Normal => Vector3.one * 4,
            Mode.Boss => Vector3.one * 4,
            Mode.Battle => Vector3.one * 4,
            _ => Vector3.one * 1
        };
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    PBModelRenderer SpawnModelRenderer(PBModelRenderer prefab)
    {
        var instance = Instantiate(prefab, transform);
        modelInstances.Add(instance.gameObject);
        return instance;
    }

    public void DestroyAllInstances(float delayTime = AnimationDuration.LONG)
    {
        foreach (var item in m_PBModelRenderers)
        {
            item.renderCamera.targetTexture = null;
        }
        foreach (var model in modelInstances)
        {
            if (model != null) Destroy(model, delayTime);
        }
        modelInstances.Clear();
        m_PBModelRenderers.Clear();
    }
}
