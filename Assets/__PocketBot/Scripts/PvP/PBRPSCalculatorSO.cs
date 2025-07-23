using System.Collections;
using System.Collections.Generic;
using LatteGames.PvP;
using UnityEngine;

[CreateAssetMenu(fileName = "RPSCalculatorSO", menuName = "PocketBots/PvP/RPSCalculatorSO")]
public class PBRPSCalculatorSO : RPSCalculatorSO
{
    public class PBRPSData : RPSData
    {
        #region Constructor
        public PBRPSData(float scoreOfPlayer, float scoreOfOpponent) : base(scoreOfPlayer, scoreOfOpponent) { }
        #endregion

        private static readonly RangeValue<float> RPSRange = new RangeFloatValue(-1f, 1f) { minValue = -1f, maxValue = 1f };
        private static readonly RangeValue<float> EvenRange = new RangeFloatValue(-0.15f, 0.15f) { minValue = -0.15f, maxValue = 0.15f };

        public override float rpsInverseLerp => 1f - base.rpsInverseLerp;

        public override float rpsValue
        {
            get
            {
                var rawRps = (scoreOfPlayer - scoreOfOpponent) / (scoreOfPlayer + scoreOfOpponent);
                return Mathf.Clamp(rawRps * 3f, rpsRange.minValue, rpsRange.maxValue);
            }
        }

        public override string stateLabel
        {
            get
            {
                if (rpsValue >= EvenRange.minValue && rpsValue <= EvenRange.maxValue)
                    return I2LHelper.TranslateTerm(I2LTerm.Text_Even);
                else if (rpsValue > EvenRange.maxValue)
                    return I2LHelper.TranslateTerm(I2LTerm.Text_Easy);
                else
                    return I2LHelper.TranslateTerm(I2LTerm.Text_Hard);

            }
        }

        /// <summary>
        /// <para>1: Player advantage</para>
        /// <para>0: Event</para>
        /// <para>-1: Opponent advantage</para>
        /// </summary>
        public int stateLabelInternal
        {
            get
            {
                if (rpsValue > EvenRange.maxValue)
                {
                    return 1; // Player advantage
                }
                else if (rpsValue > EvenRange.minValue)
                {
                    return 0; // Even
                }
                else
                {
                    return -1; // Opponent advantage
                }
            }
        }

        public override RangeValue<float> rpsRange => RPSRange;
    }

    public override RPSData CalcRPSValue(float scoreOfPlayer, float scoreOfOpponent)
    {
        return new PBRPSData(scoreOfPlayer, scoreOfOpponent);
    }
}
