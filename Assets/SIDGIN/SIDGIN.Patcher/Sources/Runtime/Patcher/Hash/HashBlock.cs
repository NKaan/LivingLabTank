namespace SIDGIN.Patcher.Internal.Hash
{
    class HashBlock
    {
        #region Properties, indexers, events and operators: public

        internal uint Checksum { get; set; }
        internal byte[] Hash { get; set; }
        internal int Length { get; set; }
        internal long Offset { get; set; }

        #endregion
    }
}