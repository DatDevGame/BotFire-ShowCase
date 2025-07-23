using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
[ExecuteAlways]
#endif
public class ChainUpdater : MonoBehaviour
{
    [SerializeField] List<ChainController> chainControllers;

    private void Update()
    {
        foreach (var chainController in chainControllers)
        {
            chainController.UpdateChain();
        }
    }
}
