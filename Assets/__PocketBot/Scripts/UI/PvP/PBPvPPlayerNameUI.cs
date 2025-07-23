using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PBPvPPlayerNameUI : MonoBehaviour
{
    [SerializeField] Image avatar;
    [SerializeField] Image flagIcon;
    [SerializeField] TMP_Text nameTMP;

    PersonalInfo personalInfo;

    public PersonalInfo PersonalInfo
    {
        get => personalInfo;
        set
        {
            personalInfo = value;
            UpdateView();
        }
    }

    void UpdateView()
    {
        if (personalInfo == null)
        {
            flagIcon.gameObject.SetActive(false);
            return;
        }
        avatar.sprite = personalInfo.avatar;
        flagIcon.gameObject.SetActive(true);
        flagIcon.sprite = personalInfo.nationalFlag;
        nameTMP.text = personalInfo.name;
    }
}
