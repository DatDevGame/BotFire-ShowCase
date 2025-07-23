using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using HyrphusQ.Events;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class SkinListView : ItemListView
{
    public List<ItemCell> GetItemCells => m_ItemCells;

    [SerializeField]
    private ScrollRect m_ScrollRect;
    [SerializeField]
    protected GameObject m_EmptyItemCellPrefab;
    [SerializeField]
    protected int minCellAmount = 4;

    private List<GameObject> emptyCells = new List<GameObject>();
    private PBPartSO m_CurrentPartSO;

    public PBPartSO currentPartSO
    {
        get
        {
            return m_CurrentPartSO;
        }
        set
        {
            m_CurrentPartSO = value;
            ClearAll();
            if (value != null)
                GenerateView(m_SelectDefaultItemAtStart);
        }
    }
    private SkinSO currentSkin
    {
        get
        {
            if (currentPartSO != null && currentPartSO.TryGetModule(out SkinItemModule skinModule))
            {
                return skinModule.currentSkin;
            }
            return null;
        }
        set
        {
            if (currentPartSO != null && currentPartSO.TryGetModule(out SkinItemModule skinModule))
            {
                skinModule.currentSkin = value;
            }
        }
    }
    private List<SkinSO> skins
    {
        get
        {
            if (currentPartSO != null && currentPartSO.TryGetModule(out SkinItemModule skinModule))
            {
                return skinModule.skins;
            }
            return default;
        }
    }

    private int currentSkinIndex => skins.IndexOf(currentSkin);
    private float _offsetScrollIndex = 70;

    private void Awake()
    {
        ObjectFindCache<SkinListView>.Add(this);
    }

    protected override void OnDestroy()
    {
        ObjectFindCache<SkinListView>.Remove(this);
    }

    protected override void SelectDefaultItem()
    {
        // Validate input data
        var currentSkin = this.currentSkin;
        if (m_ItemCells == null || m_CurrentPartSO == null || currentSkin == null)
            return;
        if (m_CurrentSelectedCell != null && m_CurrentSelectedCell.item == currentSkin)
            return;
        foreach (var itemCell in m_ItemCells)
        {
            if (itemCell.item == currentSkin)
            {
                OnItemSelected(itemCell, true);
                break;
            }
        }
    }

    protected override void NotifyEventItemPreviewed(ItemCell itemCell)
    {
        base.NotifyEventItemPreviewed(itemCell);
        GameEventHandler.Invoke(PartManagementEventCode.OnSkinPreviewed, currentPartSO.ManagerSO, currentPartSO, itemCell.item);
    }

    protected override void SelectItem(ItemCell itemCell, bool isForceSelect = false)
    {
        m_CurrentSelectedCell?.Deselect();
        m_CurrentSelectedCell = itemCell;
        m_CurrentSelectedCell.Select(isForceSelect);
        m_CurrentSelectedCell.UpdateView();

        // Notify event
        NotifyEventItemSelected(itemCell);
    }

    public override void UseItem(ItemCell itemCell)
    {
        currentSkin = itemCell.item.Cast<SkinSO>();

        // Notify event
        NotifyEventItemUsed(itemCell);
    }

    public override void GenerateView(bool selectDefaultItem = false)
    {
        if (m_CurrentPartSO == null || skins == null)
            return;
        if (m_ItemCells != null)
        {
            if (selectDefaultItem)
                SelectDefaultItem();
            return;
        }
        m_ItemCells = new List<ItemCell>();
        foreach (var item in skins)
        {
            // Generate item cell UI
            var itemCellInstance = Instantiate(m_ItemCellPrefab, m_ItemCellContainer);
            itemCellInstance.transform.localScale = Vector3.one;
            itemCellInstance.onItemClicked += OnItemSelected;
            itemCellInstance.Initialize(item, null);
            m_ItemCells.Add(itemCellInstance);
        }

        if (minCellAmount - skins.Count > 0)
        {
            for (int i = 0; i < minCellAmount - skins.Count; i++)
            {
                // Generate item cell UI
                var itemCellInstance = Instantiate(m_EmptyItemCellPrefab, m_ItemCellContainer);
                itemCellInstance.transform.localScale = Vector3.one;
                emptyCells.Add(itemCellInstance);
            }
        }

        var contentSizeFitter = m_ItemCellContainer?.GetComponent<ContentSizeFitter>();
        contentSizeFitter?.SetLayoutHorizontal();
        contentSizeFitter?.SetLayoutVertical();

        // Select current selected item
        if (selectDefaultItem)
            SelectDefaultItem();

        // Notify event dataset changed
        NotifyDatasetChanged();
        AutoScrollToIndex();

        #region Design Events
        for (int i = 1; i < m_ItemCells.Count; i++)
        {
            if (!m_ItemCells[i].item.IsUnlocked())
            {
                string partName = $"{m_CurrentPartSO.GetDisplayName()}";
                string rvName = $"UnlockSkin_{partName}";
                string location = "InfoPopup";
                GameEventHandler.Invoke(DesignEvent.RVShow);
                break;
            }
        }
        #endregion

    }

    private void AutoScrollToIndex()
    {
        var anchoredPositionX = -currentSkinIndex * _offsetScrollIndex;
        m_ItemCellContainer.DOAnchorPosX(anchoredPositionX, 0.05f).OnComplete(() =>
        {
            m_ScrollRect.velocity = Vector3.zero;
        });
    }

    public override void ClearAll()
    {
        base.ClearAll();
        foreach (var item in emptyCells)
        {
            Destroy(item);
        }
        emptyCells.Clear();
        // m_ScrollRect.horizontalScrollbar.value = 0;
    }

    public virtual void ForceSelectCell(SkinCell cell)
    {
        m_CurrentSelectedCell = null;
        OnItemSelected(cell, true);
    }

    [BoxGroup("Editor"), Button]
    private void SetPartSO(PBPartSO partSO)
    {
        currentPartSO = partSO;
    }
}