using LatteGames.Monetization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PBUltimatePackButton : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private TMP_Text _remainTimeText;
    [SerializeField] private GameObject _remainTimeGO;
    [SerializeField] private TMP_Text _discountText;
    [SerializeField] private GameObject _discountGO;

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
        PBUltimatePackPopup.Instance.ConnectButton(this);
        _button.onClick.AddListener(OnButtonClicked);
    }
    
    private void OnDestroy()
    {
        _button.onClick.RemoveListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        PBUltimatePackPopup.Instance.TryShowIfCan();
    }

    public void UpdateView(IAPProductSO iapProductSO, bool isDiscount, bool isShowEndTIme)
    {
        _discountText.SetText($"-{iapProductSO.discountRate}%");
        _discountGO.SetActive(isDiscount);
        _remainTimeGO.SetActive(isShowEndTIme);
    }

    public void UpdateTime(string time)
    {
        _remainTimeText.SetText(time);
    }
}
