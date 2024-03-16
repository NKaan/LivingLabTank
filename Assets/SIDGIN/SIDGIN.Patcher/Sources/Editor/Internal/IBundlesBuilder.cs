using System.Collections.Generic;

namespace SIDGIN.Patcher.Editors
{
    internal interface IBundlesBuilder
    {
        IEnumerable<BundleData> GetBundles(BuildSettingsData settings);
    }
}
