[EventCode]
public enum BossFightEventCode
{
    /// <summary>
    /// Raised when opening the boss map UI.
    /// </summary>
    OnBossMapOpened,
    /// <summary>
    /// Raised when closing the boss map UI.
    /// </summary>
    OnBossMapClosed,
    /// <summary>
    /// Raised when unlocking the boss.
    /// </summary>
    OnUnlockBoss,
    /// <summary>
    /// Raised when end unlocking the boss.
    /// </summary>
    OnDisableUnlockBoss,
    /// <summary>
    /// Raised when claim boss Complete in Inventory.
    /// </summary>
    OnClaimBossComplete,
    /// <summary>
    /// Raised when select boss in Inventory.
    /// </summary>
    OnSelectBossLockInventory,
    /// <summary>
    /// Raised when claim reward.
    /// </summary>
    OnClaimReward,
}
