using UnityEngine;
using UnityEngine.UI;

public class CheatAddEventTicket : MonoBehaviour
{
    [SerializeField] Button button;
    [SerializeField] CurrencySO eventTicketSO;

    private void Awake()
    {
        button.onClick.AddListener(HandleButtonClicked);
    }

    private void HandleButtonClicked()
    {
        eventTicketSO.AcquireWithoutLogEvent(10);
    }
}
