using System.Web.UI;
using EPiServer.Research.Translation4.Common;
using System.IO;
using System.Configuration;

namespace EPiServer.Research.Connector.Language.XLIFFConnector
{
    public partial class ImportFirstStep : UserControl, ICustomerStep
    {
        private readonly string _tempDir = ApplicationSettings.Instance.Xliffworkpath; //ConfigurationManager.AppSettings["xliffworkpath"];
        public void Save(TranslationProject project)
        {
            if (file != null)
            {
                if (file.FileBytes.Length > 0)
                {
                    string zippath = Path.Combine(_tempDir, "incoming\\" + project.RemoteID + ".zip");

                    FileStream f = new FileStream(zippath, FileMode.Create);
                    f.Write(file.FileBytes, 0, file.FileBytes.Length);
                    f.Close();
                    project.Status = TranslationStatus.Importing;
                    project.Save();
                    project.Connector.RetrieveProject(project);
                }
            }
        }
    }
}