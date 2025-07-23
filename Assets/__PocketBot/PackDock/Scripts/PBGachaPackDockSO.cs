using System.Collections;
using System.Collections.Generic;
using GachaSystem.Core;
using Sirenix.Utilities;
using UnityEngine;

[CreateAssetMenu(fileName = "PBGachaPackDockSO", menuName = "PocketBots/PackDock/PBGachaPackDockSO")]
public class PBGachaPackDockSO : GachaPackDockSO
{
    [SerializeField] GachaPack defaultGachaPack;
    public override void Load()
    {
        var settings = new ES3Settings(m_SaveFileName, m_EncryptionType, k_EncryptionPassword);
        m_Data = ES3.Load(m_Key, defaultData, settings);

        NotifyEventDataLoaded();

        for (var i = data.gachaPackDockSlots.Count; i < slotAmount; i++)
        {
            data.gachaPackDockSlots.Add(new());
        }
        gachaPackDockSlots = data.gachaPackDockSlots;
        foreach (var slot in gachaPackDockSlots)
        {
            if (slot.GachaPack == null && !slot.gachaPackGUID.IsNullOrWhitespace())
            {
                var pack = gachaPacksList.Packs.Find(x => x.guid == slot.gachaPackGUID);
                if (pack != null)
                {
                    var packInstance = Instantiate(pack);
                    packInstance.UnlockedDuration = slot.gachaPackUnlockedDuration;
                    slot.GachaPack = packInstance;
                }
                else
                {
                    var packInstance = Instantiate(defaultGachaPack);
                    packInstance.UnlockedDuration = slot.gachaPackUnlockedDuration;
                    slot.GachaPack = packInstance;
                }
            }
        }
    }
}
