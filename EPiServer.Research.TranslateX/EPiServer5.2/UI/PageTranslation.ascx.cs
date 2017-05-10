using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using EPiServer.Core;
using EPiServer.PlugIn;

using EPiServer.DataAbstraction;
using EPiServer.Research.Translation4.Core;
namespace EPiServer.Research.Translation4.UI
{
    [GuiPlugIn(Area=PlugInArea.EditPanel,
                Description="Translate tasks",
                DisplayName="Translation",                
                //RequiredAccessLevel=EPiServer.Security.AccessLevel.Administer,
                Url="~/EPiServer.Research.Translation4/UI/PageTranslation.ascx")]
    
    public partial class PagePicker : EPiServer.UserControlBase,PlugIn.ICustomPlugInLoader  
    {        
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                existingProjects.DataSource = Manager.Current.GetRelatedProjects(CurrentPage.PageLink.ID);
                existingProjects.DataBind();
            }
        }
        protected string GetLocalStatus(int status)
        {
            return ((Common.TranslationStatus)status).ToString();
        }

        protected void delProject(object sender, CommandEventArgs e)
        {
            Manager.Current.RemoveProject(int.Parse(e.CommandArgument as string));
            existingProjects.DataSource = Manager.Current.GetRelatedProjects(CurrentPage.PageLink.ID);
            existingProjects.DataBind();
        }

        protected string GetTargetLanguages(int projectid)
        {
            return Manager.Current.GetProjectTargetLanguages(projectid);
        }
        #region ICustomPlugInLoader Members

        public PlugInDescriptor[] List()
        {
            //if ( (EPiServer.Security.UnifiedPrincipal.Current.UserData[Core.Constants.USERPROPUSERACCOUNT] != null) && (EPiServer.Security.UnifiedPrincipal.Current.UserData[Core.Constants.USERPROPUSERACCOUNT] as string != string.Empty))
            {
                
                return new PlugInDescriptor[] { new PlugInDescriptor(1,typeof(PagePicker))};
            }
            //return new PlugInDescriptor[] {};
        }

        #endregion
    }
}
