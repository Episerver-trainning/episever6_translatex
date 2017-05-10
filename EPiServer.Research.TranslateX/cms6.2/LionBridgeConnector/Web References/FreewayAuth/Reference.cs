﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.17929
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// 
// This source code was auto-generated by Microsoft.VSDesigner, Version 4.0.30319.17929.
// 
#pragma warning disable 1591

namespace EPiServer.Research.Connector.Language.LionBridge.FreewayAuth {
    using System;
    using System.Web.Services;
    using System.Diagnostics;
    using System.Web.Services.Protocols;
    using System.Xml.Serialization;
    using System.ComponentModel;
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.17929")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Web.Services.WebServiceBindingAttribute(Name="FreewayAuthSoap", Namespace="http://tempuri.org/")]
    public partial class FreewayAuth : System.Web.Services.Protocols.SoapHttpClientProtocol {
        
        private System.Threading.SendOrPostCallback LogonOperationCompleted;
        
        private System.Threading.SendOrPostCallback CheckTicketExpiryOperationCompleted;
        
        private bool useDefaultCredentialsSetExplicitly;
        
        /// <remarks/>
        public FreewayAuth() {
            this.Url = global::EPiServer.Research.Connector.Language.LionBridge.Properties.Settings.Default.EPiServer_Research_Connector_Language_LionBridge_FreewayAuth_FreewayAuth;
            if ((this.IsLocalFileSystemWebService(this.Url) == true)) {
                this.UseDefaultCredentials = true;
                this.useDefaultCredentialsSetExplicitly = false;
            }
            else {
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }
        
        public new string Url {
            get {
                return base.Url;
            }
            set {
                if ((((this.IsLocalFileSystemWebService(base.Url) == true) 
                            && (this.useDefaultCredentialsSetExplicitly == false)) 
                            && (this.IsLocalFileSystemWebService(value) == false))) {
                    base.UseDefaultCredentials = false;
                }
                base.Url = value;
            }
        }
        
        public new bool UseDefaultCredentials {
            get {
                return base.UseDefaultCredentials;
            }
            set {
                base.UseDefaultCredentials = value;
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }
        
        /// <remarks/>
        public event LogonCompletedEventHandler LogonCompleted;
        
        /// <remarks/>
        public event CheckTicketExpiryCompletedEventHandler CheckTicketExpiryCompleted;
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://tempuri.org/Logon", RequestNamespace="http://tempuri.org/", ResponseNamespace="http://tempuri.org/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string Logon(string Username, string Password) {
            object[] results = this.Invoke("Logon", new object[] {
                        Username,
                        Password});
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public void LogonAsync(string Username, string Password) {
            this.LogonAsync(Username, Password, null);
        }
        
        /// <remarks/>
        public void LogonAsync(string Username, string Password, object userState) {
            if ((this.LogonOperationCompleted == null)) {
                this.LogonOperationCompleted = new System.Threading.SendOrPostCallback(this.OnLogonOperationCompleted);
            }
            this.InvokeAsync("Logon", new object[] {
                        Username,
                        Password}, this.LogonOperationCompleted, userState);
        }
        
        private void OnLogonOperationCompleted(object arg) {
            if ((this.LogonCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.LogonCompleted(this, new LogonCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://tempuri.org/CheckTicketExpiry", RequestNamespace="http://tempuri.org/", ResponseNamespace="http://tempuri.org/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public int CheckTicketExpiry(string Ticket) {
            object[] results = this.Invoke("CheckTicketExpiry", new object[] {
                        Ticket});
            return ((int)(results[0]));
        }
        
        /// <remarks/>
        public void CheckTicketExpiryAsync(string Ticket) {
            this.CheckTicketExpiryAsync(Ticket, null);
        }
        
        /// <remarks/>
        public void CheckTicketExpiryAsync(string Ticket, object userState) {
            if ((this.CheckTicketExpiryOperationCompleted == null)) {
                this.CheckTicketExpiryOperationCompleted = new System.Threading.SendOrPostCallback(this.OnCheckTicketExpiryOperationCompleted);
            }
            this.InvokeAsync("CheckTicketExpiry", new object[] {
                        Ticket}, this.CheckTicketExpiryOperationCompleted, userState);
        }
        
        private void OnCheckTicketExpiryOperationCompleted(object arg) {
            if ((this.CheckTicketExpiryCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.CheckTicketExpiryCompleted(this, new CheckTicketExpiryCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        public new void CancelAsync(object userState) {
            base.CancelAsync(userState);
        }
        
        private bool IsLocalFileSystemWebService(string url) {
            if (((url == null) 
                        || (url == string.Empty))) {
                return false;
            }
            System.Uri wsUri = new System.Uri(url);
            if (((wsUri.Port >= 1024) 
                        && (string.Compare(wsUri.Host, "localHost", System.StringComparison.OrdinalIgnoreCase) == 0))) {
                return true;
            }
            return false;
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.17929")]
    public delegate void LogonCompletedEventHandler(object sender, LogonCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.17929")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class LogonCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal LogonCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public string Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((string)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.17929")]
    public delegate void CheckTicketExpiryCompletedEventHandler(object sender, CheckTicketExpiryCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.17929")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class CheckTicketExpiryCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal CheckTicketExpiryCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public int Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((int)(this.results[0]));
            }
        }
    }
}

#pragma warning restore 1591