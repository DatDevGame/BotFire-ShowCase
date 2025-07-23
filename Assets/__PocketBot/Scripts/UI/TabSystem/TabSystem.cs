using System.Collections.Generic;
using HyrphusQ.Events;
using Sirenix.OdinInspector;
using UnityEngine;

namespace LatteGames.Tab
{

    public enum SeasonTabEvent
    {
        TransitionTab
    }
    public class TabSystem : MonoBehaviour
    {
        [Header("Tab Settings")]
        public List<TabPair> tabPairs;
        [SerializeField]
        protected int defaultTabIndex = 0;

        protected int activeTabIndex;

        protected virtual void Awake()
        {
            if (tabPairs.Count == 0)
            {
                Debug.LogError("No TabPairs found!");
                return;
            }

            InitializeTabs();
            ActivateTab(defaultTabIndex); // Activate the first tab by default
        }

        protected virtual void InitializeTabs()
        {
            // Initialize the TabButton click events
            for (int i = 0; i < tabPairs.Count; i++)
            {
                int index = i; // Capture the index for the closure
                tabPairs[i].tabButton.Initialize(() => ActivateTab(index));
            }
        }

        public void ActiveDefaultTab()
        {
            ActivateTab(defaultTabIndex);
        }

        public void ActivateTab(int index)
        {
            if (index < 0 || index >= tabPairs.Count)
            {
                Debug.LogError("Invalid tab index.");
                return;
            }

            // Deactivate all tabs
            for (int i = 0; i < tabPairs.Count; i++)
            {
                tabPairs[i].tabContent.SetActive(i == index);
                tabPairs[i].tabButton.SetState(i == index);
            }
            activeTabIndex = index;

            GameEventHandler.Invoke(SeasonTabEvent.TransitionTab, index);
        }
    }

    [System.Serializable]
    public class TabPair
    {
        [HorizontalGroup]
        public TabButton tabButton;
        [HorizontalGroup]
        public TabContent tabContent;
    }
}