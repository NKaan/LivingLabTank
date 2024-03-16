using System;
using SIDGIN.Patcher.Client;
namespace SIDGIN.Patcher.Common
{
    public delegate void PatcherError(Exception ex); 
    public static class ErrorHandler
    {
        public static event PatcherError onErrorHandled;
        public static void Handle(Exception ex)
        {
            if (ex is UnauthorizedAccessException || ex.InnerException is UnauthorizedAccessException)
            {
                var accessException = new UnauthorizedAccessException(Localize.Get("No_permissions"), ex.InnerException);
                onErrorHandled?.Invoke(accessException);
                throw accessException;
            }
            onErrorHandled?.Invoke(ex);
            throw ex;
        }
    }
}
