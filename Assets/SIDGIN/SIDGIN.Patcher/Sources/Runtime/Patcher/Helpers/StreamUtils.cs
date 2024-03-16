namespace SIDGIN.Patcher.Internal.Helpers
{
    using System;
    using System.IO;

    static class StreamExtensions
    {
        #region Methods: public

        internal static int ReadInt(this Stream stream)
        {
            var uintBuffer = new byte[4];
            if (stream.Read(uintBuffer, 0, 4) != 4)
            {
                throw new IOException("Not enough data available on stream");
            }
            return BitConverter.ToInt32(uintBuffer, 0);
        }

        internal static long ReadLong(this Stream stream)
        {
            var uintBuffer = new byte[8];
            if (stream.Read(uintBuffer, 0, 8) != 8)
            {
                throw new IOException("Not enough data available on stream");
            }
            return BitConverter.ToInt64(uintBuffer, 0);
        }

        internal static uint ReadUInt(this Stream stream)
        {
            var uintBuffer = new byte[4];
            if (stream.Read(uintBuffer, 0, 4) != 4)
            {
                throw new IOException("Not enough data available on stream");
            }
            return BitConverter.ToUInt32(uintBuffer, 0);
        }

        internal static ulong ReadULong(this Stream stream)
        {
            var uintBuffer = new byte[8];
            if (stream.Read(uintBuffer, 0, 8) != 8)
            {
                throw new IOException("Not enough data available on stream");
            }
            return BitConverter.ToUInt64(uintBuffer, 0);
        }

        internal static void WriteInt(this Stream stream, int value)
        {
            stream.Write(BitConverter.GetBytes(value), 0, sizeof (int));
        }

        internal static void WriteLong(this Stream stream, long value)
        {
            stream.Write(BitConverter.GetBytes(value), 0, sizeof (long));
        }

        internal static void WriteUInt(this Stream stream, uint value)
        {
            stream.Write(BitConverter.GetBytes(value), 0, sizeof (uint));
        }

        internal static void WriteULong(this Stream stream, ulong value)
        {
            stream.Write(BitConverter.GetBytes(value), 0, sizeof (ulong));
        }

        #endregion
    }
}