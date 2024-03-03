using MasterServerToolkit.Extensions;

namespace MasterServerToolkit.Examples.BasicProfile
{
    public struct MessageOpCodes
    {
        public static ushort BuyDemoItem = nameof(BuyDemoItem).ToUint16Hash();
        public static ushort SellDemoItem = nameof(SellDemoItem).ToUint16Hash();
        public static ushort GMPlayerDeath = nameof(GMPlayerDeath).ToUint16Hash();
        public static ushort CMAddStatPoint = nameof(CMAddStatPoint).ToUint16Hash();
    }
}