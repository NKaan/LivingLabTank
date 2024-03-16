namespace SIDGIN.Patcher.Internal.Hash
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Libraries;

    class HashBlockGenerator
    {
        internal IRollingChecksum ChecksumProvider { get; set; }
        internal IHashAlgorithm HashAlgorithm { get; set; }
        private readonly int _blockSize;

        internal HashBlockGenerator(IRollingChecksum checksumProvider, IHashAlgorithm hashProvider, int blockSize)
        {
            if (checksumProvider == null) throw new ArgumentNullException("checksumProvider");
            if (hashProvider == null) throw new ArgumentNullException("hashProvider");
            if (blockSize <= 0) throw new ArgumentException("blockSize must be greater than zero");
            ChecksumProvider = checksumProvider;
            HashAlgorithm = hashProvider;
            _blockSize = blockSize;
        }

        #region Methods: public

        internal IEnumerable<HashBlock> ProcessStream(Stream inputStream)
        {
            if (inputStream == null) throw new ArgumentNullException("inputStream");
            int read;
            var buffer = new byte[_blockSize];
            long offset = 0;
            while ((read = inputStream.Read(buffer, 0, _blockSize)) > 0)
            {
                ChecksumProvider.ProcessBlock(buffer, 0, read);
                yield return new HashBlock
                                 {
                                     Hash = HashAlgorithm.ComputeHash(buffer, 0, read),
                                     Checksum = ChecksumProvider.Value,
                                     Offset = (uint) offset,
                                     Length = read
                                 };
                offset += read;
            }
        }

        #endregion
    }
}