using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    [SerializeField]
    private bool isFlip;

    private Camera mainCam;

    void Start()
    {
        mainCam = MainCameraFindCache.Get();
    }

    void Update()
    {
        transform.forward = isFlip ? -(mainCam.transform.position - transform.position).normalized : (mainCam.transform.position - transform.position).normalized;
    }
}