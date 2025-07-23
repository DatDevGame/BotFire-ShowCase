using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LadderedOfferRewardItemCell : MonoBehaviour
{
    [SerializeField] private Image _thumbnail;
    [SerializeField] private Image _rarityOutline;
    [SerializeField] private TMP_Text _amountTxt;

    public Image thumpnail => _thumbnail;
    public Image rarityOutline => _rarityOutline;
    public TMP_Text amountTxt => _amountTxt;
}