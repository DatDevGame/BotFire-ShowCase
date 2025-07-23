using System.Collections.Generic;

public static class GearSlotHelper
{
    public static PBPartSlot GetSlotPartType(PBPartType pbPartType, int slotIndex)
    {
        List<PBPartSlot> upperPartSlots = new List<PBPartSlot>();
        upperPartSlots.Add(PBPartSlot.Upper_1);
        upperPartSlots.Add(PBPartSlot.Upper_2);

        List<PBPartSlot> frontPartSlots = new List<PBPartSlot>();
        frontPartSlots.Add(PBPartSlot.Front_1);
        frontPartSlots.Add(PBPartSlot.Front_2);

        List<PBPartSlot> wheelPartSlots = new List<PBPartSlot>();
        wheelPartSlots.Add(PBPartSlot.Wheels_1);
        wheelPartSlots.Add(PBPartSlot.Wheels_2);

        return pbPartType switch
        {
            PBPartType.Upper => upperPartSlots[slotIndex],
            PBPartType.Front => frontPartSlots[slotIndex],
            _ => wheelPartSlots[slotIndex]
        };
    }

    public static int GetSlotIndex(PBPartSlot pbPartSlot)
    {
        return pbPartSlot switch
        {
            PBPartSlot.Upper_1 => 0,
            PBPartSlot.Upper_2 => 1,
            PBPartSlot.Front_1 => 0,
            PBPartSlot.Front_2 => 1,
            PBPartSlot.Wheels_1 => 0,
            PBPartSlot.Wheels_2 => 1,
            _ => 0
        };
    }
}
