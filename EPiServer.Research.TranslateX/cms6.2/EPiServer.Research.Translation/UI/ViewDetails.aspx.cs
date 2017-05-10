using System;
using System.Web.UI.WebControls;
using EPiServer.Research.Translation.Core;
using EPiServer.Research.Translation4.Common;
using System.Collections.Generic;

namespace EPiServer.Research.Translation.UI
{
    public partial class ViewDetails : SimplePage
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        
            if (!IsPostBack)
            {
                localstatus.DataSource = Enum.GetNames(typeof(TranslationStatus));
                localstatus.DataBind();
                ListItem li = localstatus.Items.FindByText(((TranslationStatus)(CurrentProject.Status)).ToString());
                if (li != null)
                {
                    li.Selected = true;
                }
                dlPages.DataSource = CurrentProject.Pages;
                dlPages.DataBind();
                targetLangs.DataBind();
            }

                      
        }

        protected string GetFilename(string filepath)
        {
            return System.IO.Path.GetFileName(filepath);
        }

        protected void ok_click(object sender, EventArgs e)
        {
            CurrentProject.Status = (TranslationStatus)Enum.Parse(typeof(TranslationStatus), localstatus.SelectedValue);
            CurrentProject.Modified = true;
            CurrentProject.Save();
            foreach (TranslationPage tp in CurrentProject.Pages)
            {
                foreach (string lang in CurrentProject.TargetLanguages)
                    tp.SetStatus(lang, (TranslationStatus)CurrentProject.Status);
            }
            foreach (TranslationFile file in CurrentProject.Files)
            {
                foreach (string lang in CurrentProject.TargetLanguages)
                    file.SetStatus(lang, (TranslationStatus)CurrentProject.Status);
            }
            dlPages.DataSource = CurrentProject.Pages;
            dlPages.DataBind();            
        }

        private TranslationProject _currentProject = null;
        protected TranslationProject CurrentProject
        {
            get
            {
                if (_currentProject == null)
                {
                    int projectid = int.Parse(Request.QueryString["projectid"]);
                    _currentProject = Manager.Current.GetTranslationProject(projectid);
                    _currentProject.Connector = Manager.Current.GetConnectors()[0];                    
                }
                return _currentProject;
            }
        }

        protected List<TranslationFile> getFileList(TranslationPage page)
        {
            List<TranslationFile> ret = new List<TranslationFile>();
            foreach (TranslationFile file in CurrentProject.Files)
            {
                if (file.PageLink == page.OriginalID)
                {
                    if (file.Status==(int)ProjectItemStatus.Send)
                        ret.Add(file);
                }
            }
            return ret;
        }
        protected List<string> GetPageLanguageStatus(TranslationPage page)
        {
            List<string> langs = CurrentProject.TargetLanguages;
            List<string> status = new List<string>();
            foreach (string lang in langs)
            {
                status.Add(page.GetStatus(lang).ToString());
                status.Add(page.GetRemoteStatus(lang) ?? "");
            }
            return status;

        }
        protected List<string> GetFileLanguageStatus(TranslationFile file)
        {
            List<string> langs = CurrentProject.TargetLanguages;
            List<string> status = new List<string>();
            foreach (string lang in langs)
            {
                status.Add(file.GetStatus(lang).ToString());
                status.Add(file.GetRemoteStatus(lang)??"");
            }
            return status;
        }

      
        public override void ValidatePageTemplate()
        {
            return;
        }
    }
}
