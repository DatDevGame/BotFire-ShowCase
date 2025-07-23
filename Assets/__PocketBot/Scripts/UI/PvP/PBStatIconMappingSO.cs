using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StatIconMapping", menuName = "PocketBots/ScriptableObjects/StatIconMapping")]
public class PBStatIconMappingSO : ScriptableObject
{
    [SerializeField] List<StatMap> StatMaps = new();

    public Sprite GetStatIcon(PBStatID statID)
    {
        return StatMaps.Find(map => map.StatID == statID).StatIcon;
    }

    [System.Serializable]
    struct StatMap
    {
        public PBStatID StatID;
        public Sprite StatIcon;
    }
}
