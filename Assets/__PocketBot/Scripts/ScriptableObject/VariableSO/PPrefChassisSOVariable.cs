using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PPrefChassisSOVariable : PPrefItemSOVariable
{
    [SerializeField]
    protected List<ItemListSO> m_ItemListSOs;

    public override ItemSO value
    {
        get
        {
            if (m_RuntimeValue == null)
            {
                if (m_ItemListSOs == null || m_ItemListSOs.Count <= 0)
                    return m_InitialValue;
                var itemGuid = PlayerPrefs.GetString(m_Key, m_InitialValue?.guid ?? string.Empty);
                foreach (var itemListSO in m_ItemListSOs)
                {
                    var items = itemListSO.value;
                    foreach (var item in items)
                    {
                        if (item.guid == itemGuid)
                        {
                            m_RuntimeValue = item;
                            break;
                        }
                    }
                }
            }
            return m_RuntimeValue ?? m_InitialValue;
        }
        set
        {
            PlayerPrefs.SetString(m_Key, value == null ? string.Empty : value.guid);
            base.value = value;
        }
    }
}