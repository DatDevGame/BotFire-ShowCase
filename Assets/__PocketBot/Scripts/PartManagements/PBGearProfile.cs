using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PBGearProfileEnum
{
    HorizontalSpinner,
    DrumSpinner,
    Shield,
    Lifter,
    Smasher,
    Flamethrower,
    Wheel,
    RocketGun,
    Puncher,
    StandStill
}

public enum PBDamageTypeEnum { Impact, Continuous }

public static class PBGearProfile
{
    public static T GetGearProfile<T>(PBGearProfileEnum gearProfile) where T : AbstractGearProfile //Refactor Gear profile when has time
    {
        return null;
    }
}

public class AbstractGearProfile
{
    public PBDamageTypeEnum DamageType;
    public float PerHitRatio;
}
