using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GachaSystem.Core;
using HyrphusQ.Events;
using Sirenix.OdinInspector;
using LatteGames.UnpackAnimation;

namespace PB.UnitTest
{
    public class TestOpenGachaPack : MonoBehaviour
    {
        public GachaPack pack;
        public OpenPackAnimationSM openPackAnimationSM;
        public SummaryToPrepareTransitionSO SummaryToPrepareTransitionSO;
        public SummaryStateSO SummaryStateSO;

        [Button]
        public void OpenPack()
        {
            GameEventHandler.Invoke(UnpackEventCode.OnUnpackStart, null, new List<GachaPack>() { pack }, null);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.U))
            {
                OpenPack();
            }
        }
    }
}