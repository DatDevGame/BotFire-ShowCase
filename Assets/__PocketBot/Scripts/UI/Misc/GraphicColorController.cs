using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class GraphicColorController : MonoBehaviour
{
    [SerializeField] Material grayscaleMat;
    [SerializeField] List<Graphic> excludeList = new();

    List<Graphic> graphics = null;
    List<Material> baseMats = new();
    Dictionary<Graphic, Color> baseTMPColors = new();

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        if (graphics != null)
            return;
        graphics = new(GetComponentsInChildren<Graphic>(true));
        foreach (var exclude in excludeList)
        {
            graphics.Remove(exclude);
        }
        foreach (var graphic in graphics)
        {
            baseMats.Add(graphic.material);
            if (graphic is TMP_Text)
            {
                baseTMPColors.Add(graphic, graphic.color);
            }
        }
    }

    public void SetGrayscale(bool shouldBeGrayscale)
    {
        for (int i = 0; i < graphics.Count; i++)
        {
            var graphic = graphics[i];
            if (graphic is TMP_Text)
            {
                var grayScaleValue = baseTMPColors[graphic].r * 0.3 + baseTMPColors[graphic].g * 0.59 + baseTMPColors[graphic].b * 0.11;
                ((TMP_Text)graphic).color = shouldBeGrayscale ? new Color((float)grayScaleValue, (float)grayScaleValue, (float)grayScaleValue) : baseTMPColors[graphic];
            }
            else
            {
                graphics[i].material = shouldBeGrayscale ? grayscaleMat : baseMats[i];
            }
        }
    }

    public void SetDarkCover(bool shouldBeDarkCover, float darkValue = 1)
    {
        for (int i = 0; i < graphics.Count; i++)
        {
            var graphic = graphics[i];
            graphic.CrossFadeColor(shouldBeDarkCover ? new Color(darkValue, darkValue, darkValue, 1) : Color.white, 0, true, true);
        }
    }
}
