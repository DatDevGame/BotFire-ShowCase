using System.Collections;
using System.Collections.Generic;
using LatteGames;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreenUI : Singleton<LoadingScreenUI>
{
    public static bool IS_LOADING_COMPLETE = false;

    [SerializeField]
    private Slider m_LoadingSlider;

    private IUIVisibilityController m_VisibilityController;
    private IUIVisibilityController visibilityController
    {
        get
        {
            if (m_VisibilityController == null)
            {
                m_VisibilityController = GetComponentInChildren<IUIVisibilityController>();
            }
            return m_VisibilityController;
        }
    }

    private static IEnumerator Load_CR(IAsyncTask loadingAsyncTask, float minDuration)
    {
        ShowImmediately();
        float timeSinceStart = Time.time;

        float targetValue = 0f;
        float currentValue = 0f;
        float smoothSpeed = 5f;

        while (!loadingAsyncTask.isCompleted)
        {
            float normalizedProgress = Mathf.InverseLerp(0.5f, 1f, loadingAsyncTask.percentageComplete);
            normalizedProgress = Mathf.Clamp01(normalizedProgress);
            targetValue = normalizedProgress * 0.8f;

            currentValue = Mathf.Lerp(currentValue, targetValue, Time.deltaTime * smoothSpeed);
            Instance.m_LoadingSlider.value = currentValue;

            yield return null;
        }

        float t = 0f;
        currentValue = Instance.m_LoadingSlider.value;
        float startValue = currentValue;
        float endValue = 1.0f;

        while (t < minDuration)
        {
            t += Time.deltaTime;
            float progress = Mathf.Clamp01(t / minDuration);
            Instance.m_LoadingSlider.value = Mathf.Lerp(startValue, endValue, progress);
            yield return null;
        }
        yield return new WaitForSeconds(0.1f);

        HideImmediately();
        IS_LOADING_COMPLETE = true;
    }




    public static void Load(IAsyncTask loadingAsyncTask, float minDuration = AnimationDuration.LONG)
    {
        Instance.StartCoroutine(Load_CR(loadingAsyncTask, minDuration));
    }

    public static void Show()
    {
        Instance.visibilityController.Show();
    }

    public static void ShowImmediately()
    {
        Instance.visibilityController.ShowImmediately();
    }

    public static void Hide()
    {
        Instance.visibilityController.Hide();
    }

    public static void HideImmediately()
    {
        Instance.visibilityController.HideImmediately();
    }
}