using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ActiveSkillCardUI : ItemCell
{
    public const int k_MaxCardQuantityDisplay = 9999;

    [SerializeField]
    protected Color m_NormalCardQuantityTextColor = Color.white, m_ZeroCardQuantityTextColor = Color.red;
    [SerializeField]
    protected GameObject m_EquippedOutlineGO;
    [SerializeField]
    protected TextMeshProUGUI m_SkillNameText;
    [SerializeField]
    protected TextMeshProUGUI m_CardQuantityText;

    public Button button => m_Button;
    public ActiveSkillSO activeSkillSO => item.Cast<ActiveSkillSO>();

    public override void Initialize(ItemSO item, ItemManagerSO itemManagerSO)
    {
        if (item == null)
            return;
        name = item.GetDisplayName();
        m_ItemSO = item;
        m_ItemManagerSO = itemManagerSO;
        m_Button.onClick.RemoveListener(OnItemClicked);
        m_Button.onClick.AddListener(OnItemClicked);
        m_ThumbnailImage = m_ItemSO.CreateIconImage(m_ThumbnailImage);
        m_SkillNameText.SetText(item.GetDisplayName());
        UpdateView();
    }

    public override void UpdateView()
    {
        if (item == null)
            return;
        int cardQuantity = Mathf.Min(item.GetNumOfCards(), k_MaxCardQuantityDisplay);
        m_CardQuantityText.color = cardQuantity > 0 ? m_NormalCardQuantityTextColor : m_ZeroCardQuantityTextColor;
        m_CardQuantityText.SetText(cardQuantity.ToString());
        if (m_EquippedOutlineGO != null)
            m_EquippedOutlineGO.SetActive(itemManagerSO.currentItemInUse == item);
    }
}