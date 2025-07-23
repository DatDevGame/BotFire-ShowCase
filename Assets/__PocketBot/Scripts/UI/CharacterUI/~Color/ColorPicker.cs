using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorPicker : MonoBehaviour
{
    public PBPartSO partSO;
    [SerializeField] ColorButton colorButtonPrefab;
    [SerializeField] List<ColorButton> colorButtons;

    public void GenerateColor(SkinSO skinSO)
    {
        ColorButton colorButton = Instantiate(colorButtonPrefab, this.transform);
        colorButton.Skin = skinSO;
        colorButton.PartSO = partSO;
        colorButton.colorPicker = this;
        colorButtons.Add(colorButton);
    }

    public void ClearList()
    {
        for (int i = 0; i < colorButtons.Count; i++)
        {
            Destroy(colorButtons[i].gameObject);
        }
        colorButtons.Clear();
    }
    public void DisableOutlineAll()
    {
        foreach (var item in colorButtons)
        {
            item.DisableOutline();
        }
    }
}
