﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34209
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BTVSchedule.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "11.0.0.0")]
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
        [global::System.Configuration.DefaultSettingValueAttribute("http://172.16.1.178:8129/wsdl/BTVScheduler.asmx")]
        public string BTVSchedule_BTVScheduler_BTVScheduler {
            get {
                return ((string)(this["BTVSchedule_BTVScheduler_BTVScheduler"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.WebServiceUrl)]
        [global::System.Configuration.DefaultSettingValueAttribute("http://172.16.1.178:8129/wsdl/BTVLicenseManager.asmx")]
        public string BTVSchedule_BTVLicenseManager_BTVLicenseManager {
            get {
                return ((string)(this["BTVSchedule_BTVLicenseManager_BTVLicenseManager"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.WebServiceUrl)]
        [global::System.Configuration.DefaultSettingValueAttribute("http://172.16.1.178:8129/wsdl/BTVLibrary.asmx")]
        public string BTVSchedule_BTVLibrary_BTVLibrary {
            get {
                return ((string)(this["BTVSchedule_BTVLibrary_BTVLibrary"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.WebServiceUrl)]
        [global::System.Configuration.DefaultSettingValueAttribute("http://172.16.1.178:8129/wsdl/BTVGuideUpdater.asmx")]
        public string BTVSchedule_BTVGuideUpdater_BTVGuideUpdater {
            get {
                return ((string)(this["BTVSchedule_BTVGuideUpdater_BTVGuideUpdater"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.WebServiceUrl)]
        [global::System.Configuration.DefaultSettingValueAttribute("http://172.16.1.178:8129/wsdl/BTVDispatcher.asmx")]
        public string BTVSchedule_BTVDispatcher_BTVDispatcher {
            get {
                return ((string)(this["BTVSchedule_BTVDispatcher_BTVDispatcher"]));
            }
        }
    }
}
