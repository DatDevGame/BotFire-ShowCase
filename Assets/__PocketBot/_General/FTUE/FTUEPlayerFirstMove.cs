using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FTUEPlayerFirstMove : Singleton<FTUEPlayerFirstMove>
{
    [SerializeField] RectTransform cursor;
    [SerializeField] GameObject handFTUE;
    public bool hasMove = false;

    void Update()
    {
        if(!hasMove && cursor.anchoredPosition.x != 0)
        {
            hasMove = true;
            handFTUE.SetActive(false);
        }
    }
}
