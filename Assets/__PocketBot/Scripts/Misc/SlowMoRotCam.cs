using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class SlowMoRotCam : MonoBehaviour
{
    [SerializeField] float duration = 1;
    [SerializeField] float timeScale = 0.1f;

    CinemachineVirtualCamera virtualCamera;
    CinemachineVirtualCamera clonedVirtualCamera;
    CinemachineBrain cinemachineBrain;
    Transform targetPivot;
    bool isRotating;

    private void Start()
    {
        cinemachineBrain = ObjectFindCache<CinemachineBrain>.Get();
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
        targetPivot = new GameObject("targetPivot").transform;
        clonedVirtualCamera = new GameObject("clonedVirtualCamera").AddComponent<CinemachineVirtualCamera>();
        clonedVirtualCamera.transform.SetParent(targetPivot);
        clonedVirtualCamera.Priority = 1000;
        clonedVirtualCamera.gameObject.SetActive(false);
        targetPivot.SetParent(transform.parent);
    }

    private void Update()
    {
        if (!isRotating && Input.GetKeyDown(KeyCode.Z))
        {
            StartCoroutine(CR_RotateCam());
        }
    }

    IEnumerator CR_RotateCam()
    {
        targetPivot.position = virtualCamera.m_Follow.position;
        targetPivot.localRotation = Quaternion.Euler(0f, 0f, 0f);
        yield return null;
        clonedVirtualCamera.gameObject.SetActive(true);
        clonedVirtualCamera.transform.position = transform.position;
        clonedVirtualCamera.transform.rotation = transform.rotation;
        Time.timeScale = timeScale;
        cinemachineBrain.enabled = false;
        isRotating = true;
        var t = 0f;
        while (t < 1)
        {
            t += Time.unscaledDeltaTime / duration;
            targetPivot.position = virtualCamera.m_Follow.position;
            targetPivot.localRotation = Quaternion.Euler(0f, Mathf.Lerp(0, 360f, t), 0f);
            cinemachineBrain.transform.position = clonedVirtualCamera.transform.position;
            cinemachineBrain.transform.rotation = clonedVirtualCamera.transform.rotation;
            yield return null;
        }
        Time.timeScale = 1;
        isRotating = false;
        cinemachineBrain.enabled = true;
        clonedVirtualCamera.gameObject.SetActive(false);
    }
}
