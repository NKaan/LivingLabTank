using UnityEngine;
namespace SIDGIN.Common
{
    public abstract class ISettings<T> : ScriptableObject where T : ISettings<T>
    {
        static T settings;
        public static T Settings
        {
            get
            {
                settings = Load();
                return settings;
            }
        }
        static T Load()
        {
            return Resources.Load<T>(typeof(T).Name);
        }
    }
}