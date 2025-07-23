using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public partial class Const
{
    public static class ResourceItemId
    {
        // Free money
        public const string MainScreen = "Main screen";
        public const string PvPChooseArena = "PvP Choose Arena";
        public const string GetX2Money = "Get x2 money";
        public const string GetBackMoney = "Get back money";
        // Money Pack
        public const string Exchange = "Exchange";
        public const string MoneyPack = "{MoneyPack}";
        // Gem Pack
        public const string GemPack = "{GemPack}";
        // PvP
        public const string PvPVictory = "A";
        public const string PvPLose = "{ArenaType} Lose";
        public const string PvPEnterFee = "{ArenaType} Enter Fee";
        // Box
        public const string PvPBox = "PvP box";
        public const string IAPBox = "IAP box";
        public const string FreeBox = "FreeBox";
        // Trophy
        public const string TrophyReward = "Reward";
        // Upgrade
        public const string CharacterUpgrade = "Character Upgrade";
        public const string GearUpgrade = "Gear Upgrade";

        public static List<string> GetAllItemIds()
        {
            FieldInfo[] fieldInfos = typeof(ResourceItemId).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            return fieldInfos.Where(fi => fi.IsLiteral && !fi.IsInitOnly).Select(fi => fi.GetRawConstantValue() as string).ToList();
        }
    }

    public static class UpgradeValue
    {
        public const float BaseHp = 350;
        public const float HpStepPercent = 0.1f;

        public const float BaseAttack = 10;
        public const float AttackStepPercent = 0.1f;

        public const float StatsFactor = 0.1f;

        public const float Virtual_Atk = 5;
    }

    public static class CollideValue
    {
        public const float ReceiveCollisionForceCooldown = 3f / 50f;
        public const float ImpactDamageCooldown = 0.5f;
        public const float ContinuousDamageCooldown = 0.2f;
        public const float ContinuousTriggerDamageCooldown = 0.4f;
    }

    public static class PvPValue
    {
        public const float StartMatchCountdownTime = 2;
    }

    public static class PBLayerMask
    {
        public static readonly int Ground = LayerMask.GetMask("Ground");
        public static readonly int Wall = LayerMask.GetMask("Wall");
        public static readonly int LocalPlayerRobot = LayerMask.GetMask("PlayerPart");
        public static readonly int OtherPlayerRobots = LayerMask.GetMask("EnemyPart1", "EnemyPart2", "EnemyPart3", "EnemyPart4", "EnemyPart5");
        public static readonly int AllRobots = LocalPlayerRobot | OtherPlayerRobots;
    }
}
