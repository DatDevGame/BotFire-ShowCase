using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class HandleGearInfoPopup : MonoBehaviour
{
    [SerializeField, BoxGroup("Ref")] private GearInfoPopup gearInfoPopup;
    [SerializeField, BoxGroup("Ref")] private Transform content;

    private void Start()
    {
        Instantiate(gearInfoPopup, content);
    }
}
