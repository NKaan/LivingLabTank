namespace SIDGIN.Patcher.Internal.Libraries
{
    interface IHashAlgorithm
    {
        #region Properties, indexers, events and operators: public

        int HashSize { get; }

        #endregion

        #region Methods: public

        byte[] ComputeHash(byte[] buffer, int offset, int length);
        void Clear();

        #endregion
    }
}