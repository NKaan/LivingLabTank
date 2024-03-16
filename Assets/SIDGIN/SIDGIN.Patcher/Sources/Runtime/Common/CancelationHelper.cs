using System.Threading;
namespace SIDGIN.Patcher.Common
{
    public class CancelationHelper
    {
        public static CancellationToken CancelToken { get { return cancelTokenSource.Token; } }
        static CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
        public static void Cancel()
        {
            cancelTokenSource.Cancel();
            cancelTokenSource = new CancellationTokenSource();
        }
    }
}
