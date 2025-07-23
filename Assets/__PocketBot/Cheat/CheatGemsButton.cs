using UnityEngine;
using UnityEngine.UI;

public class CheatGemsButton : MonoBehaviour
{
    [SerializeField] Button button;
    [SerializeField] CurrencySO gemSO;

    private void Awake()
    {
        button.onClick.AddListener(HandleButtonClicked);
    }

    private void HandleButtonClicked()
    {
        gemSO.AcquireWithoutLogEvent(10000);
    }
}
