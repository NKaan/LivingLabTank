namespace SIDGIN.Patcher.Delta
{
    public class FileHashesDontMatch : System.Exception
    {
        public FileHashesDontMatch(string message, System.Exception iternalException = null) : base(message, iternalException)
        {

        }
    }
}
