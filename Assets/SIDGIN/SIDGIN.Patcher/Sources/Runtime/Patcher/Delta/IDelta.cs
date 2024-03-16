namespace SIDGIN.Patcher.Internal.Delta
{
    using System.IO;

    interface IDelta
    {
        #region Methods: public

        int Length { get; set; }
        long Offset { get; set; }

        #endregion
    }
}