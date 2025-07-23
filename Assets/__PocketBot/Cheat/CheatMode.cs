using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheatMode
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoInstantiate_Resources()
    {
        if (!GameDataSO.Instance.isDevMode)
            return;
        var cheatObj = GameObject.Instantiate(Resources.Load("Cheat"));
        cheatObj.name = "Cheat";
        GameObject.DontDestroyOnLoad(cheatObj);
    }
}