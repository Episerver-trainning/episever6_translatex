using EPiServer.Research.Translation4.Common;
namespace EPiServer.Research.Connector.Language.XLIFFConnector
{
    public partial class XLiffWizardLastStep : System.Web.UI.UserControl, ICustomerStep
    {
        public void Save(TranslationProject project)
        {
            project.Properties["xliffemail"] = notificationMail2.Text;
            project.Status = TranslationStatus.ReadyForSend;
            project.Save();
        }
    }
}