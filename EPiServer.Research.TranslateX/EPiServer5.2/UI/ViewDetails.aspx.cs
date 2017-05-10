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
using EPiServer.Research.Translation4.Common;
using EPiServer.Research.Translation4.Core;
using System.Collections.Generic;
using EPiServer.Web.PageExtensions;

namespace EPiServer.Research.Translation4.UI
{
    
    public partial class ViewDetails : EPiServer.SimplePage
    {
        //public ViewDetails(): base(ContextMenu.OptionFlag,0 )
        //{
        //}
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                localstatus.DataSource = Enum.GetNames(typeof(Common.TranslationStatus));
                localstatus.DataBind();
                ListItem li = localstatus.Items.FindByText(((Common.TranslationStatus)(CurrentProject.Status)).ToString());
                if (li != null)
                    li.Selected = true;
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
            CurrentProject.Status = (Common.TranslationStatus)Enum.Parse(typeof(Common.TranslationStatus), localstatus.SelectedValue);
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
