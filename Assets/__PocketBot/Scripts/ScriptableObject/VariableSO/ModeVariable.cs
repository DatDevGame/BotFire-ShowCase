using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HyrphusQ.SerializedDataStructure;

[CreateAssetMenu(fileName = "ModeVariable", menuName = "PocketBots/VariableSO/Mode")]
public class ModeVariable : Variable<Mode>
{
    private string m_key = $"ModeVariable-SaveMode";

    public Sprite CurrentSpriteMode => SpriteMode[value];
    public SerializedDictionary<Mode, Sprite> SpriteMode;
    public void AdjustSaveMode() => GetSaveMode();

    public Mode GameMode
    {
        get
        {
            return GetSaveMode();
        }

        set
        {
            int index = value switch
            {
                Mode.Normal => 0,
                Mode.Battle => 1,
                Mode.Boss => 2,
                _ => 99,
            };
            PlayerPrefs.SetInt(m_key, index);
            value = GetSaveMode();
        }
    }

    private Mode GetSaveMode()
    {
        if (!PlayerPrefs.HasKey(m_key))
        {
            PlayerPrefs.SetInt(m_key, 0);
        }
        int intdex = PlayerPrefs.GetInt(m_key);
        Mode mode = intdex switch
        {
            0 => Mode.Normal,
            1 => Mode.Battle,
            2 => Mode.Boss,
            _ => Mode.Normal
        };

        value = mode;
        return mode;
    }
}