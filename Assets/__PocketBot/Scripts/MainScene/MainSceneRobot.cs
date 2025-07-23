using System;
using System.Collections;
using System.Collections.Generic;
using LatteGames;
using UnityEngine;

public class MainSceneRobot : MonoBehaviour
{
    [SerializeField]
    private float rotateDuration = 3f;
    [SerializeField]
    private float rotateAngle = 45f;
    [SerializeField]
    private AnimationCurve curve;

    private float originAngleY;

    private void Start()
    {
        originAngleY = transform.localEulerAngles.y;
        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, originAngleY - 45f, transform.localEulerAngles.z);
        StartCoroutine(Rotate_CR());
    }

    private IEnumerator Rotate_CR()
    {
        float sign = 1f;
        while (true)
        {
            var from = transform.localEulerAngles.y;
            var to = from + sign * rotateAngle * 2f;
            yield return CommonCoroutine.LerpAnimation(rotateDuration, curve, t =>
            {
                transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, Mathf.Lerp(from, to, t) ,transform.localEulerAngles.z);
            });
            sign = -sign;
        }
    }
}