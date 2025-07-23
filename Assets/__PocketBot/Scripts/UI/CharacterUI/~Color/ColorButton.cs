using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorButton : MonoBehaviour
{
    private SkinSO skin;

    public SkinSO Skin
    {
        get => skin; set
        {
            skin = value;
            imageColor.sprite = skin.icon;
            imageColor.material = null;
            imageColor.color = Color.white;
        }
    }


    [SerializeField] Image imageColor;
    [SerializeField] Button button;
    public GameObject selectedOutline;
    public ColorPicker colorPicker;
    private PBPartSO partSO;

    public PBPartSO PartSO
    {
        get => partSO; set
        {
            partSO = value;
            CheckSelected(partSO);
        }
    }


    public void PickColor()
    {
        if (PartSO != null)
        {
            SkinItemModule skinModule = PartSO.GetModule<SkinItemModule>();
            skinModule.currentSkin = Skin;
            colorPicker.DisableOutlineAll();
            CheckSelected(partSO);
        }
    }

    void CheckSelected(PBPartSO PartSO)
    {
        SkinItemModule skinModule = PartSO.GetModule<SkinItemModule>();

        if (Skin == skinModule.currentSkin)
        {
            selectedOutline.SetActive(true);
        }
        else
            selectedOutline.SetActive(false);
    }

    public void DisableOutline()
    {
        selectedOutline.SetActive(false);
    }
}
