public interface IBoostFireRate
{
    public void BoostSpeedUpFire(float boosterPercent);
    public void BoostSpeedUpStop(float boosterPercent);
    public bool IsSpeedUp();
    public int GetStackSpeedUp();
    public float GetPercentSpeedUp();
}
