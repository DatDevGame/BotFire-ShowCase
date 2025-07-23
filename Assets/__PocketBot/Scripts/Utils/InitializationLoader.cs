using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitializationLoader : MonoBehaviour
{
    private void Start()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }
}