using HyrphusQ.Events;
using HyrphusQ.Helpers;
using LatteGames.Monetization;
using Sirenix.OdinInspector;
using UnityEngine;

public class PBSkipAdOfferGroup : MonoBehaviour
{
    [SerializeField] private LG_IAPButton _discountOffer;
    [SerializeField] private float _minTrophyShow;
    [SerializeField] private FloatVariable _highestAchivedMedalVariable;
    [SerializeField] private TimeBasedRewardSO _discoundOfferCD;
    [SerializeField, BoxGroup("Optional")] private LG_IAPButton _normalOffer;

    private void Awake()
    {
        //TODO: Hide IAP & Popup
        CanvasGroup canvasGroup = gameObject.transform.GetChild(0).GetOrAddComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    private void Start()
    {
        GameEventHandler.AddActionEvent(IAPEventCode.OnPurchaseItemCompleted, OnPurchaseCompleted);
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(IAPEventCode.OnPurchaseItemCompleted, OnPurchaseCompleted);
    }

    private void OnPurchaseCompleted(params object[] objects)
    {
        if (objects[0] is LG_IAPButton lG_IAPButton && lG_IAPButton.IAPProductSO == _discountOffer.IAPProductSO)
        {
            _discoundOfferCD.GetReward();
        }
    }

    private void Update()
    {
        bool isHaveDiscount = _discoundOfferCD.canGetReward && _minTrophyShow <= _highestAchivedMedalVariable.value;
        _discountOffer.gameObject.SetActive(isHaveDiscount);
        if (_normalOffer)
        {
            _normalOffer.gameObject.SetActive(!isHaveDiscount);
        }
    }
}
