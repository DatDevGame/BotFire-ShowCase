using System;
using UnityEngine;

[System.Serializable]
public class DateTimeSerialized
{
    [SerializeField]
    private long dateTimeTicks;

    public DateTimeSerialized() { }

    public DateTimeSerialized(DateTime dateTime)
    {
        DateTime = dateTime;
    }

    public DateTime DateTime
    {
        get => new DateTime(dateTimeTicks);
        set => dateTimeTicks = value.Ticks;
    }

    public static implicit operator DateTimeSerialized(DateTime dateTime) => new DateTimeSerialized(dateTime);
    public static implicit operator DateTime(DateTimeSerialized dateTimeSerialized) => dateTimeSerialized.DateTime;
}
