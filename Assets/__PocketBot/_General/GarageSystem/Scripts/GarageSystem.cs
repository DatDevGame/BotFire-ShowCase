using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using HyrphusQ.Events;
using HyrphusQ.SerializedDataStructure;
using DissolveExample;
using DG.Tweening;
using System.Linq;
using HightLightDebug;
using LatteGames;
using Unity.VisualScripting;

[EventCode]
public enum GarageEvent
{ 
    Show,
    Hide,
    PreviewGarage
}
public class GarageSystem : MonoBehaviour
{
    [SerializeField, BoxGroup("Config")] private float m_DurationAnimationTransitonGarage;
    [SerializeField, BoxGroup("Config")] private float m_SnapDuration;

    [SerializeField, BoxGroup("Ref")] private CanvasGroupVisibility m_MainCanvasGroup;
    [SerializeField, BoxGroup("Ref")] private GarageCellUI m_GarageCellUIPrefab;
    [SerializeField, BoxGroup("Ref")] private ScrollRect m_ScrollRect;
    [SerializeField, BoxGroup("Ref")] private Transform m_ContentCell;
    [SerializeField, BoxGroup("Ref")] private Transform m_GarageHolder;
    [SerializeField, BoxGroup("Ref")] private MultiImageButton m_CloseButton;
    [SerializeField, BoxGroup("Data")] private GarageManagerSO m_GarageManagerSO;

    private Dictionary<GarageSO, GarageModelHandle> m_DissolveGarageModels;
    private List<GarageCellUI> m_GarageCellUIs;
    private GarageCellUI m_SelectingGarage;
    private GarageCellUI m_PreviousGarage;
    private bool m_IsCanTransitionGarage = true;
    private bool m_IsShow = false;
    private bool m_IsTheFirstTime = false;

    private void Awake()
    {
        GameEventHandler.AddActionEvent(GarageEvent.Show, OnShow);
        GameEventHandler.AddActionEvent(GarageEvent.PreviewGarage, PreviewGarageHandle);
        GameEventHandler.AddActionEvent(MainSceneEventCode.OnClickButtonShop, OnClickButtonShop);
        GameEventHandler.AddActionEvent(BuyType.Transformer_King, OnClickCloseButton);
        m_CloseButton.onClick.AddListener(OnClickCloseButton);
        CurrencyManager.Instance.GetCurrencySO(CurrencyType.Standard).onValueChanged += Stand_OnValueChanged;
        CurrencyManager.Instance.GetCurrencySO(CurrencyType.Premium).onValueChanged += Premium_OnValueChanged;
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(GarageEvent.Show, OnShow);
        GameEventHandler.RemoveActionEvent(GarageEvent.PreviewGarage, PreviewGarageHandle);
        GameEventHandler.RemoveActionEvent(MainSceneEventCode.OnClickButtonShop, OnClickButtonShop);
        GameEventHandler.RemoveActionEvent(BuyType.Transformer_King, OnClickCloseButton);
        m_CloseButton.onClick.RemoveListener(OnClickCloseButton);

        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.GetCurrencySO(CurrencyType.Standard).onValueChanged -= Stand_OnValueChanged;
            CurrencyManager.Instance.GetCurrencySO(CurrencyType.Premium).onValueChanged -= Premium_OnValueChanged;
        }


