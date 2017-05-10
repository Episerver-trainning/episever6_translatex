using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using EPiServer.Research.Translation4.Common;
namespace EPiServer.Research.Connector.Language.XLIFFConnector
{
    public partial class XLiffWizardLastStep : System.Web.UI.UserControl, ICustomerStep
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //Response.Write("ONLOAD");
            //Response.Write("noemail:" + notificationMail.Text);            
        }

        #region ICustomerCreatingStep Members
       

        public void Save(TranslationProject project)
        {
            project.Properties["xliffemail"] = notificationMail2.Text;
            project.Status = TranslationStatus.ReadyForSend;
            project.Save();
        }

        #endregion
    }
}