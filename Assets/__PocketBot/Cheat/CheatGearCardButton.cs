using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CheatGearCardButton : MonoBehaviour
{
    [SerializeField]
    private Button m_CheatButton;
    [SerializeField]
    private PBPartManagerSO m_BodyManagerSO;
    [SerializeField]
    private ItemSOVariable m_PreviewBodyVariable;

    private void Awake()
    {
        m_CheatButton.onClick.AddListener(OnAddGearCard);
    }

    private void OnAddGearCard()
    {
        if (m_PreviewBodyVariable.value == null)
            return;
        var slots = m_PreviewBodyVariable.value.Cast<PBChassisSO>().AllPartSlots;
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].PartVariableSO.value == null)
                continue;
            if (slots[i].PartVariableSO.value.IsUnlocked() == false) slots[i].PartVariableSO.value.TryUnlockIgnoreRequirement();
            slots[i].PartVariableSO.value.UpdateNumOfCards(slots[i].PartVariableSO.value.GetNumOfCards() + 50);
        }
        var body = m_BodyManagerSO.value.Find(item => item.GetDisplayName().Equals(m_PreviewBodyVariable.value.GetDisplayName()));
        if (body.IsUnlocked() == false) body.TryUnlockIgnoreRequirement();
        body.UpdateNumOfCards(m_PreviewBodyVariable.value.GetNumOfCards() + 50);
    }
}