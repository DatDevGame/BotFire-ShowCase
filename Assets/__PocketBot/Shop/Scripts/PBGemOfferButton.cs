using System.Collections;
using System.Collections.Generic;
using LatteGames;
using LatteGames.Monetization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PBGemOfferButton : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private TMP_Text _remainTimeText;
    [SerializeField] private TMP_Text _discountText;

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
        PBGemOfferPopup.Instance.ConnectButton(this);
        _button.onClick.AddListener(OnButtonClicked);
    }
    
    private void OnDestroy()
    {
        _button.onClick.RemoveListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        PBGemOfferPopup.Instance.Show("Manually");
    }

    public void UpdateView(IAPProductSO iapProductSO)
    {
        _discountText.SetText($"-{iapProductSO.discountRate}%");
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
