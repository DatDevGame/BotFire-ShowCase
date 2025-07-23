using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PBAnalyticsEvents;

public class ButtonClickAnalyticsEmitter : MonoBehaviour
{
    [SerializeField] string buttonName;
    public int clickTimes
    {
        get
        {
            return PlayerPrefs.GetInt($"ButtonClickAnalyticsEmitter_{buttonName}_ClickTimes", 0);
        }
        set
        {
            PlayerPrefs.SetInt($"ButtonClickAnalyticsEmitter_{buttonName}_ClickTimes", value);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        this.gameObject.GetComponent<Button>().onClick.AddListener(OnButtonClick);
    }
    private void OnDestroy()
    {
        this.gameObject.GetComponent<Button>().onClick.RemoveListener(OnButtonClick);
    }
    void OnButtonClick()
    {
        clickTimes++;
        PBAnalyticsManager.Instance.ClickButton(buttonName, clickTimes);
    }
}
