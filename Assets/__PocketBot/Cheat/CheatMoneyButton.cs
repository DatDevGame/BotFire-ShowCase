using UnityEngine;
using UnityEngine.UI;

public class CheatMoneyButton : MonoBehaviour
{
    [SerializeField] Button button;
    [SerializeField] CurrencySO moneySO;

    private void Awake()
    {
        button.onClick.AddListener(HandleButtonClicked);
    }

    private void HandleButtonClicked()
    {
        moneySO.AcquireWithoutLogEvent(1000000);
    }
}
