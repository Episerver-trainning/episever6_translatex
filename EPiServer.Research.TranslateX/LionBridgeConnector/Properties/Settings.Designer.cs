﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.1434
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EPiServer.Research.Connector.Language.LionBridge.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "9.0.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.WebServiceUrl)]
        [global::System.Configuration.DefaultSettingValueAttribute("https://freeway.demo.lionbridge.com/vojo/FreewayAuth.asmx")]
        public string EPiServer_Research_Connector_Language_LionBridge_FreewayAuth_FreewayAuth {
            get {
                return ((string)(this["EPiServer_Research_Connector_Language_LionBridge_FreewayAuth_FreewayAuth"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.WebServiceUrl)]
        [global::System.Configuration.DefaultSettingValueAttribute("https://freeway.demo.lionbridge.com/vojo/service.asmx")]
        public string EPiServer_Research_Connector_Language_LionBridge_FreewayWS_Vojo {
            get {
                return ((string)(this["EPiServer_Research_Connector_Language_LionBridge_FreewayWS_Vojo"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.WebServiceUrl)]
        [global::System.Configuration.DefaultSettingValueAttribute("https://freeway.demo.lionbridge.com/vojo/EvaluationHelper.asmx")]
        public string EPiServer_Research_Connector_Language_LionBridge_FreewayEval_EvaluationHelper {
            get {
                return ((string)(this["EPiServer_Research_Connector_Language_LionBridge_FreewayEval_EvaluationHelper"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.ConnectionString)]
        [global::System.Configuration.DefaultSettingValueAttribute("Data Source=rj4\\sqlexpress;Initial Catalog=dbEPiServerCMSR1;User ID=cmsR1")]
        public string dbEPiServerCMSR1ConnectionString {
            get {
                return ((string)(this["dbEPiServerCMSR1ConnectionString"]));
            }
        }
    }
}