using MasterServerToolkit.Bridges;
using MasterServerToolkit.MasterServer;
using MasterServerToolkit.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MasterServerToolkit.Examples.BasicProfile
{
    public class ProfileView : UIView
    {
        #region INSPECTOR

        [Header("Player"), SerializeField]
        private AvatarComponent avatar;
        [SerializeField]
        private UIProperty displayNameUIProperty;

        [Header("Currencies"), SerializeField]
        private UIProperty bronzeUIProperty;
        [SerializeField]
        private UIProperty silverUIProperty;
        [SerializeField]
        private UIProperty goldUIProperty;



        public Slider playerExpSlider;
        public TextMeshProUGUI playerLevelTxt;
        public TextMeshProUGUI playerOwnExpTxt;
        public TextMeshProUGUI playerExpPercentageTxt;

        #endregion

        private ProfileLoaderBehaviour profileLoader;

        protected override void Start()
        {
            base.Start();

            profileLoader = FindObjectOfType<ProfileLoaderBehaviour>();
            profileLoader.OnProfileLoadedEvent.AddListener(OnProfileLoadedEventHandler);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (profileLoader && profileLoader.Profile != null)
                profileLoader.Profile.OnPropertyUpdatedEvent -= Profile_OnPropertyUpdatedEvent;
        }

        private void OnProfileLoadedEventHandler()
        {
            profileLoader.Profile.OnPropertyUpdatedEvent += Profile_OnPropertyUpdatedEvent;

            foreach (var property in profileLoader.Profile.Properties)
                Profile_OnPropertyUpdatedEvent(property.Key, property.Value);
        }

        private void Profile_OnPropertyUpdatedEvent(ushort key, IObservableProperty property)
        {
            if (key == ProfilePropertyOpCodes.displayName)
                displayNameUIProperty.Lable = property.As<ObservableString>().Value;
            else if (key == ProfilePropertyOpCodes.avatarUrl)
                avatar.SetAvatarUrl(property.Serialize());
            else if (key == ProfilePropertyOpCodes.bronze)
                bronzeUIProperty.SetValue(property.As<ObservableInt>().Value);
            else if (key == ProfilePropertyOpCodes.silver)
                silverUIProperty.SetValue(property.As<ObservableInt>().Value);
            else if (key == ProfilePropertyOpCodes.gold)
                goldUIProperty.SetValue(property.As<ObservableInt>().Value);
            else if (key == ProfilePropertyOpCodes.playerExp)
            {
                int playerExp = property.As<ObservableInt>().Value;

            }
            else if (key == ProfilePropertyOpCodes.playerLevel)
            {
                playerLevelTxt.text = "Lv." + property.As<ObservableInt>().Value.ToString();
            }
        }
    }
}