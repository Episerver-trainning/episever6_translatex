using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using EPiServer;
using EPiServer.Core;
using EPiServer.PlugIn;
namespace EPiServer.Research.Translation4.UI
{
    [GuiPlugIn(DisplayName="Translation settings",Area=PlugInArea.SidSettingsArea,Description="Settings for translation service",Url="~/EPiServer.Research.Translation4/UI/TranslationUserSettings.ascx")]
    public partial class TranslationUserSettings : System.Web.UI.UserControl,EPiServer.PlugIn.ISidSettings
    {
        protected void Page_Load(object sender, EventArgs e)
        {            
        }
        
        #region ISidSettings Members

        public void LoadSettings(EPiServer.DataAbstraction.Sid sid, EPiServer.Personalization.PersonalizedData data)
        {
            if (!IsPostBack)
            {
                if (data[Core.Constants.USERPROPUSERACCOUNT] != null)
                {
                    userAccount.Text = data[Core.Constants.USERPROPUSERACCOUNT] as string;
                }
                if (data[Core.Constants.USERPROPUSERPASSWORD] != null)
                {
                    userPassword.Text = data[Core.Constants.USERPROPUSERPASSWORD] as string;
                }               
            }
        }

        
        public void SaveSettings(EPiServer.DataAbstraction.Sid sid, EPiServer.Personalization.PersonalizedData data)
        {
            if ( ((userAccount.Text != null) && (userAccount.Text != string.Empty)))
            {
                data[Core.Constants.USERPROPUSERACCOUNT] = userAccount.Text;
            }
            else
            {
                data[Core.Constants.USERPROPUSERACCOUNT] = string.Empty;
            }
            
            // TODO : validate user password 
            if ((userPassword.Text != null) && (userPassword.Text != string.Empty))
            {
                data[Core.Constants.USERPROPUSERPASSWORD] = userPassword.Text;
            }
            else
            {
                //data[Core.Constants.USERPROPUSERPASSWORD] = null;
            }
        }

        #endregion
    }
}