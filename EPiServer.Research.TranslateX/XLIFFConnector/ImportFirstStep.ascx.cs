using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using EPiServer.Research.Translation4.Common;
using System.IO;
using System.Configuration;

namespace EPiServer.Research.Connector.Language.XLIFFConnector
{
    public partial class ImportFirstStep : UserControl, ICustomerStep
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        #region ICustomerCreatingStep Members
        string tempDir = ConfigurationSettings.AppSettings["xliffworkpath"];
        public void Save(TranslationProject project)
        {
            if (file != null)
            {
                if (file.FileBytes.Length > 0)
                {
                    string workpath = Path.Combine(tempDir, "incoming\\" + project.RemoteID);
                    string zippath = Path.Combine(tempDir, "incoming\\" + project.RemoteID + ".zip");

                    FileStream f = new FileStream(zippath, FileMode.Create);
                    f.Write(file.FileBytes, 0, file.FileBytes.Length);
                    f.Close();
                    project.Status = TranslationStatus.Importing;
                    project.Save();
                    project.Connector.RetrieveProject(project);
                }
            }
        }

        #endregion
    }
}