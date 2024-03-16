using System;

namespace SIDGIN.Patcher.Common
{
    public delegate void InternalError(Exception ex); 
    public static class InternalErrorHandler
    {
        public static event InternalError onErrorHandled;
        public static void Handle(Exception ex)
        {
            onErrorHandled?.Invoke(ex);
        }
    }
}
