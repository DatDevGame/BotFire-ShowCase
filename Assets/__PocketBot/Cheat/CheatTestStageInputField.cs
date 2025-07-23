using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CheatTestStageInputField : MonoBehaviour
{
    public static string stageName;
    private void Start()
    {
        var input = GetComponent<TMP_InputField>();
        input.onValueChanged.AddListener(name =>
        {
            stageName = name;
        });
    }
}
