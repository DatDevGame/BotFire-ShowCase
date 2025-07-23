public enum PBStatID
{
    Health,
    Attack,
    Power,
    Resistance,
    Turning,
    StatsScore = 60
}

public interface IPartStats
{
    public IStat<PBStatID, float> GetHealth();
    public IStat<PBStatID, float> GetAttack();
    public IStat<PBStatID, float> GetPower();
    public IStat<PBStatID, float> GetResistance();
    public IStat<PBStatID, float> GetTurning();
    public IStat<PBStatID, float> GetStatsScore();
}

public struct PBStat<TValue> : IStat<PBStatID, TValue>
{
    #region Constructor
    public PBStat(PBStatID id, TValue value)
    {
        m_Id = id;
        m_Value = value;
    }
    #endregion

    private PBStatID m_Id;
    private TValue m_Value;

    public PBStatID id => m_Id;
    public string label => "Example & No Implementation";
    public TValue value => m_Value;
}
