using System;
using EPiServer.Research.Translation4.Common;

namespace EPiServer.Research.Connector.Language.LionBridge
{
    public partial class WizardLastStep : System.Web.UI.UserControl, ICustomerStep
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        
            if (ddltasks.Items.Count == 0)
            { 
                Connector conn = new Connector();

                ddltasks.DataSource = conn.GetTasks();
                ddltasks.DataTextField = "Description";
                ddltasks.DataValueField = "ID";
                ddltasks.DataBind();
                ddltasks.Items.Insert(0,"");
            }
        }
        protected void taskchanged(object sender, EventArgs e)
        {
            if (ddltasks.SelectedValue != "")
            {
                Connector conn = new Connector();

                ddlsubtasks.DataSource = conn.GetSubTasks(ddltasks.SelectedValue);
                ddlsubtasks.DataTextField = "Description";
                ddlsubtasks.DataValueField = "ID";

                ddlsubtasks.DataBind();
                ddlsubtasks.Items.Insert(0, "");

                ddluoms.DataSource = conn.GetUOM(ddltasks.SelectedValue);
                ddluoms.DataTextField = "Description";
                ddluoms.DataValueField = "ID";

                ddluoms.DataBind();
                ddluoms.Items.Insert(0, "");
            }
            else {
                ddlsubtasks.Items.Clear();
                ddluoms.Items.Clear();
            }
        }
      
        public void Save(TranslationProject project)
        {
            project.Properties["projectstartdate"] = tbstartdate.Text;
            project.Properties["projectdeliverydate"] = tbdeliverydate.Text;
            project.Properties["notificationmail"] = tbnotificationmail.Text;
            project.Properties["TaskID"] = ddltasks.SelectedValue;
            project.Properties["SubtaskID"] = ddlsubtasks.SelectedValue;
            project.Properties["UomID"] = ddluoms.SelectedValue;
            project.Status = TranslationStatus.ReadyForSend;
            project.Save();
        }
    }
}