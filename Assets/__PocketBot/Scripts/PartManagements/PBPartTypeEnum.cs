[SerializeEnum]
public enum PBPartType
{
    Body,
    Upper,
    Front,
    Wheels,
}
[SerializeEnum]
public enum PBPartSlot
{
    Body,
    Upper_1,
    Upper_2,
    Front_1,
    Wheels_1,
    Wheels_2,
    Wheels_3,
    Front_2,
    PrebuiltBody
}
public static class PBPartTypeEnum
{
    public static PBPartType GetPartTypeOfPartSlot(this PBPartSlot partSlot)
    {
        PBPartType partType = (PBPartType)(-1);
        switch (partSlot)
        {
            case PBPartSlot.Body:
            case PBPartSlot.PrebuiltBody:
                partType = PBPartType.Body;
                break;
            case PBPartSlot.Wheels_1:
            case PBPartSlot.Wheels_2:
            case PBPartSlot.Wheels_3:
                partType = PBPartType.Wheels;
                break;
            case PBPartSlot.Front_1:
            case PBPartSlot.Front_2:
                partType = PBPartType.Front;
                break;
            case PBPartSlot.Upper_1:
            case PBPartSlot.Upper_2:
                partType = PBPartType.Upper;
                break;
            default:
                break;
        }
        return partType;
    }
}