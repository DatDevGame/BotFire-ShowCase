using UnityEngine;
using UnityEngine.UI;

public class CheatMedalsButton : MonoBehaviour
{
    [SerializeField] Button button;
    [SerializeField] CurrencySO medalSO;

    private void Awake()
    {
        button.onClick.AddListener(HandleButtonClicked);
    }

    private void HandleButtonClicked()
    {
        medalSO.AcquireWithoutLogEvent(50);
    }
}
