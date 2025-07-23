using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BossClaimByRVSO", menuName = "PocketBots/BossFight/BossClaimByRVSO")]
public class BossClaimByRVSO : SavedDataSO<BossClaimByRVSO.Data>
{
    [Serializable]
    public class Data : SavedData
    {
        [SerializeField]
        private Dictionary<string, int> claimRVProgress = new Dictionary<string, int>();

        public int this[string claimByRVAmountKey]
        {
            get => claimRVProgress.TryGetValue(claimByRVAmountKey, out int watchCount) ?  watchCount : 0;
            set => claimRVProgress[claimByRVAmountKey] = value;
        }
    }
}
