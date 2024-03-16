namespace SIDGIN.Patcher.Internal.Hash
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Helpers;

    class HashBlockStreamer
    {
        #region Methods: public

        internal static HashBlock[] Destream(Stream inputStream)
        {
            uint count = inputStream.ReadUInt();
            var hashBlocks = new HashBlock[count];
            for (int i = 0; i < count; ++i)
            {
                hashBlocks[i] = new HashBlock {Hash = new byte[16]};
                inputStream.Read(hashBlocks[i].Hash, 0, 16);
                hashBlocks[i].Length = inputStream.ReadInt();
                hashBlocks[i].Offset = inputStream.ReadLong();
                hashBlocks[i].Checksum = inputStream.ReadUInt();
            }
            return hashBlocks;
        }

        internal static void Stream(IEnumerable<HashBlock> hashBlocks, Stream outputStream)
        {
            outputStream.WriteUInt((uint) hashBlocks.Count());
            foreach (HashBlock block in hashBlocks)
            {
                outputStream.Write(block.Hash, 0, 16);
                outputStream.WriteInt(block.Length);
                outputStream.WriteLong(block.Offset);
                outputStream.WriteUInt(block.Checksum);
            }
        }

        #endregion
    }
}