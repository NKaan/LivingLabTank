namespace SIDGIN.Patcher.Internal.Delta
{
    using System;

    class DeltaStatistics
    {
        private readonly int _blockSize;

        internal int BucketCollisions;
        internal long FileLength;
        internal int Matching;
        internal int NonMatching;
        internal int PossibleMatches;

        internal DeltaStatistics(int blockSize)
        {
            _blockSize = blockSize;
        }

        #region Methods: public

        internal void Dump()
        {
            Console.Out.WriteLine("Matched blocks: {0}, False positives due to weak checksum: {1}",
                                  Matching,
                                  PossibleMatches - Matching);
            Console.Out.WriteLine("Bytes saved: {0}, Bytes total: {1} => {2}% transfer size reduction.",
                                  Matching*_blockSize,
                                  FileLength,
                                  (int) (((float) Matching*_blockSize/FileLength)*100));
        }

        #endregion
    }
}