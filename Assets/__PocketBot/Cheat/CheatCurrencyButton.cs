using UnityEngine;
using UnityEngine.UI;

public class CheatCurrencyButton : MonoBehaviour
{
    [SerializeField] Button button;
    [SerializeField] float amount;
    [SerializeField] CurrencySO currencySO;

    private void Awake()
    {
        button.onClick.AddListener(HandleButtonClicked);
    }

    private void HandleButtonClicked()
    {
        currencySO.AcquireWithoutLogEvent(amount);
    }
}
