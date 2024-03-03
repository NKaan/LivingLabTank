using MasterServerToolkit.Extensions;

namespace MasterServerToolkit.MasterServer
{
    public partial struct ProfilePropertyOpCodes
    {
        public static ushort displayName = nameof(displayName).ToUint16Hash();
        public static ushort avatarUrl = nameof(avatarUrl).ToUint16Hash();
        public static ushort tankColor = nameof(tankColor).ToUint16Hash();
        public static ushort bronze = nameof(bronze).ToUint16Hash();
        public static ushort silver = nameof(silver).ToUint16Hash();
        public static ushort gold = nameof(gold).ToUint16Hash();
        public static ushort spice = nameof(spice).ToUint16Hash();
        public static ushort wood = nameof(wood).ToUint16Hash();
        public static ushort items = nameof(items).ToUint16Hash();


        public static ushort playerLevel = nameof(playerLevel).ToUint16Hash();
        public static ushort playerExp = nameof(playerExp).ToUint16Hash();
        public static ushort needExp = nameof(needExp).ToUint16Hash();


        public static ushort usedStatPoint = nameof(usedStatPoint).ToUint16Hash();

        public static ushort damageStatPoint = nameof(damageStatPoint).ToUint16Hash();
        public static ushort speedStatPoint = nameof(speedStatPoint).ToUint16Hash();
        public static ushort healthStatPoint = nameof(healthStatPoint).ToUint16Hash();
        public static ushort fuelStatPoint = nameof(fuelStatPoint).ToUint16Hash();


    }
}