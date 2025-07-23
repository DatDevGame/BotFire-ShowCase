using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisabledGameObjectAfterAPeriodOfTime : MonoBehaviour
{
    [SerializeField] float disableTime;

    float elapsedTime = 0;

    private void OnEnable()
    {
        elapsedTime = 0;
    }

    private void OnDisable()
    {
        elapsedTime = 0;
    }

    // Update is called once per frame
    void Update()
    {
        elapsedTime += Time.deltaTime;
        if (elapsedTime >= disableTime) gameObject.SetActive(false);
    }
}
