using System;
using SIDGIN.Patcher.Client;
namespace SIDGIN.Patcher.Storages
{
    public class UnableLoadResource : Exception
    {
        public UnableLoadResource(int code,string resourceName, Exception innerException = null) : base(Localize.Format("Unable_load_resource",resourceName, code.ToString()), innerException)
        {

        }
    }
}
