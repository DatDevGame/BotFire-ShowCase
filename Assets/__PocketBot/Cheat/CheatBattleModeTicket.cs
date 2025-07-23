using UnityEngine;
using UnityEngine.UI;

public class CheatBattleModeTicket : MonoBehaviour
{
    [SerializeField]
    private Button m_Button;
    [SerializeField]
    private RangePPrefIntProgressSO m_TicketCountProgressVariable;

    private RangeProgress<int> ticketProgress => m_TicketCountProgressVariable.rangeProgress;

    private void Awake()
    {
        m_Button.onClick.AddListener(() =>
        {
            ticketProgress.value = Mathf.Clamp(ticketProgress.value + 1, ticketProgress.minValue, ticketProgress.maxValue);
        });
    }
}