        m_GarageManagerSO.GarageSOs.ForEach(v => v.OnOwn -= OnLoadCell);
    }

    private void Start()
    {
        InitCell();
    }

    private void InitCell()
    {
        m_GarageCellUIs = new List<GarageCellUI>();
        m_DissolveGarageModels = new Dictionary<GarageSO, GarageModelHandle>();
        for (int i = 0; i < m_GarageManagerSO.GarageSOs.Count; i++)
        {
            //Instantiate Cell Garage UI
            InstantiateCellGarageUI(i);

            //Instantiate Garage
            InstantiateGaragePrefab(i);
        }

        //Set Default Garage
        if (!m_GarageManagerSO.GarageSOs.Any(v => v.IsSelected))
        {
            m_IsTheFirstTime = true;
            m_SelectingGarage = m_GarageCellUIs[0];
            m_SelectingGarage.GarageSO.Own();
            m_SelectingGarage.GarageSO.Select();
            OnLoadCell();

            PreviewGarageHandle(m_SelectingGarage, true);
        }

        m_GarageManagerSO.GarageSOs.ForEach(v => v.OnOwn += OnLoadCell);
    }

    private void InstantiateCellGarageUI(int i)
    {
        GarageCellUI garageCellUI = Instantiate(m_GarageCellUIPrefab, m_ContentCell);
        garageCellUI.Load(m_GarageManagerSO.GarageSOs[i]);
        m_GarageCellUIs.Add(garageCellUI);

        if (garageCellUI.GarageSO.IsSelected)
            m_SelectingGarage = garageCellUI;
    }

    private void InstantiateGaragePrefab(int i)
    {
        GarageModelHandle Garage = Instantiate(m_GarageManagerSO.GarageSOs[i].Room);
        MeshRenderer meshRendererGagage = Garage.Renderer;

        //List<Material> dissolveMaterials = new List<Material>(meshRendererGagage.materials.ToList());
        //Garage.SetMeshRenderer(dissolveMaterials);
        //if (m_GarageManagerSO.GarageSOs[i].IsSelected)
        //    Garage.ShowDissolve(0);
        //else
        //    Garage.HideDissolve(0);

        if (m_GarageManagerSO.GarageSOs[i].IsSelected)
            Garage.Show();
        else
            Garage.Hide();

        m_DissolveGarageModels.Add(m_GarageManagerSO.GarageSOs[i], Garage);
    }

    private void OnLoadCell()
    {
        m_GarageCellUIs
            .Where(v => v != null)
            .ToList()
            .ForEach(v => v.Load());
    }

    private void PreviewGarageHandle(params object[] parameters)
    {
        if (parameters.Length <= 0 || parameters[0] == null) return;
        GarageCellUI garageCellUI = parameters[0] as GarageCellUI;

        bool isIgnoreSelecting = false;
        if (parameters.Length > 1 && parameters[1] != null)
            isIgnoreSelecting = (bool)parameters[1];

        if (m_SelectingGarage == garageCellUI && !isIgnoreSelecting)
            return;

        if (!m_IsCanTransitionGarage && !isIgnoreSelecting) 
            return;
        m_IsCanTransitionGarage = false;

        if (garageCellUI != null)
        {
            m_PreviousGarage = m_SelectingGarage;
            m_SelectingGarage = garageCellUI;
            m_GarageCellUIs.ForEach(v => v.PreviewUnSelect());
            m_SelectingGarage.PreviewSelect();
            OnLoadGarage();

            if (garageCellUI.GarageSO.IsOwned)
            {
                m_GarageCellUIs
                    .Where(v => v.GarageSO.IsOwned).ToList()
                    .ForEach(v => v.GarageSO.UnSelect());

                m_PreviousGarage.GarageSO.UnSelect();
                m_SelectingGarage.GarageSO.Select();
                OnLoadCell();
            }
        }
    }

    private void OnClickButtonShop()
    {
        OnClickCloseButton();
    }

    private void OnLoadGarage(float durationDissolve = 1)
    {
        if (m_IsTheFirstTime)
        {
            durationDissolve = 0;
            m_IsTheFirstTime = false;
        }

        m_DissolveGarageModels
            .Where(v => v.Value != m_DissolveGarageModels[m_PreviousGarage.GarageSO] && m_DissolveGarageModels[m_SelectingGarage.GarageSO])
            .ToList()
            .ForEach(x => x.Value.gameObject.SetActive(false));

        m_DissolveGarageModels[m_PreviousGarage.GarageSO].Hide();
        m_DissolveGarageModels[m_SelectingGarage.GarageSO].Show();
        m_IsCanTransitionGarage = true;

        //m_DissolveGarageModels[m_PreviousGarage.GarageSO].HideDissolve(durationDissolve);
        //m_DissolveGarageModels[m_SelectingGarage.GarageSO].ShowDissolve(durationDissolve, () => { m_IsCanTransitionGarage = true; });
    }

    private void OnClickCloseButton()
    {
        if (!m_IsCanTransitionGarage) return;

        OnHide();
        m_GarageCellUIs.ForEach(v => v.PreviewUnSelect());
        GarageCellUI currentGarageSelected = m_GarageCellUIs.Find(v => v.GarageSO.IsSelected);
        if (currentGarageSelected == null)
            currentGarageSelected = m_GarageCellUIs[0];

        currentGarageSelected.PreviewSelect();
        PreviewGarageHandle(currentGarageSelected, false);
    }

    private void OnShow()
    {
        if (m_IsShow) return;

        SnapToIndex(m_GarageCellUIs.FindIndex(v => v.GarageSO.IsSelected));
        m_IsShow = true;
        m_MainCanvasGroup.Show();
        OnLoadCell();
    }

    private void OnHide()
    {
        if (!m_IsShow) return;

        m_IsShow = false;
        m_MainCanvasGroup.Hide();
        GameEventHandler.Invoke(GarageEvent.Hide);
    }

    public void SnapToIndex(int index)
    {
        if (m_GarageCellUIs.Count <= 0) return;

        // Clamp index to ensure it's within range
        index = Mathf.Clamp(index, 0, m_GarageCellUIs.Count - 1);

        // Calculate the target position
        float targetX = index * (m_GarageCellUIs[0].GetComponent<RectTransform>().sizeDelta.x + 50);

        // Adjust target to center the cell
        float viewportWidth = m_ScrollRect.viewport.rect.width;
        targetX = Mathf.Clamp(targetX, 0, m_ContentCell.GetComponent<RectTransform>().rect.width - viewportWidth);

        // Use DOTween to smoothly scroll to the target position
        m_ContentCell.GetComponent<RectTransform>().DOAnchorPosX(-targetX, 0.5f).SetEase(Ease.InOutQuad);
    }

    private void Stand_OnValueChanged(ValueDataChanged<float> obj) => m_GarageCellUIs.ForEach(v => v.Load());
    private void Premium_OnValueChanged(ValueDataChanged<float> obj) => m_GarageCellUIs.ForEach(v => v.Load());
}
