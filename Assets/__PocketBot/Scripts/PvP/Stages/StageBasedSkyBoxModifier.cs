using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageBasedSkyBoxModifier : MonoBehaviour
{
    [SerializeField] Material skyBoxMat;
    // Start is called before the first frame update
    void Start()
    {
        var skyBox = Instantiate(skyBoxMat);
        RenderSettings.skybox = skyBox;
    }
}
