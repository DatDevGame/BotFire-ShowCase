using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LatteGames.GameManagement;

public class CheatSkipAllFTUEButton : MonoBehaviour
{
    [SerializeField] Button m_CheatButton;
    [SerializeField] List<BoolVariable> m_PprefFTUEList;

    private void Awake()
    {
        m_CheatButton.onClick.AddListener(OnCheatButtonClicked);
    }

    void OnCheatButtonClicked()
    {
        foreach (var ppref in m_PprefFTUEList)
        {
            ppref.value = !ppref.initialValue;
        }

        SceneManager.LoadScene(SceneName.InitializationScene);
    }
}
