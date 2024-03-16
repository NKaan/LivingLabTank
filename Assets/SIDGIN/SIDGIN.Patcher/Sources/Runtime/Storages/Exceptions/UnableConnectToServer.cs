using System;
using SIDGIN.Patcher.Client;
namespace SIDGIN.Patcher.Storages
{
    public class UnableConnectToServer : Exception
    {
        public UnableConnectToServer(Exception innerException = null) : base(Localize.Get("Unable_connect_server"), innerException)
        {

        }
    }
}
