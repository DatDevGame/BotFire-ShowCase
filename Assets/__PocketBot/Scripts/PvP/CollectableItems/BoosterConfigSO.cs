using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BoosterConfigSO", menuName = "PocketBots/BoosterConfigSO")]
public class BoosterConfigSO : ScriptableObject
{
    [SerializeField]
    private float showBoosterTextDuration = 1f;
    [SerializeField]
    private float boosterModelMovingSpeed = 0.5f;
    [SerializeField]
    private float boosterModelRotationSpeed = 50f;
    [SerializeField]
    private Vector3 boosterModelMovingRange = Vector3.up;
    [SerializeField]
    private List<Booster> boosters;

    public float BoosterModelMovingSpeed => boosterModelMovingSpeed;
    public float BoosterModelRotationSpeed => boosterModelRotationSpeed;
    public Vector3 BoosterModelMovingRange => boosterModelMovingRange;

    public Booster GetBooster(PvPBoosterType boosterType)
    {
        foreach (var booster in boosters)
        {
            if (booster.BoosterType == boosterType)
                return booster;
        }
        return null;
    }

    public float GetBoosterTextDuration()
    {
        return showBoosterTextDuration;
    }

    public float GetBoostPercent(PvPBoosterType boosterType)
    {
        var booster = GetBooster(boosterType);
        return booster == null ? Const.FloatValue.ZeroF : booster.BoostPercent;
    }

    [Serializable]
    public class Booster
    {
        [SerializeField]
        private PvPBoosterType boosterType;
        [SerializeField, Range(0, 1f)]
        private float boostPercent = 0.2f;
        [SerializeField]
        private string boosterDesc;
        [SerializeField]
        private string boosterDescDecrease;
        [SerializeField]
        private float duration = 5f;

        public PvPBoosterType BoosterType => boosterType;
        public float BoostPercent => boostPercent;
        public string BoosterDesc => boosterDesc.Replace(Const.StringValue.PlaceholderValue, Mathf.RoundToInt(BoostPercent * 100).ToString());
        public string BoosterDescDecrease => boosterDescDecrease.Replace(Const.StringValue.PlaceholderValue, Mathf.RoundToInt(BoostPercent * 100).ToString());
        public float Duration => duration;
    }
}