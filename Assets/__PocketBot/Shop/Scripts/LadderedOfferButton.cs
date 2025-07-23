using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LadderedOfferButton : MonoBehaviour
{

    [SerializeField] private Button _button;
    [SerializeField] private TMP_Text _remainTimeText;

    private void Awake()
    {
        //TODO: Hide IAP & Popup
        CanvasGroup canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    private void Start()
    {
        LadderedOfferPopup.Instance.ConnectButton(this);
        _button.onClick.AddListener(OnButtonClicked);
    }
    
    private void OnDestroy()
    {
        _button.onClick.RemoveListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        LadderedOfferPopup.Instance.Show();
    }

    public void UpdateTime(string time)
    {
        _remainTimeText.SetText(time);
    }
    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
