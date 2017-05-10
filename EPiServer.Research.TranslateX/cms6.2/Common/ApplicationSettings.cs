using System;
using System.Web.UI.WebControls;
using EPiServer.Events;
using EPiServer.Events.Clients;
using EPiServer.PlugIn;

namespace EPiServer.Research.Translation4.Common
{
    [GuiPlugIn(Area = PlugInArea.None, DisplayName = "TranslateX Settings")]
    public sealed class ApplicationSettings
    {
        private static ApplicationSettings _instance;
        internal static Guid _broadcastSettingsChangedEventId = new Guid("CF7A2C90-45F4-4059-AADB-8F75D6188201");

        private string _translationConnector = "EPiServer.Research.Connector.Language.XLIFF.Connector,EPiServer.Research.Connector.Language.XLIFFConnector";
        private string _translationWs = "https://freeway.demo.lionbridge.com/vojo/service.asmx";
        private string _translationAuthenticationWs = "https://freeway.demo.lionbridge.com/vojo/FreewayAuth.asmx";
        private string _xliffworkpath = "C:\temp\translations";

        [PlugInProperty(Description = "TranslationConnector", AdminControl = typeof(TextBox), AdminControlValue = "Text")]
        public string TranslationConnector
        {
            get { return _translationConnector; }
            set { _translationConnector = value; }
        }
        
        [PlugInProperty(Description = "URL to Lionbridge service", AdminControl = typeof(TextBox), AdminControlValue = "Text")]
        public string TranslationWs { get { return _translationWs; } set { _translationWs = value; } }
        
        [PlugInProperty(Description = "URL to lionbridge FreewayAuth Service", AdminControl = typeof(TextBox), AdminControlValue = "Text")]
        public string TranslationAuthenticationWs { get { return _translationAuthenticationWs; } set { _translationAuthenticationWs = value; } }

        [PlugInProperty(Description = "username", AdminControl = typeof(TextBox), AdminControlValue = "Text")]
        public string TranslationUserName { get; set; }

        [PlugInProperty(Description = "password", AdminControl = typeof(TextBox), AdminControlValue = "Text")]
        public string TranslationPassword { get; set; }
        
        [PlugInProperty(Description = "Filepath to translation temp folder", AdminControl = typeof(TextBox), AdminControlValue = "Text")]
        public string Xliffworkpath { get { return _xliffworkpath; } set { _xliffworkpath = value; } }

        private ApplicationSettings()
        {
            PlugInSettings.SettingsChanged += PlugInSettingsSettingsChanged;
            Event broadcastSettingsChangedEvent = Event.Get(_broadcastSettingsChangedEventId);
            broadcastSettingsChangedEvent.Raised += BroadcastSettingsChangedEvent_Raised;
        }

        private static void PlugInSettingsSettingsChanged(object sender, EventArgs e)
        {
            //Broadcast event to all servers
            Event settingsChangedEvent = Event.Get(_broadcastSettingsChangedEventId);
            settingsChangedEvent.Raise(_broadcastSettingsChangedEventId, null);
        }

        private void BroadcastSettingsChangedEvent_Raised(object sender, EventNotificationEventArgs e)
        {
            _instance = null;
        }

        public static ApplicationSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ApplicationSettings();
                    PlugInSettings.AutoPopulate(_instance);
                }
                return _instance;
            }
        }
    }
}
