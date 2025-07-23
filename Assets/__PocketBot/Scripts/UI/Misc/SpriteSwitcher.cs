using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpriteSwitcher : MonoBehaviour
{
    [SerializeField] Image imageTarget;
    [SerializeField] List<Sprite> sprites;
    public Image ImageTarget { get => imageTarget;}

    public void ChangeSprite(int spriteIndex)
    {
        if (sprites == null || sprites.Count == 0) return;
        imageTarget.sprite = sprites[Mathf.Clamp(spriteIndex, 0, sprites.Count - 1)];
    }
}
