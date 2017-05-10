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
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Research.Translation4.Core;

using System.Globalization;
using EPiServer.FileSystem;
using EPiServer.Research.Translation4.Common;
using EPiServer.Web.Hosting;

namespace EPiServer.Research.Translation4.UI
{   
    public partial class EditProject : EPiServer.SimplePage
    {
        public EditProject()
            : base(EPiServer.Web.PageExtensions.CustomPageLink.OptionFlag, 0)
        { }

        public override void ValidatePageTemplate()
        {
            
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                sourceLanguage.DataSource = EPLangUtil.GetLanaguages();
                sourceLanguage.DataTextField = "Name";
                sourceLanguage.DataValueField = "Locale";
                sourceLanguage.DataBind();

                string currentLocale = LanguageBranch.Load(CurrentPage.LanguageBranch).LanguageID;
                sourceLanguage.Items.FindByValue(currentLocale).Selected = true;
                sourceLanguage.Enabled = false;
                projectName.Text = CurrentPage.PageName;
                targetLanguages.DataSource = EPLangUtil.GetLanaguages();
                targetLanguages.DataTextField = "Name";
                targetLanguages.DataValueField = "Locale";
                targetLanguages.DataBind();
                ListItem li = targetLanguages.Items.FindByValue(sourceLanguage.SelectedValue);
                if (li != null)
                    targetLanguages.Items.Remove(li);

                ddlConnector.DataSource = Manager.Current.Connectors;
                ddlConnector.DataTextField = "Name";
                ddlConnector.DataValueField = "Name";
                ddlConnector.DataBind();
            }
            else
            {
                string laststep = Manager.Current.GetConntectorDefinitionByName(ddlConnector.SelectedValue).ControlToCreatingLaststep;
                if (laststep != null)
                {                    
                    customerStep.UserControlString = laststep;                    
                }                
            }
        }      
        
        private void setParentsStatus(string sessionID, string pageID, int status, ref DataSet ds)
        {
            setPageStatus(sessionID, pageID, status);
            DataRow[] drs = ds.Tables[0].Select("page_link='" + pageID + "'");
            DataRow[] drparent = ds.Tables[0].Select("page_link like '" + drs[0]["parent_link"] as string + "[_]%'");
            if (drparent.Length > 0)
                setParentsStatus(sessionID, drparent[0]["page_link"] as string, status, ref ds);
        }

