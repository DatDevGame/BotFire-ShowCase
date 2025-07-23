using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CheatResetTimeFreeBox : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField TMP_InputField;
    [SerializeField]
    private TimeBasedRewardSO m_timeBasedRewardSO;

    private void Awake()
    {
        TMP_InputField.onValueChanged.AddListener(second =>
        {
            m_timeBasedRewardSO.CoolDownInterval = int.Parse(second);
            m_timeBasedRewardSO.GetReward();
        });
    }
}
