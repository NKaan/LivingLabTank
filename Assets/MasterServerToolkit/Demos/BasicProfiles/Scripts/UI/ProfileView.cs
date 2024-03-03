using MasterServerToolkit.Bridges;
using MasterServerToolkit.MasterServer;
using MasterServerToolkit.Networking;
using MasterServerToolkit.UI;
using System;
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

        [Header("Stats")]
        public TextMeshProUGUI myStatPoint;

        public Button plusDamageBtn;
        public Button negDamageBtn;
        public TextMeshProUGUI damageTxt;

        public Button plusSpeedBtn;
        public Button negSpeedBtn;
        public TextMeshProUGUI speedTxt;

        public Button plusHealthBtn;
        public Button negHealthBtn;
        public TextMeshProUGUI healtTxt;

        public Button plusFuelBtn;
        public Button negFuelBtn;
        public TextMeshProUGUI fuelTxt;

        

        [Header("Profil")]
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

            float playerLevel = profileLoader.Profile.Properties[ProfilePropertyOpCodes.playerLevel].As<ObservableInt>().Value;
            float playerExp = profileLoader.Profile.Properties[ProfilePropertyOpCodes.playerExp].As<ObservableInt>().Value;
            float needExp = NeedExpCalculate((int)playerLevel);

            playerLevelTxt.text = "Lv." + playerLevel.ToString();
            playerOwnExpTxt.text = playerExp.ToString("N0") + "/" + needExp.ToString("N0");
            playerExpPercentageTxt.text = "%" + ((playerExp / needExp) * 100f).ToString("N1");

            playerExpSlider.maxValue = needExp;
            playerExpSlider.value = playerExp;


        }

        public float NeedExpCalculate(int playerLevel)
        {
            return (float)(100 * Math.Pow(1.1, playerLevel - 1));
        }

        public void AddStat(int statID)
        {
            AddStats(statID,1);
        }

        public void RemoveStat(int statID)
        {
            AddStats(statID, -1);
        }

        public void AddStats(int statID,int addPoint)
        {
            if (profileLoader.Profile.Properties[ProfilePropertyOpCodes.usedStatPoint].As<ObservableInt>().Value + addPoint > profileLoader.Profile.Properties[ProfilePropertyOpCodes.playerLevel].As<ObservableInt>().Value)
            {
                return;
            }

            if (profileLoader.Profile.Properties[ProfilePropertyOpCodes.gold].As<ObservableInt>().Value < Math.Abs(addPoint) * 1000)
                return;

            if (statID == 1)
            {
                if (profileLoader.Profile.Properties[ProfilePropertyOpCodes.damageStatPoint].As<ObservableInt>().Value + addPoint < 0)
                    return;
            }
            else if (statID == 2)
            {
                if (profileLoader.Profile.Properties[ProfilePropertyOpCodes.speedStatPoint].As<ObservableInt>().Value + addPoint < 0)
                    return;
            }
            else if (statID == 3)
            {
                if (profileLoader.Profile.Properties[ProfilePropertyOpCodes.healthStatPoint].As<ObservableInt>().Value + addPoint < 0)
                    return;
            }
            else if (statID == 4)
            {
                if (profileLoader.Profile.Properties[ProfilePropertyOpCodes.fuelStatPoint].As<ObservableInt>().Value + addPoint < 0)
                    return;
            }

            MstProperties prop = new MstProperties();
            prop.Add("statID", statID);
            prop.Add("value", addPoint);

            Mst.Server.Rooms.Connection.SendMessage(MessageOpCodes.CMAddStatPoint, prop.ToBytes(), (succes, response) =>
            {
                Debug.Log(succes.ToString());
            });

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
            else if (key == ProfilePropertyOpCodes.damageStatPoint)
                damageTxt.text = property.As<ObservableInt>().Value.ToString();
            else if (key == ProfilePropertyOpCodes.speedStatPoint)
                speedTxt.text = property.As<ObservableInt>().Value.ToString();
            else if (key == ProfilePropertyOpCodes.healthStatPoint)
                healtTxt.text = property.As<ObservableInt>().Value.ToString();
            else if (key == ProfilePropertyOpCodes.fuelStatPoint)
                fuelTxt.text = property.As<ObservableInt>().Value.ToString();
            else if (key == ProfilePropertyOpCodes.usedStatPoint)
                myStatPoint.text = 
                    (profileLoader.Profile.Properties[ProfilePropertyOpCodes.playerLevel].As<ObservableInt>().Value - 
                    property.As<ObservableInt>().Value).ToString();

            else if (key == ProfilePropertyOpCodes.playerLevel)
            {
                playerLevelTxt.text = "Lv." + property.As<ObservableInt>().Value.ToString();
            }
        }
    }
}