        private void setChildrenStatus(string sessionID, string pageID, int status, ref DataSet ds)
        {
            DataRow[] drs = ds.Tables[0].Select("parent_link ='" + pageID + "'");
            foreach (DataRow dr in drs)
            {
                string currentPageID = (dr["page_link"] as string).Split(new char[] { '_' })[0];
                setPageStatus(sessionID, dr["page_link"] as string, status);
                setChildrenStatus(sessionID, currentPageID, status, ref ds);
            }
        }

      
        protected void changePageStatus(object sender, CommandEventArgs e)
        {
            string sessionID = SessionID;
            string selectedPageID = e.CommandArgument as string;
            DataSet ds = Manager.Current.GetPageList(sessionID);

            DataRow[] drs = ds.Tables[0].Select("page_link='" + selectedPageID + "'");

            int status = (int)(drs[0]["status"]);

            // selected 
            // gray out parents and clear children.
            // set status 1
            if ((status == (int)(ProjectItemStatus.Send)))
            {
                setPageStatus(sessionID, selectedPageID, (int)ProjectItemStatus.NoSend);
                DataRow[] drparent = ds.Tables[0].Select("page_link like '" + drs[0]["parent_link"] as string + "[_]%'");
                if (drparent.Length > 0)
                {
                    setParentsStatus(sessionID, drparent[0]["page_link"] as string, (int)ProjectItemStatus.Send, ref ds);
                }
                setChildrenStatus(sessionID, selectedPageID.Split(new char[] { '_' })[0], (int)ProjectItemStatus.NoSend, ref ds);
            }
            else
            {
                // unselected 
                // make it to status 3 and select on children.
                setPageStatus(sessionID, selectedPageID, (int)ProjectItemStatus.Send);
                setChildrenStatus(sessionID, selectedPageID.Split(new char[] { '_' })[0], (int)ProjectItemStatus.Send, ref ds);
                //if (status == 0)
                //{
                //}
                //else
                //{
                //    // gray out
                //    //
                //    // make it to status 0
                //    // delete and gray out parent and clear 
                //}
            }
            pagesToTranslate.DataSource = Manager.Current.GetPageList(sessionID);
            pagesToTranslate.DataBind();

        }
        protected DataTable GetPageFiles(string pagelink,int indent)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("filename");
            dt.Columns.Add("filepath");
            dt.Columns.Add("indent");
            dt.Columns.Add("status");

            
            UnifiedDirectory ud = DataFactory.Instance.GetPage(PageReference.Parse(pagelink)).GetPageDirectory(false);
            if (ud != null)
            {
                foreach (UnifiedFile uf in ud.GetFiles())
                {
                    DataRow dr = dt.NewRow();
                    dr["filename"] = uf.Name;
                    dr["filepath"] = uf.VirtualPath;
                    dr["indent"] = indent;
                    if (!IsFileLoaded)
                    {
                        dr["status"] = (int)ProjectItemStatus.NoSend;
                    }
                    else
                    {
                        dr["status"] = (int)Manager.Current.GetFileStatus(SessionID, uf.VirtualPath);
                    }
                    dt.Rows.Add(dr);
                    if (!IsFileLoaded)
                    {
                        insertIntoDatabase(PageReference.Parse(pagelink), PageReference.EmptyReference, indent, uf.VirtualPath, SessionID, ProjectItemStatus.NoSend, ItemType.File);
                    }
                }
            }
            return dt;
            
        }
        protected void changeFileStatus(object sender, CommandEventArgs e)
        {
            string sessionID = SessionID;
            string selectedFilepath = e.CommandArgument as string;
            ProjectItemStatus pis = Manager.Current.GetFileStatus(sessionID, selectedFilepath);
            if (pis==ProjectItemStatus.NoSend)
                Manager.Current.SetFileStatus(sessionID, selectedFilepath, ProjectItemStatus.Send);
            else
                Manager.Current.SetFileStatus(sessionID, selectedFilepath, ProjectItemStatus.NoSend);

            filesToTranslate.DataSource = Manager.Current.GetPageList(SessionID);
            filesToTranslate.DataBind();
        }

        protected void CancelOperation(object sender, CommandEventArgs e)
        {
            closeWinPanel.Visible = true;
        }
        protected void WizardFinished(object sender, EventArgs e)
        {
            if (ViewState["laststepcontrol"] != null)
            {
                customerStep.UserControlString = ViewState["laststepcontrol"] as string;
            }
            EnsureChildControls();

            string sessionID = ViewState[Core.Constants.VIEWSTATSESSIONID] as string;
            Common.TranslationStatus status = Common.TranslationStatus.Created;
            string connectorName = ddlConnector.SelectedValue;

            int projectid = Manager.Current.CreateProject(projectName.Text, sessionID, connectorName, "", sourceLanguage.SelectedValue, status);
            Common.TranslationProject tproj = Manager.Current.GetTranslationProject(projectid);
            
            foreach (ListItem item in targetLanguages.Items)
            {
                if (item.Selected)
                {
                    string lang = item.Value;//Manager.Current.ToValidLocale(item.Value);
                    Manager.Current.InsertTargetLanguage(projectid, lang);
                    foreach (Common.TranslationPage tp in tproj.Pages)
                    {
                        tp.SetStatus(lang, tproj.Status);
                    }
                    foreach (TranslationFile tf in tproj.Files)
                    {
                        tf.SetStatus(lang, tproj.Status);
                    }
                }
            }
            ICustomerStep iccs = customerStep.InnerControl as ICustomerStep;
            if ( iccs != null)
            {
                iccs.Save(tproj);
            }            
        }

        
        private string SessionID
        {
            get
            {
                string ret = string.Empty;
                
                ret = ViewState[Core.Constants.VIEWSTATSESSIONID] as string;
                if (ret == null)
                    ret = string.Empty;
                
                return ret;
            }
            set
            {
                ViewState[Core.Constants.VIEWSTATSESSIONID] = value;
            }
        }
        private bool IsPageLoaded
        {
            get
            {
                string ret = ViewState[Core.Constants.VIEWSTATISPAGELOADED] as string;
                if (ret == null)
                    return false;
                else
                    return true;
            }
            set
            {
                ViewState[Core.Constants.VIEWSTATISPAGELOADED] = value.ToString();
            }
        }
        private bool IsFileLoaded
        {
            get
            {
                string ret = ViewState[Core.Constants.VIEWSTATISFILELOADED] as string;
                if (ret == null)
                    return false;
                else
                    return true;
            }
            set
            {
                ViewState[Core.Constants.VIEWSTATISFILELOADED] = value.ToString();
            }
        }
        protected void StepChanged(object sender, EventArgs e)
        {
            if (Wizard1.ActiveStep == ProjectPages)
            {
                string sessionID = SessionID;
                if (sessionID == string.Empty)
                {
                    sessionID = Guid.NewGuid().ToString();
                    SessionID = sessionID;
                    PageDataCollection pages = new PageDataCollection();
                    /*
                     * CurrentPage.Indent = 1;
                     */
                    pages.Add(CurrentPage);
                    insertIntoDatabase(CurrentPage.PageLink, CurrentPage.ParentLink, 1, CurrentPage.PageName, sessionID,ProjectItemStatus.Send,ItemType.Page);
                    if (includeChildren.Checked)
                    {
                        getSubPagesWithLanguage(CurrentPage.PageLink, new EPiServer.Core.LanguageSelector(CurrentPage.LanguageBranch), 2, sessionID, ref pages);
                    }
                    IsPageLoaded = true;
                    pagesToTranslate.DataSource = Manager.Current.GetPageList(sessionID);
                    pagesToTranslate.DataBind(); 
                }
                               
            }
            else
            {
                if (Wizard1.ActiveStep == ProjectFiles)
                {
                    string sessionID = SessionID;
                    if (!IsFileLoaded)
                    {

                        filesToTranslate.DataSource = Manager.Current.GetPageList(sessionID);
                        filesToTranslate.DataBind();
                        IsFileLoaded = true;
                    }
                }
                else
                {

                    //if (Wizard1.ActiveStep != ProjectSetting)
                    if (Wizard1.ActiveStep == customerStep)
                    {
                        if (ViewState["laststepcontrol"] == null)
                        {

                        }
                    }
                    else
                    {
                        if (Wizard1.ActiveStep == summarystep)
                        {
                            string sessionID = SessionID;
                            numberofpages.Text = Manager.Current.GetPagesCount(sessionID).ToString();
                            numberoffiles.Text = Manager.Current.GetFilesCount(sessionID).ToString();
                        }
                    }
                }

            }
        }

        private void setPageStatus(string sessionID, string pageID, int status)
        {
            Manager.Current.SetPageStatus(sessionID, pageID, status);
        }

        private void getSubPagesWithLanguage(PageReference parent, ILanguageSelector langSelector, int indent, string sessionID, ref PageDataCollection retPages)
        {
            PageDataCollection pages = DataFactory.Instance.GetChildren(parent,langSelector);

            foreach (PageData page in pages)
            {                
                PageVersion pv = PageVersion.LoadPublishedVersion(page.PageLink, page.LanguageBranch);
                if (pv == null)
                    continue;
                /*
                 * page.Indent = indent;
                 */ 
                retPages.Add(page);
                insertIntoDatabase(pv.ID, page.ParentLink, indent, page.PageName, sessionID,ProjectItemStatus.Send,ItemType.Page);
                getSubPagesWithLanguage(page.PageLink, langSelector, indent + 1, sessionID, ref retPages);
            }
        }
        private void insertIntoDatabase(PageReference pageLink, PageReference parentLink, int indent, string pageName, string sessionID, ProjectItemStatus status, ItemType itemtype)
        {
            Manager.Current.InsertItemIntoDatabase(pageLink, parentLink, indent, pageName, sessionID, status, itemtype);
        }

    }
}
