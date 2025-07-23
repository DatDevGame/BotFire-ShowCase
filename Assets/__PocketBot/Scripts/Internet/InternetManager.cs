using System.Collections;
using System.Collections.Generic;
using LatteGames;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InternetManager : MonoBehaviour
{
    [SerializeField] Button tryAgainBtn;
    [SerializeField] CanvasGroupVisibility popupVisibility;
    [SerializeField] CanvasGroup popupCanvasGroup;
    [SerializeField] SceneName mainSceneName;
    [SerializeField] float showPopupTimeThreshold = 5;
    Coroutine checkInternetCoroutine;

    private void Start()
    {
        tryAgainBtn.onClick.AddListener(OnTryAgainClicked);
        checkInternetCoroutine = StartCoroutine(CR_CheckInternet());
    }

    IEnumerator CR_CheckInternet()
    {
        yield return new WaitUntil(() => LoadingScreenUI.IS_LOADING_COMPLETE);
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            popupVisibility.Show();
        }
        while (true)
        {
            yield return new WaitUntil(() => Application.internetReachability == NetworkReachability.NotReachable);
            yield return new WaitForSeconds(showPopupTimeThreshold);
            if (SceneManager.GetActiveScene().name == mainSceneName.ToString() && Application.internetReachability == NetworkReachability.NotReachable)
            {
                popupVisibility.Show();
            }
            yield return new WaitUntil(() => popupCanvasGroup.alpha == 0);
        }
    }

    void OnTryAgainClicked()
    {
        if (Application.internetReachability != NetworkReachability.NotReachable)
        {
            popupVisibility.Hide();
        }
    }
}
