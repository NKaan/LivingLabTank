using System.Collections.Generic;
namespace SIDGIN.Common
{
    public interface ICollectionSet<TItem>
    {
        void SetCollection(List<TItem> data);
    }
}
