using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using HyrphusQ.Events;
using Sirenix.OdinInspector;
using UnityEngine.UI;

public class DamageTextManager : MonoBehaviour
{
    [SerializeField, BoxGroup("Config")] private Color m_ColorTextPlayer;
    [SerializeField, BoxGroup("Config")] private Color m_ColorTextOpponent;

    [SerializeField] ModeVariable currentChosenModeVariable;
    [SerializeField] float timePerText = 1f;
    [SerializeField] float textFollowSpeed = 0.5f;
    [SerializeField] int poolAmount;
    [SerializeField] TextMeshProUGUI textPrefab;
    [SerializeField] Vector3 flyingUpOffset;
    [SerializeField] AnimationCurve flyingUpSpeedCurve;

    List<DamageTextData> textPool = new();
    int poolIndex = 0;
    Camera mainCam;

    private void Awake()
    {
        mainCam = MainCameraFindCache.Get();
        if (currentChosenModeVariable.value == Mode.Battle) poolAmount *= 2; //Increase pool amount because of increasing amount of players
        InitPool();
        GameEventHandler.AddActionEvent(RobotStatusEventCode.OnRobotDamaged, HandleRobotDamaged);
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(RobotStatusEventCode.OnRobotDamaged, HandleRobotDamaged);
    }

    void HandleRobotDamaged(object[] parameters)
    {
        if (parameters == null) return;
        PBRobot pbRobot = (PBRobot)parameters[0];
        PBRobot attacker = (parameters[3] as PBPart)?.RobotChassis?.Robot;
        if (attacker == null || pbRobot == null || !pbRobot.PersonalInfo.isLocal && !attacker.PersonalInfo.isLocal)
            return;
        float damage = (float)Convert.ToDouble(parameters[1]);
        var textData = textPool[poolIndex++ % textPool.Count];
        var damageText = textData.text;
        textData.worldPos = pbRobot.ChassisInstanceTransform.position;
        textData.hideTime = Time.time + timePerText;
        damageText.gameObject.SetActive(false);
        damageText.text = damage.ToRoundedText();

        Image iconDamage = damageText.GetComponentInChildren<Image>();
        if (pbRobot.PlayerInfoVariable.value.isLocal)
        {
            damageText.color = m_ColorTextPlayer;
            iconDamage.color = m_ColorTextPlayer;
        }
        else
        {
            damageText.color = m_ColorTextOpponent;
            iconDamage.color = m_ColorTextOpponent;
        }
        ShowText(textData);
    }

    private void LateUpdate()
    {
        foreach (var textData in textPool)
        {
            if (textData.text.gameObject.activeInHierarchy == false) continue;
            float lifeCycleTime = Mathf.InverseLerp(textData.showTime, textData.hideTime, Time.time);
            if (lifeCycleTime >= 1) HideText(textData);
            else
            {
                Vector3 textTargetPos = GetTextScreenPos(textData.worldPos, lifeCycleTime);
                Transform textTransform = textData.text.transform;
                textTransform.position = Vector3.Lerp(textTransform.position, textTargetPos, textFollowSpeed);
            }
        }
    }

    Vector3 GetTextScreenPos(Vector3 worldPos, float lifeCycleTime)
    {
        Vector3 textOrgPos = mainCam.WorldToScreenPoint(worldPos);
        Vector3 textEndPos = textOrgPos + flyingUpOffset;
        Vector3 textTargetPos = Vector3.Lerp(textOrgPos, textEndPos, flyingUpSpeedCurve.Evaluate(lifeCycleTime));
        return textTargetPos;
    }

    void ShowText(DamageTextData textData)
    {
        var damageText = textData.text;
        textData.showTime = Time.time;
        damageText.transform.position = GetTextScreenPos(textData.worldPos, 0);
        damageText.gameObject.SetActive(true);
    }

    void HideText(DamageTextData textData)
    {
        var damageText = textData.text;
        damageText.gameObject.SetActive(false);
    }

    void InitPool()
    {
        for (int i = 0; i < poolAmount; i++)
        {
            var text = Instantiate(textPrefab, transform);
            text.gameObject.SetActive(false);
            textPool.Add(new DamageTextData(text));
        }
    }

    public class DamageTextData
    {
        public TextMeshProUGUI text;
        public Vector3 worldPos;
        public float hideTime;
        public float showTime;

        public DamageTextData(TextMeshProUGUI text)
        {
            this.text = text;
        }
    }
}
