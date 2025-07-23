using System.Collections;
using System.Collections.Generic;
using GachaSystem.Core;
using UnityEngine;
using UnityEngine.UI;

public class CheatAddBox : MonoBehaviour
{
    [SerializeField] PBGachaPackManagerSO gachaPackManagerSO;
    [SerializeField] GachaPackRarity gachaPackRarity;
    private void Awake()
    {
        var button = GetComponent<Button>();
        button.onClick.AddListener(() =>
        {
            PackDockManager.Instance.TryToAddPack(gachaPackManagerSO.GetGachaPackCurrentArena(gachaPackRarity));
        });
    }
}
