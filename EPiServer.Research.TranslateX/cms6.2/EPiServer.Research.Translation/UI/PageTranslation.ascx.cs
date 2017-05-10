using System;
using System.Web.UI.WebControls;
using EPiServer.PlugIn;
using EPiServer.Research.Translation.Core;

namespace EPiServer.Research.Translation.UI
{
    [GuiPlugIn(Area = PlugInArea.EditPanel,
                Description = "Translate tasks",
                DisplayName = "Translation",
                Url = "~/EPiServer.Research.Translation/UI/PageTranslation.ascx")]
    public partial class PagePicker : UserControlBase, ICustomPlugInLoader
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!IsPostBack)
            {
                var source = Manager.Current.GetRelatedProjects(CurrentPage.PageLink.ID);
                
                if (source != null)
                {
                    existingProjects.DataSource = source;
                }
                existingProjects.DataBind();
            }
        }
        protected string GetLocalStatus(int status)
        {
            return ((Translation4.Common.TranslationStatus)status).ToString();
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

        public PlugInDescriptor[] List()
        {
            return new[] { new PlugInDescriptor(1, typeof(PagePicker)) };
        }
    }
}
