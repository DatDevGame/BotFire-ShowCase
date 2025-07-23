using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "ABTestAIProfileSO", menuName = "PocketBots/ABTest/ABTestAIProfileSO")]
public class ABTestAIProfileSO : GroupBasedABTestSO
{
    [Serializable]
    public class AIProfileData
    {
        public float movingSpeed;
        public float steeringSpeed;
        public float movingSpeedWhenAttack;
        public float steeringSpeedWhenAttack;
        public float collectBoosterProbability;
        public float lowerOverallscoreCollectBoosterProbability;
        public PB_AIProfile aiProfile;

        public void InjectData()
        {
            aiProfile.MovingSpeed = movingSpeed;
            aiProfile.SteeringSpeed = steeringSpeed;
            aiProfile.MovingSpeedWhenAttack = movingSpeedWhenAttack;
            aiProfile.SteeringSpeedWhenAttack = steeringSpeedWhenAttack;
            aiProfile.CollectBoosterProbability = collectBoosterProbability;
            aiProfile.LowerOverallscoreCollectBoosterProbability = lowerOverallscoreCollectBoosterProbability;
        }

        public void RetrieveData(PB_AIProfile aiProfile)
        {
            this.aiProfile = aiProfile;
            movingSpeed = aiProfile.MovingSpeed;
            steeringSpeed = aiProfile.SteeringSpeed;
            movingSpeedWhenAttack = aiProfile.MovingSpeedWhenAttack;
            steeringSpeedWhenAttack = aiProfile.SteeringSpeedWhenAttack;
            collectBoosterProbability = aiProfile.CollectBoosterProbability;
            lowerOverallscoreCollectBoosterProbability = aiProfile.LowerOverallscoreCollectBoosterProbability;
        }
    }
    [Serializable]
    public class AIProfileDataGroup
    {
        [TableList]
        public List<AIProfileData> dataOfAIProfiles;

        public void InjectData()
        {
            for (int i = 0; i < dataOfAIProfiles.Count; i++)
            {
                dataOfAIProfiles[i].InjectData();
            }
        }

        public void RetrieveData(List<PB_AIProfile> aiProfiles)
        {
            for (int i = 0; i < dataOfAIProfiles.Count; i++)
            {
                dataOfAIProfiles[i].RetrieveData(aiProfiles[i]);
            }
        }
    }
    [SerializeField]
    private List<AIProfileDataGroup> aiProfileGroups;

    public override void InjectData(int groupIndex)
    {
        aiProfileGroups[groupIndex].InjectData();
    }

    public virtual void RetrieveData(List<PB_AIProfile> aiProfiles, int groupIndex)
    {
        aiProfileGroups[groupIndex].RetrieveData(aiProfiles);
    }
}