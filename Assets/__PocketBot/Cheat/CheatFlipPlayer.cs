using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class CheatFlipPlayer : MonoBehaviour
{
    [SerializeField]
    private Button m_Button;

    private bool flip = true;
    private void Awake()
    {
        m_Button.onClick.AddListener(Flip);
    }

    public void Flip()
    {
        PBLevelController pBLevelController = ObjectFindCache<PBLevelController>.Get();

        if (pBLevelController != null)
        {
            var objectFlip = pBLevelController.Competitors.Find(v => v.PersonalInfo.isLocal);
            CarPhysics carPhysics = objectFlip.GetComponentInChildren<CarPhysics>();
            if (carPhysics != null)
            {
                flip = !flip;
                float eulr = !flip ? 180 : 0;

                carPhysics.transform.parent.position = carPhysics.transform.position;
                carPhysics.transform.localPosition = Vector3.zero;
                carPhysics.transform.parent.eulerAngles = new Vector3(carPhysics.transform.parent.eulerAngles.x, carPhysics.transform.parent.eulerAngles.y, eulr);
            }
        }
    }
}
