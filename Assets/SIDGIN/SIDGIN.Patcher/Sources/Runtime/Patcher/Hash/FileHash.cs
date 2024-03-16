namespace SIDGIN.Patcher.Internal.Hash
{
    using System.Collections.Generic;

    class FileHash
    {
        #region Properties, indexers, events and operators: public

        internal IEnumerable<HashBlock> Hash { get; set; }
        internal string Path { get; set; }

        #endregion
    }
}