namespace SIDGIN.Patcher.Internal.Helpers
{
    static class MathEx
    {
        #region Bounded

        internal static int Bounded(int min, int max, int value)
        {
            return value < min ? min : (value > max ? max : value);
        }

        internal static uint Bounded(uint min, uint max, uint value)
        {
            return value < min ? min : (value > max ? max : value);
        }

        internal static decimal Bounded(decimal min, decimal max, decimal value)
        {
            return value < min ? min : (value > max ? max : value);
        }

        internal static short Bounded(short min, short max, short value)
        {
            return value < min ? min : (value > max ? max : value);
        }

        internal static ushort Bounded(ushort min, ushort max, ushort value)
        {
            return value < min ? min : (value > max ? max : value);
        }

        internal static long Bounded(long min, long max, long value)
        {
            return value < min ? min : (value > max ? max : value);
        }

        internal static ulong Bounded(ulong min, ulong max, ulong value)
        {
            return value < min ? min : (value > max ? max : value);
        }

        #endregion
    }
}