using MasterServerToolkit.Bridges;
using MasterServerToolkit.MasterServer;

namespace MasterServerToolkit.Examples.BasicProfile
{
    public static class ProfileProperties
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile"></param>
        public static void Fill(ObservableProfile profile)
        {
            profile.Add(new ObservableString(ProfilePropertyOpCodes.displayName));
            profile.Add(new ObservableString(ProfilePropertyOpCodes.avatarUrl));
            profile.Add(new ObservableColor(ProfilePropertyOpCodes.tankColor));
            profile.Add(new ObservableInt(ProfilePropertyOpCodes.bronze));
            profile.Add(new ObservableInt(ProfilePropertyOpCodes.silver));
            profile.Add(new ObservableInt(ProfilePropertyOpCodes.gold));
            profile.Add(new ObservableDictStringInt(ProfilePropertyOpCodes.items));

            profile.Add(new ObservableInt(ProfilePropertyOpCodes.playerLevel));
            profile.Add(new ObservableInt(ProfilePropertyOpCodes.playerExp));
            profile.Add(new ObservableInt(ProfilePropertyOpCodes.needExp));
        }
    }
}